using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace RabbitMQ.EventBus.AspNetCore.Modules
{
    /// <summary>
    /// 
    /// </summary>
    internal class EventHandlerModuleFactory : IEventHandlerModuleFactory
    {
        private readonly List<IModuleHandle> modules;
        private readonly Dictionary<string, Action<EventBusArgs>> PubliushEventManagers;
        private readonly Dictionary<string, Action<EventBusArgs>> SubscribeEventManagers;
        private readonly object sync_root = new object();
        private readonly IServiceProvider _serviceProvider;

        public EventHandlerModuleFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            PubliushEventManagers = new Dictionary<string, Action<EventBusArgs>>();
            SubscribeEventManagers = new Dictionary<string, Action<EventBusArgs>>();
            modules = new List<IModuleHandle>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="handler"></param>
        public void TryAddPubliushEventHandler(string typeName, Action<EventBusArgs> handler)
        {
            lock (sync_root)
            {
                if (!PubliushEventManagers.ContainsKey(typeName))
                {
                    PubliushEventManagers.Add(typeName, handler);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="handler"></param>
        public void TryAddSubscribeEventHandler(string typeName, Action<EventBusArgs> handler)
        {
            lock (sync_root)
            {
                if (!SubscribeEventManagers.ContainsKey(typeName))
                {
                    SubscribeEventManagers.Add(typeName, handler);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void PubliushEvent(EventBusArgs e)
        {
            foreach (IModuleHandle model in modules)
            {
                model.PublishEvent(e);
            }
            //foreach (KeyValuePair<string, Action<EventBusArgs>> manager in PubliushEventManagers)
            //{
            //    manager.Value?.Invoke(e);
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        public void SubscribeEvent(EventBusArgs e)
        {
            foreach (IModuleHandle model in modules)
            {
                model.SubscribeEvent(e);
            }
            //foreach (KeyValuePair<string, Action<EventBusArgs>> manager in SubscribeEventManagers)
            //{
            //    manager.Value?.Invoke(e);
            //}
        }

        public void TryAddMoudle(IModuleHandle module)
        {
            lock (sync_root)
            {
                modules.Add(module);
            }
        }
    }
}
