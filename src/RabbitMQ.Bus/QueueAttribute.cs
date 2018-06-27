using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// RabbitMQBus订阅
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class QueueAttribute : Attribute
    {
        /// <summary>
        /// 队列名
        /// </summary>
        public string QueueName { get; set; }
        /// <summary>
        /// 交换机名
        /// </summary>
        public string ExchangeName { get; set; }
        /// <summary>
        /// 路由Key
        /// </summary>
        public string RoutingKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public QueueAttribute()
        {
            RoutingKey = "";
        }
    }
}
