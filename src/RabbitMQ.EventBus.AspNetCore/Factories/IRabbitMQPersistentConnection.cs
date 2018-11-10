using RabbitMQ.Client;
using System;

namespace RabbitMQ.EventBus.AspNetCore.Factories
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitMQPersistentConnection : IDisposable
    {
        /// <summary>
        /// 连接点
        /// </summary>
        string Endpoint { get; }
        /// <summary>
        /// 客户端
        /// </summary>
        string ClientProvidedName { get; }
        /// <summary>
        /// 消费消息异常后的拒绝时间
        /// </summary>
        TimeSpan ConsumerFailRetryInterval { get; }
        /// <summary>
        /// 是否打开链接
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// 尝试链接
        /// </summary>
        /// <returns></returns>
        bool TryConnect();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IModel CreateModel();

    }
}
