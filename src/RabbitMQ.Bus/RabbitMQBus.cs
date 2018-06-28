using Newtonsoft.Json;
using RabbitMQ.Bus.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitMQBusService : IRabbitMQBus
    {
        private readonly RabbitMQBusFactory _factory;
        private readonly RabbitMQConfig _config;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<MessageContext> OnMessageReceived;
        #region Handlers and Queues
        private readonly Func<Type, Type[]> GetHandlers = new Func<Type, Type[]>((messageType) => AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IRabbitMQBusHandler<>).MakeGenericType(messageType)))).ToArray());
        private readonly Func<Type[]> GetQueues = new Func<Type[]>(() => AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof(QueueAttribute), true).Any())).ToArray());
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public RabbitMQBusService(RabbitMQConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _factory = new RabbitMQBusFactory
            {
                ConnectionFactory = new ConnectionFactory
                {
                    AutomaticRecoveryEnabled = config.AutomaticRecoveryEnabled,
                    NetworkRecoveryInterval = config.NetworkRecoveryInterval,
                    Uri = new Uri(config.ConnectionString)
                }
            };
            _factory.GetConnection = _factory.ConnectionFactory.CreateConnection(clientProvidedName: _config.ClientProvidedName);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        public void Subscribe<TMessage>() where TMessage : class
        {
            var queue = GetQueue(typeof(TMessage));
            IModel channel = CreateQueue(queue.ExchangeName, queue.QueueName, queue.RoutingKey);
            EventingBasicConsumer _consumer = new EventingBasicConsumer(channel);
            _consumer.Received += async (ch, ea) =>
            {
                var messageBody = Encoding.UTF8.GetString(ea.Body);
                TMessage message = JsonConvert.DeserializeObject<TMessage>(messageBody);
                foreach (var handleType in GetHandlers(typeof(TMessage)))
                {
                    if (OnMessageReceived != null)
                    {
                        OnMessageReceived?.Invoke(ea, new MessageContext(handleType, message, messageBody));
                    }
                    else
                    {
                        var handle = Activator.CreateInstance(handleType) as IRabbitMQBusHandler<TMessage>;
                        await handle.Handle(message);
                    }
                }
                channel.BasicAck(ea.DeliveryTag, false);
            };
            channel.BasicConsume(queue.QueueName, false, _consumer);
        }
        /// <summary>
        /// 自定订阅
        /// </summary>
        public void AutoSubscribe()
        {
            foreach (var messageType in GetQueues())
            {
                var queue = GetQueue(messageType);
                IModel channel = CreateQueue(queue.ExchangeName, queue.QueueName, queue.RoutingKey);
                EventingBasicConsumer _consumer = new EventingBasicConsumer(channel);
                _consumer.Received += (ch, ea) =>
                {
                    var messageBody = Encoding.UTF8.GetString(ea.Body);
                    var message = JsonConvert.DeserializeObject(messageBody, messageType);
                    foreach (var handleType in GetHandlers(messageType))
                    {
                        if (OnMessageReceived != null)
                        {
                            OnMessageReceived?.Invoke(ea, new MessageContext(handleType, message, messageBody));
                        }
                        else
                        {
                            var handle = Activator.CreateInstance(handleType);
                            var method = handleType.GetMethod(nameof(IRabbitMQBusHandler.Handle));
                            var task = (Task)method.Invoke(handle, new[] { message });
                            task.GetAwaiter().GetResult();
                        }
                    }
                    channel.BasicAck(ea.DeliveryTag, false);
                };
                channel.BasicConsume(queue.QueueName, false, _consumer);
            }
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="value">需要发送的消息</param>
        /// <param name="routingKey">路由Key</param>
        /// <param name="exchangeName">留空则使用默认的交换机</param>
        public Task Publish<TMessage>(TMessage value, string routingKey = "", string exchangeName = "")
        {
            var sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
            return Publish(sendBytes, routingKey, exchangeName);
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="sendBytes"></param>
        /// <param name="routingKey"></param>
        /// <param name="exchangeName"></param>
        public Task Publish(byte[] sendBytes, string routingKey = "", string exchangeName = "")
        {
            using (IModel channel = BindExchange(exchangeName ?? _config.ExchangeName))
            {
                IBasicProperties properties = null;
                if (_config.Persistence)
                {
                    properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2;
                }
                channel.BasicReturn += async (se, ex) => await Task.Delay(_config.NoConsumerMessageRetryInterval).ContinueWith((t) => Publish(ex.Body, ex.RoutingKey, ex.Exchange));
                channel.BasicPublish(
                    exchange: exchangeName ?? _config.ExchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: sendBytes);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TMessage"><see cref="QueueAttribute"/>标识的类</typeparam>
        /// <param name="value">需要发送的消息</param>
        public Task Publish<TMessage>(TMessage value)
        {
            var queue = GetQueue(typeof(TMessage));
            return Publish(value, queue.RoutingKey ?? "", queue.ExchangeName);
        }

        private (string QueueName, string RoutingKey, string ExchangeName) GetQueue(Type messageType)
        {
            var queue = messageType.GetQueue();
            var exchangeName = queue.ExchangeName ?? _config.ExchangeName;
            var queueName = queue.QueueName ?? $"{exchangeName}.{messageType.Name}";
            return (queueName, queue.RoutingKey ?? "", exchangeName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName"></param>
        private IModel BindExchange(string exchangeName)
        {
            IModel channel = _factory.GetConnection.CreateModel();
            try
            {
                channel.ExchangeDeclarePassive(exchangeName ?? _config.ExchangeName);
            }
            catch
            {
                channel = _factory.GetConnection.CreateModel();
                channel.ExchangeDeclare(exchangeName ?? _config.ExchangeName, _config.ExchangeType, durable: _config.Persistence, autoDelete: false);
            }
            return channel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        private IModel CreateQueue(string exchangeName, string queueName, string routingKey)
        {

            IModel channel = BindExchange(exchangeName);
            channel.QueueUnbind(queueName, exchangeName ?? _config.ExchangeName, routingKey);
            try
            {
                channel.QueueDeclarePassive(queueName);
            }
            catch
            {
                channel = BindExchange(exchangeName);
                channel.QueueDeclare(queueName, _config.Persistence, false, false, null);
            }
            channel.QueueBind(queueName, exchangeName ?? _config.ExchangeName, routingKey, null);
            channel.BasicQos(0, 1, false);
            return channel;
        }
    }
}
