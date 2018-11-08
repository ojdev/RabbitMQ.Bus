using System.Threading.Tasks;

namespace RabbitMQ.EventBus.AspNetCore.Events
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<TEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task Handle(TEvent message);
    }
}
