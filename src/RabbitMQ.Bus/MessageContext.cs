using System;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageContext
    {
        /// <summary>
        /// 
        /// </summary>
        public Type HandleType { private set; get; }
        /// <summary>
        /// 
        /// </summary>
        public Object Message { private set; get; }
        /// <summary>
        /// 原始数据
        /// </summary>
        public string OriginalData { private set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handleType"></param>
        /// <param name="message"></param>
        /// <param name="originalData"></param>
        public MessageContext(Type handleType, object message, string originalData)
        {
            HandleType = handleType ?? throw new ArgumentNullException(nameof(handleType));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            OriginalData = originalData ?? throw new ArgumentNullException(nameof(originalData));
        }
    }
}
