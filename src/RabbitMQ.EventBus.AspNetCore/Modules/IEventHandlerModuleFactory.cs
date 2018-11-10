using System;

namespace RabbitMQ.EventBus.AspNetCore.Modules
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventHandlerModuleFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        void TryAddMoudle(IModuleHandle module);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="handler"></param>
        void TryAddPubliushEventHandler(string typeName, Action<EventBusArgs> handler);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="handler"></param>
        void TryAddSubscribeEventHandler(string typeName, Action<EventBusArgs> handler);
        /// <summary>
        /// 
        /// </summary>
        void PubliushEvent(EventBusArgs e);
        /// <summary>
        /// 
        /// </summary>
        void SubscribeEvent(EventBusArgs e);
    }
}
