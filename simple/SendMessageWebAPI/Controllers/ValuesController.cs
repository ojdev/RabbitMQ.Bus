﻿using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Bus;
using System;
using System.Collections.Generic;

namespace SendMessageWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IRabbitMQBus _rabbit;
        public ValuesController(IRabbitMQBus rabbit)
        {
            _rabbit = rabbit;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            //_rabbit.Publish(new { Message="发送了一条消息",routingkey);
            _rabbit.Publish(new { message = "topic send message." }, routingKey: "send.message", exchangeName: "dev.ex.temp.topic");
            //_rabbit.Publish(new { message = "topic send message1." }, routingKey: "send.message", exchangeName: "dev.ex.temp.topic");
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
