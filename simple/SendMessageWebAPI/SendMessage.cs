using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Bus;
namespace SendMessageWebAPI
{
    [Queue(ExchangeName = "dev.ex.temp.topic", RoutingKey = "send.message")]
    public class SendMessage
    {
        public string Message { set; get; }

        public SendMessage(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
    [Queue(ExchangeName = "dev.ex.temp.topic", RoutingKey = "send.message")]
    public class SendMessage1
    {
        public string Message { set; get; }

        public SendMessage1(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
    [Queue(ExchangeName = "dev.ex.temp.topic", RoutingKey = "send.#")]
    public class SendMessage2
    {
        public string Message { set; get; }

        public SendMessage2(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
    [Queue(ExchangeName = "dev.ex.temp.topic", RoutingKey = "send.get")]
    public class SendMessage3
    {
        public string Message { set; get; }

        public SendMessage3(string message)
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
    public class SendMessageHandle1 : IRabbitMQBusHandler<SendMessage1>
    {
        public Task Handle(SendMessage1 message)
        {
            Console.WriteLine(message.Message);
            return Task.CompletedTask;
        }
    }
    public class SendMessageHandle2 : IRabbitMQBusHandler<SendMessage2>
    {
        public Task Handle(SendMessage2 message)
        {
            Console.WriteLine(message.Message);
            return Task.CompletedTask;
        }
    }
    public class SendMessageHandle3 : IRabbitMQBusHandler<SendMessage3>
    {
        public Task Handle(SendMessage3 message)
        {
            Console.WriteLine(message.Message);
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
