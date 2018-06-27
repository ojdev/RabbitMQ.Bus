using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Bus;
namespace SendMessageWebAPI
{
    [Queue(ExchangeName = "dev.ex", RoutingKey = "send.#")]
    public class SendMessage
    {
        public string Message { set; get; }

        public SendMessage(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
    public class SendMessageHandle : IRabbitMQBusHandler<SendMessage>
    {
        private readonly SendMessageManager manager;
        public SendMessageHandle(SendMessageManager sendMessage)
        {
            manager = sendMessage;
        }
        public Task Handle(SendMessage message)
        {
            manager.Write(message);
            return Task.CompletedTask;
        }
    }
    public class SendMessageHandle1 : IRabbitMQBusHandler<SendMessage>
    {
        private readonly SendMessageManager manager;
        public SendMessageHandle1(SendMessageManager sendMessage)
        {
            manager = sendMessage;
        }
        public Task Handle(SendMessage message)
        {
            manager.Write(message);
            return Task.CompletedTask;
        }
    }
    public class SendMessageManager
    {
        public void Write(SendMessage message)
        {
            Console.WriteLine(message.Message);
        }
    }
}
