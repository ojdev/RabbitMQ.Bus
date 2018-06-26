using System.Threading.Tasks;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IRabbitMQBusHandler<TMessage>
    {
        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="message">接收到的消息</param>
        /// <returns></returns>
        Task Handle(TMessage message);
    }
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitMQBusHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task Handle(dynamic message);
    }
}
