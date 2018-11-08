using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.EventBus.AspNetCore;
using RabbitMQ.EventBus.AspNetCore.Attributes;
using RabbitMQ.EventBus.AspNetCore.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SendMessageWebAPI.Controllers
{
    [EventBus(Exchange = "EventBusTest.Pressure", RoutingKey = "Pressure1")]
    public class EventBusTest : IEvent
    {
        public int Index { get; set; }
    }
    public class EventBusTestHandler : IEventHandler<EventBusTest>
    {
        private readonly ILogger<EventBusTestHandler> _logger;

        public EventBusTestHandler(ILogger<EventBusTestHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Handle(EventBusTest message)
        {
            //Console.WriteLine(message.Index);
            //_logger.LogInformation($"消息编号:{message.Index}");
            return Task.CompletedTask;
        }
    }
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IRabbitMQEventBus _eventBus;

        public TestController(IRabbitMQEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }
        [HttpGet]
        public IActionResult Get()
        {
            for (int i = 0; i <= 500; i++)
            {
                Thread th = new Thread(new ParameterizedThreadStart(TestRabbitMQEventBus))
                {
                    IsBackground = true
                };
                th.Start();
            }
            return Ok();
        }

        private void TestRabbitMQEventBus(object state)
        {
            for (int i = 0; i <= 1000; i++)
            {
                _eventBus.Publish(new
                {
                    index = i
                }, "EventBusTest.Pressure", "Pressure1");
            }
        }
    }
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IRabbitMQEventBus _rabbit;
        public ValuesController(IRabbitMQEventBus rabbit)
        {
            _rabbit = rabbit;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            //_rabbit.Publish(new { Message="发送了一条消息",routingkey);
            /*_rabbit.Publish(new
            {
                Scope = Guid.NewGuid(),
                Type = "ListingDealSendMessageCreatorAction",
                Destination = Guid.NewGuid(),
                Parameters = new
                {
                    Code="000",
                    Community = "aaa"
                },
                Extra = new { },
                Source = "Core.Listing"
            }, routingKey: "send.message", exchangeName: "dev.ex.temp.topic");*/
            _rabbit.Publish(new { message = "topic send message1." }, routingKey: "send.message", exchangeName: "dev.ex.temp.topic");
            return new string[] { "value1", "value2", DateTime.Now.ToString(), DateTime.UtcNow.ToString() };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
