using System.Threading.Tasks;

namespace RabbitMQ.EventBus.AspNetCore.Modules
{
    /// <summary>
    /// 
    /// </summary>
    public interface IModuleHandle
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task PublishEvent(EventBusArgs e);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task SubscribeEvent(EventBusArgs e);
    }
}
