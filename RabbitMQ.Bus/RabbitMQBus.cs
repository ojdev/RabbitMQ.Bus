﻿using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
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
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="queueName">队列名称</param>
        /// <param name="isReply">是否确认消费，确认后其他人将接收不到消息,默认为确认</param>
        public void Subscribe<TIn, TOut>(string queueName, bool isReply = true) where TIn : IRabbitMQBusHandler<TOut>
        {
            EventingBasicConsumer _consumer = new EventingBasicConsumer(_factory.Channel);
            _consumer.Received += (ch, ea) =>
            {
                var handler = Activator.CreateInstance<TIn>();
                var message = Encoding.UTF8.GetString(ea.Body);
                handler.Handle(JsonConvert.DeserializeObject<TOut>(message));
                if (isReply)
                {
                    _factory.Channel.BasicAck(ea.DeliveryTag, false);
                }
            };
            _factory.Channel.BasicConsume(queueName, false, _consumer);
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="value"></param>
        /// <param name="routingKey">路由Key，可为空</param>
        public void Publish<TIn>(string queueName, TIn value, string routingKey = "")
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
        private void Binding(string queueName, string routingKey = "")
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