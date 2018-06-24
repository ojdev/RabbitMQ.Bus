using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitMQ.Bus.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class AttributeExtensions
    {
        /// <summary>
        /// 获取<see cref="QueueAttribute"/>标识中的QueueName
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static QueueAttribute GetQueue(this Type type)
        {
            var messageAttribute = type.GetCustomAttributes(typeof(QueueAttribute), true).FirstOrDefault();
            if (messageAttribute == null)
            {
                throw new ArgumentNullException($"{type.Name}缺少{nameof(QueueAttribute)}标识。");
            }
            var queueAttribute = (messageAttribute as QueueAttribute);
            if (string.IsNullOrWhiteSpace(queueAttribute.QueueName))
            {
                throw new ArgumentNullException($"{nameof(QueueAttribute)}中的队列名不能为空。");
            }
            return queueAttribute;
        }
    }
}
