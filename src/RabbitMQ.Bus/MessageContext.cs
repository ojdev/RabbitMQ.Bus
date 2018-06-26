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

        public MessageContext(Type handleType, object message)
        {
            HandleType = handleType ?? throw new ArgumentNullException(nameof(handleType));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
