using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using RabbitMQ.Bus.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitMQBusService
    {
        private readonly RabbitMQBusFactory _factory;
        private readonly RabbitMQConfig _config;
        /// <summary>
        /// 现有队列
        /// </summary>
        public List<string> Queues { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public RabbitMQBusService(RabbitMQConfig config)
        {
            Queues = new List<string>();
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
            _factory.Channel = _factory.GetConnection.CreateModel();
        }
        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        public void Subscribe<TMessage>() where TMessage : class
        {
            var messageType = typeof(TMessage);
            var queue = messageType.GetQueueName();

            EventingBasicConsumer _consumer = new EventingBasicConsumer(_factory.Channel);
            _consumer.Received += (ch, ea) =>
            {
                var allhandles = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IRabbitMQBusHandler<TMessage>)))).ToArray();
                if (allhandles.Count() == 0)
                {
                    Console.Error.WriteLine($"找不到实现了IRabbitMQBusHandler<{messageType.Name}>的消息处理类。");
                }
                var messageBody = Encoding.UTF8.GetString(ea.Body);
                TMessage message = JsonConvert.DeserializeObject<TMessage>(messageBody);
                foreach (var handleType in allhandles)
                {
                    var handler = (IRabbitMQBusHandler<TMessage>)Activator.CreateInstance(handleType);
                    handler.Handle(message).Wait();
                    handler = null;
                }
                _factory.Channel.BasicAck(ea.DeliveryTag, false);
            };
            _factory.Channel.BasicConsume(queue.QueueName, false, _consumer);
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="value"></param>
        /// <param name="routingKey">路由Key</param>
        public void Publish<TMessage>(string queueName, TMessage value, string routingKey)
        {
            if (!Queues.Contains(queueName))
            {
                Binding(queueName, routingKey);
            }

            var sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
            _factory.Channel.BasicPublish(_config.ExchangeName, routingKey, null, sendBytes);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="value"></param>
        /// <param name="routingKey">路由Key</param>
        public void Publish<TMessage>(TMessage value, string routingKey)
        {
            var messageType = typeof(TMessage);
            var queue = messageType.GetQueueName();

            if (!Queues.Contains(queue.QueueName))
            {
                if (queue.ExchangeName.IsNullOrWhiteSpace())
                {
                    Binding(queue.QueueName, routingKey);
                }
                else
                {
                    Binding(queue.ExchangeName, queue.QueueName, routingKey);
                }

            }

            var sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
            _factory.Channel.BasicPublish(_config.ExchangeName, routingKey, null, sendBytes);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        private void Binding(string queueName, string routingKey) => Binding(_config.ExchangeName, queueName, routingKey);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        private void Binding(string exchangeName, string queueName, string routingKey)
        {
            _factory.Channel.QueueUnbind(queueName, exchangeName, routingKey);
            _factory.Channel.ExchangeDeclare(_config.ExchangeName, _config.ExchangeType, false, false, null);
            _factory.Channel.QueueDeclare(queueName, false, false, false, null);
            _factory.Channel.QueueBind(queueName, _config.ExchangeName, routingKey, null);
            Queues.Add(queueName);
        }
    }
}
