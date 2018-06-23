using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
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
        private bool _isBinding = false;
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
            _factory = new RabbitMQBusFactory();
            _factory.ConnectionFactory = new ConnectionFactory();
            _factory.ConnectionFactory.AutomaticRecoveryEnabled = config.AutomaticRecoveryEnabled;
            _factory.ConnectionFactory.NetworkRecoveryInterval = config.NetworkRecoveryInterval;
            _factory.ConnectionFactory.Uri = new Uri(config.ConnectionString);
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
            var messageAttribute = messageType.GetCustomAttributes(typeof(QueueAttribute), true).FirstOrDefault();
            if (messageAttribute == null)
            {
                throw new ArgumentNullException($"{messageType.Name}缺少{nameof(QueueAttribute)}标识。");
            }
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
            _factory.Channel.BasicConsume((messageAttribute as QueueAttribute).QueueName, false, _consumer);
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
            if (!_isBinding || !Queues.Contains(queueName))
            {
                Binding(queueName, routingKey);
            }
            var sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
            _factory.Channel.BasicPublish(_config.ExchangeName, routingKey, null, sendBytes);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        private void Binding(string queueName, string routingKey)
        {
            _factory.Channel.QueueUnbind(queueName, _config.ExchangeName, routingKey);
            _factory.Channel.ExchangeDeclare(_config.ExchangeName, ExchangeType.Direct, false, false, null);
            _factory.Channel.QueueDeclare(queueName, false, false, false, null);
            _factory.Channel.QueueBind(queueName, _config.ExchangeName, routingKey, null);
            _isBinding = true;
            Queues.Add(queueName);
        }
    }
}
