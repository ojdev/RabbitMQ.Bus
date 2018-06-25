using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using RabbitMQ.Bus.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitMQBusService
    {
        private readonly RabbitMQBusFactory _factory;
        private readonly RabbitMQConfig _config;
        private IServiceProvider _serviceProvider;
        private RabbitMQConfig config;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="serviceProvider"></param>
        public RabbitMQBusService(IServiceProvider serviceProvider, RabbitMQConfig config)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(config));
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
            _factory.GetConnection = _factory.ConnectionFactory.CreateConnection();
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        public void Subscribe<TMessage>() where TMessage : class
        {
            var messageType = typeof(TMessage);
            var queue = messageType.GetQueue();

            IModel channel = Binding(_config.ExchangeName, queue.QueueName, queue.RoutingKey);

            EventingBasicConsumer _consumer = new EventingBasicConsumer(channel);
            _consumer.Received += async (ch, ea) =>
            {
                var serviceObject = _serviceProvider.GetService(typeof(IEnumerable<IRabbitMQBusHandler<TMessage>>));
                if (serviceObject == null)
                {
                    return;
                }
                var allhandles = (IEnumerable<IRabbitMQBusHandler<TMessage>>)serviceObject;

                if (!allhandles.Any())
                {
                    Console.Error.WriteLine($"找不到实现了IRabbitMQBusHandler<{messageType.Name}>的消息处理类。");
                    return;
                }
                var messageBody = Encoding.UTF8.GetString(ea.Body);
                TMessage message = JsonConvert.DeserializeObject<TMessage>(messageBody);

                foreach (var handler in allhandles)
                {
                    try
                    {
                        await handler.Handle(message);
                    }
                    catch
                    {
                    }
                }
                channel.BasicAck(ea.DeliveryTag, false);
            };
            channel.BasicConsume(queue.QueueName, true, _consumer);
        }
        /// <summary>
        /// 自动订阅消息
        /// </summary>
        public void AutoSubscribe()
        {
            var allQueue = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof(QueueAttribute), true).Any())).ToArray();

            foreach (var queueType in allQueue)
            {
                var genericType = typeof(IRabbitMQBusHandler<>).MakeGenericType(queueType);
                var serviceObject = _serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(genericType));
                if (serviceObject == null)
                {
                    continue;
                }
                var allHandler = (IEnumerable<IRabbitMQBusHandler>)serviceObject;

                if (!allHandler.Any())
                {
                    continue;
                }

                var queue = queueType.GetQueue();
                IModel channel = Binding(_config.ExchangeName, queue.QueueName, queue.RoutingKey);
                EventingBasicConsumer _consumer = new EventingBasicConsumer(channel);
                _consumer.Received += async (ch, ea) =>
                {
                    var messageBody = Encoding.UTF8.GetString(ea.Body);
                    var message = JsonConvert.DeserializeObject(messageBody, queueType);
                    await ProcessMessageAsync(queueType, message, genericType, allHandler);
                    channel.BasicAck(ea.DeliveryTag, false);
                };
                channel.BasicConsume(queue.QueueName, false, _consumer);
            }
        }
        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <param name="handlerType"></param>
        /// <param name="handlers"></param>
        /// <returns></returns>
        public async Task ProcessMessageAsync(Type messageType, object message, Type handlerType, IEnumerable<IRabbitMQBusHandler> handlers)
        {
            var method = handlerType.GetMethod(nameof(IRabbitMQBusHandler<Object>.Handle), new[] { messageType });
            if (method == null)
            {
                return;
            }
            var tasks = new List<Task>();

            foreach (var handler in handlers)
            {
                var task = (Task)method.Invoke(handler, new object[] { message });
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="exchangeName">留空则使用默认的交换机</param>
        /// <param name="queueName">队列名</param>
        /// <param name="value">需要发送的消息</param>
        /// <param name="routingKey">路由Key</param>
        public void Publish<TMessage>(string exchangeName, string queueName, TMessage value, string routingKey)
        {
            using (IModel channel = Binding(exchangeName ?? _config.ExchangeName, queueName, routingKey))
            {
                var sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
                IBasicProperties properties = null;
                if (_config.Persistence)
                {
                    properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
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
            var messageType = typeof(TMessage);
            var queue = messageType.GetQueue();
            Publish(queue.ExchangeName, queue.QueueName, value, queue.RoutingKey ?? "");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        private IModel Binding(string exchangeName, string queueName, string routingKey)
        {
            var channel = _factory.GetConnection.CreateModel();
            channel.QueueUnbind(queueName, exchangeName ?? _config.ExchangeName, routingKey);
            channel.ExchangeDeclare(exchangeName ?? _config.ExchangeName, _config.ExchangeType, false, false, null);
            channel.QueueDeclare(queueName, _config.Persistence, false, false, null);
            channel.QueueBind(queueName, exchangeName ?? _config.ExchangeName, routingKey, null);
            return channel;
        }
    }
}
