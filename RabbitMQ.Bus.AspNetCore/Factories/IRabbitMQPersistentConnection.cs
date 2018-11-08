using RabbitMQ.Client;
using System;

namespace RabbitMQ.EventBus.AspNetCore.Factories
{
    public interface IRabbitMQPersistentConnection : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();

    }
}
