using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// 
    /// </summary>
    public class OpenTracingMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public string ExchangeName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RoutingKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Information { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool NoConsumer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="information"></param>\
        /// <param name="noConsumer"></param>
        public OpenTracingMessage(string exchangeName, string routingKey, string information,bool noConsumer)
        {
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
            Information = information;
            NoConsumer = noConsumer;
        }
    }
}
