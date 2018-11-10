using RabbitMQ.EventBus.AspNetCore.Attributes;
using RabbitMQ.EventBus.AspNetCore.Events;
using System;
using System.Threading.Tasks;
namespace SendMessageWebAPI
{
    [EventBus(Exchange = "dev.ex.temp.topic", RoutingKey = "send.message")]
    public class SendMessage : IEvent
    {
        public string Message { set; get; }

        public SendMessage(string message)
        {
            Message = message;
        }
    }
    [EventBus(Exchange = "dev.ex.temp.topic", RoutingKey = "send.message")]
    public class SendMessage1 : IEvent
    {
        public string Message { set; get; }

        public SendMessage1(string message)
        {
            Message = message;
        }
    }
    [EventBus(Exchange = "dev.ex.temp.topic", RoutingKey = "send.#")]
    public class SendMessage2 : IEvent
    {
        public string Message { set; get; }

        public SendMessage2(string message)
        {
            Message = message;
        }
    }
    [EventBus(Exchange = "dev.ex.temp.topic", RoutingKey = "send.get")]
    public class SendMessage3 : IEvent
    {
        public string Message { set; get; }

        public SendMessage3(string message)
        {
            Message = message;
        }
    }
    public class SendMessageHandle : IEventHandler<SendMessage>
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

    public class SendMessageHandle1 : IEventHandler<SendMessage1>
    {
        public Task Handle(SendMessage1 message)
        {
            Console.WriteLine($"收到消息1{message.Message}");
            return Task.CompletedTask;
        }
    }
    public static class ErrorCount
    {
        public static int Index { get; set; } = 0;
    }
    public class SendMessageHandle2 : IEventHandler<SendMessage2>
    {
        public Task Handle(SendMessage2 message)
        {
            if (ErrorCount.Index++ < 5)
            {
                int d = 9;
                int x = d /0;
            }
            Console.WriteLine($"收到消息2{message.Message}");
            return Task.CompletedTask;
        }
    }
    public class SendMessageHandle3 : IEventHandler<SendMessage3>
    {
        public Task Handle(SendMessage3 message)
        {
            Console.WriteLine($"收到消息3{message.Message}");
            return Task.CompletedTask;
        }
    }
    public class SendMessageManager
    {
        public void Write(SendMessage message)
        {
            Console.WriteLine($"收到消息{message.Message}");
        }
    }
}
