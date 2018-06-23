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
        public string QueueName { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName">队列名</param>
        public QueueAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}
