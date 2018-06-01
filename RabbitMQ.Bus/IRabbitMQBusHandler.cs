using System.Threading.Tasks;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public interface IRabbitMQBusHandler<TIn>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task Handle(TIn obj);
    }
}
