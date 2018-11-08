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
        /// 
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// 
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
