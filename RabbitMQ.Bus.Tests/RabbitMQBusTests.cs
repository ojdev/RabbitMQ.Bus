using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RabbitMQ.Bus.Tests
{
    public class RabbitMQBusTests : RabbitMQBusTestBase
    {
        private RabbitMQBusService _rabbitMQBusService;

        protected override string RabbitMQConnectionString => "amqp://guest:guest@192.168.0.252:5672/";

        protected override void Initialize()
        {
            this._rabbitMQBusService = Resolve<RabbitMQBusService>();
        }

        [Fact]
        public void AutoSubScribeTest()
        {
            _rabbitMQBusService.AutoSubscribe();
        }

        [Fact]
        public async Task ProcessMessageAsyncTest()
        {
            var message = new TestMessage { TestProperty = "test" };
            await _rabbitMQBusService.ProcessMessageAsync(
                typeof(TestMessage),
                message,
                typeof(IRabbitMQBusHandler<TestMessage>),
                new[] { new TestMessageHandler() });

            Assert.Contains(message, TestMessageHandler.Arguments);
        }
    }

    [Queue("testqueue")]
    public class TestMessage
    {
        public string TestProperty { get; set; }
    }

    public class TestMessageHandler : IRabbitMQBusHandler<TestMessage>
    {
        private static readonly ICollection<TestMessage> _arguments;
        static TestMessageHandler()
        {
            _arguments = new List<TestMessage>();
        }
        public static IEnumerable<TestMessage> Arguments => _arguments;
        public Task Handle(TestMessage message)
        {
            _arguments.Add(message);
            return Task.CompletedTask;
        }
    }
}
