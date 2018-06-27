using System;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitMQBus
    {
        /// <summary>
        /// 
        /// </summary>
        event EventHandler<MessageContext> OnMessageReceived;
        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        void Subscribe<TMessage>() where TMessage : class;
        /// <summary>
        /// 自定订阅
        /// </summary>
        void AutoSubscribe();
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="value">需要发送的消息</param>
        /// <param name="routingKey">路由Key</param>
        /// <param name="exchangeName">留空则使用默认的交换机</param>
        void Publish<TMessage>(TMessage value, string routingKey = "", string exchangeName = "");
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="value"></param>
        void Publish<TMessage>(TMessage value);
    }
}
