using Newtonsoft.Json;
using RabbitMQ.Bus.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
            IModel channel = CreateConsumer(queue.ExchangeName, queue.QueueName, queue.RoutingKey);
            EventingBasicConsumer _consumer = new EventingBasicConsumer(channel);
            _consumer.Received += async (ch, ea) =>
            {
                var messageBody = Encoding.UTF8.GetString(ea.Body);
                TMessage message = JsonConvert.DeserializeObject<TMessage>(messageBody);
                var handlers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IRabbitMQBusHandler<>).MakeGenericType(typeof(TMessage))))).ToList();
                foreach (var handleType in handlers)
                {
                    if (OnMessageReceived != null)
                    {
                        OnMessageReceived?.Invoke(ea, new MessageContext(handleType, message));
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
            var allQueue = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof(QueueAttribute), true).Any())).ToArray();
            foreach (var messageType in allQueue)
            {
                var queue = GetQueue(messageType);
                IModel channel = CreateConsumer(queue.ExchangeName, queue.QueueName, queue.RoutingKey);
                EventingBasicConsumer _consumer = new EventingBasicConsumer(channel);
                _consumer.Received += (ch, ea) =>
                {
                    var messageBody = Encoding.UTF8.GetString(ea.Body);
                    var message = JsonConvert.DeserializeObject(messageBody, messageType);
                    var handlers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IRabbitMQBusHandler<>).MakeGenericType(messageType)))).ToList();
                    foreach (var handleType in handlers)
                    {
                        if (OnMessageReceived != null)
                        {
                            OnMessageReceived?.Invoke(ea, new MessageContext(handleType, message));
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
        public void Publish<TMessage>(TMessage value, string routingKey = "", string exchangeName = "")
        {
            using (IModel channel = CreateProduce(exchangeName ?? _config.ExchangeName, routingKey))
            {
                var sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
                IBasicProperties properties = null;
                if (_config.Persistence)
                {
                    properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2;
                }
                channel.BasicPublish(exchangeName ?? _config.ExchangeName, routingKey, properties, sendBytes);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TMessage"><see cref="QueueAttribute"/>标识的类</typeparam>
        /// <param name="value">需要发送的消息</param>
        public void Publish<TMessage>(TMessage value)
        {
            var queue = GetQueue(typeof(TMessage));
            Publish(value, queue.RoutingKey ?? "", queue.ExchangeName);
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
        /// <param name="routingKey"></param>
        private IModel CreateProduce(string exchangeName, string routingKey)
        {
            var channel = _factory.GetConnection.CreateModel();
            channel.ExchangeDeclare(exchangeName ?? _config.ExchangeName, _config.ExchangeType, durable: _config.Persistence, autoDelete: false);
            return channel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        private IModel CreateConsumer(string exchangeName, string queueName, string routingKey)
        {

            var channel = _factory.GetConnection.CreateModel();
            channel.ExchangeDeclare(exchangeName ?? _config.ExchangeName, _config.ExchangeType, durable: _config.Persistence, autoDelete: false);
            channel.QueueUnbind(queueName, exchangeName ?? _config.ExchangeName, routingKey);
            channel.QueueDeclare(queueName, _config.Persistence, false, false, null);
            channel.QueueBind(queueName, exchangeName ?? _config.ExchangeName, routingKey, null);
            return channel;
        }
    }
}
