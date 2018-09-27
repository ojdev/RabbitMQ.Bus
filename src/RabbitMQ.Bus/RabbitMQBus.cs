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
        /// <summary>
        /// 
        /// </summary>
        public RabbitMQConfig Config => _factory?.Config;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<MessageContext> OnMessageReceived;
        /// <summary>
        /// 发送消息
        /// </summary>
        public event EventHandler<OpenTracingMessage> OnPublish;
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
            _factory = new RabbitMQBusFactory
            {
                Config = config ?? throw new ArgumentNullException(nameof(config)),
                ConnectionFactory = new ConnectionFactory
                {
                    AutomaticRecoveryEnabled = config.AutomaticRecoveryEnabled,
                    NetworkRecoveryInterval = config.NetworkRecoveryInterval,
                    Uri = new Uri(config.ConnectionString)
                }
            };
            _factory.TryConnect();
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        public void Subscribe<TMessage>() where TMessage : class
        {
            (string QueueName, string RoutingKey, string ExchangeName) queue = GetQueue(typeof(TMessage));
            IModel channel = CreateQueue(queue.ExchangeName, queue.QueueName, queue.RoutingKey);
            EventingBasicConsumer _consumer = new EventingBasicConsumer(channel);
            _consumer.Received += async (ch, ea) =>
            {
                string messageBody = Encoding.UTF8.GetString(ea.Body);
                TMessage message = JsonConvert.DeserializeObject<TMessage>(messageBody);
                foreach (Type handleType in GetHandlers(typeof(TMessage)))
                {
                    if (OnMessageReceived != null)
                    {
                        OnMessageReceived?.Invoke(ea, new MessageContext(handleType, message, messageBody));
                    }
                    else
                    {
                        IRabbitMQBusHandler<TMessage> handle = Activator.CreateInstance(handleType) as IRabbitMQBusHandler<TMessage>;
                        await handle.Handle(message);
                    }
                }
                channel.BasicAck(ea.DeliveryTag, false);
            };
            channel.BasicConsume(queue.QueueName, false, _consumer);
            Console.WriteLine($"\t订阅\tExchange::{queue.ExchangeName}\tRoutingKey::{queue.RoutingKey}");
        }
        /// <summary>
        /// 自定订阅
        /// </summary>
        public void AutoSubscribe()
        {
            Console.WriteLine("开启自动订阅::BEGIN");
            foreach (Type messageType in GetQueues())
            {
                (string QueueName, string RoutingKey, string ExchangeName) queue = GetQueue(messageType);
                IModel channel = CreateQueue(queue.ExchangeName, queue.QueueName, queue.RoutingKey);
                EventingBasicConsumer _consumer = new EventingBasicConsumer(channel);
                _consumer.Received += (ch, ea) =>
                {
                    string messageBody = Encoding.UTF8.GetString(ea.Body);
                    object message = JsonConvert.DeserializeObject(messageBody, messageType);
                    foreach (Type handleType in GetHandlers(messageType))
                    {
                        if (OnMessageReceived != null)
                        {
                            OnMessageReceived?.Invoke(ea, new MessageContext(handleType, message, messageBody));
                        }
                        else
                        {
                            object handle = Activator.CreateInstance(handleType);
                            MethodInfo method = handleType.GetMethod(nameof(IRabbitMQBusHandler.Handle));
                            Task task = (Task)method.Invoke(handle, new[] { message });
                            task.GetAwaiter().GetResult();
                        }
                    }
                    channel.BasicAck(ea.DeliveryTag, false);
                };
                channel.BasicConsume(queue.QueueName, false, _consumer);
                Console.WriteLine($"\t订阅\tExchange::{queue.ExchangeName}\tRoutingKey::{queue.RoutingKey}");
            }
            Console.WriteLine("开启自动订阅::END");
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
            byte[] sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
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
            using (IModel channel = BindExchange(exchangeName ?? _factory.Config.ExchangeName))
            {
                IBasicProperties properties = null;
                if (_factory.Config.Persistence)
                {
                    properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2;
                }
                channel.BasicReturn += async (se, ex) => await Task.Delay(_factory.Config.NoConsumerMessageRetryInterval).ContinueWith((t) => Publish(ex.Body, ex.RoutingKey, ex.Exchange));
                channel.BasicPublish(
                    exchange: exchangeName ?? _factory.Config.ExchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: sendBytes);
            }
            OnPublish?.Invoke(this, new OpenTracingMessage());
            return Task.CompletedTask;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TMessage"><see cref="QueueAttribute"/>标识的类</typeparam>
        /// <param name="value">需要发送的消息</param>
        public Task Publish<TMessage>(TMessage value)
        {
            (string QueueName, string RoutingKey, string ExchangeName) queue = GetQueue(typeof(TMessage));
            return Publish(value, queue.RoutingKey ?? "", queue.ExchangeName);
        }

        private (string QueueName, string RoutingKey, string ExchangeName) GetQueue(Type messageType)
        {
            QueueAttribute queue = messageType.GetQueue();
            string exchangeName = queue.ExchangeName ?? _factory.Config.ExchangeName;
            string queueName = queue.QueueName ?? $"{exchangeName}.{messageType.Name}";
            return (queueName, queue.RoutingKey ?? "", exchangeName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName"></param>
        private IModel BindExchange(string exchangeName)
        {
            IModel channel = _factory.CreateModel();
            try
            {
                channel.ExchangeDeclarePassive(exchangeName ?? _factory.Config.ExchangeName);
            }
            catch
            {
                channel = _factory.CreateModel();
                channel.ExchangeDeclare(exchangeName ?? _factory.Config.ExchangeName, _factory.Config.ExchangeType, durable: _factory.Config.Persistence, autoDelete: false);
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
            channel.QueueUnbind(queueName, exchangeName ?? _factory.Config.ExchangeName, routingKey);
            try
            {
                channel.QueueDeclarePassive(queueName);
            }
            catch
            {
                channel = BindExchange(exchangeName);
                channel.QueueDeclare(queueName, _factory.Config.Persistence, false, false, null);
            }
            channel.QueueBind(queueName, exchangeName ?? _factory.Config.ExchangeName, routingKey, null);
            channel.BasicQos(0, 1, false);
            return channel;
        }
    }
}
