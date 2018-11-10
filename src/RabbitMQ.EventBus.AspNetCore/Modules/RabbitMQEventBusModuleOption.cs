using System;

namespace RabbitMQ.EventBus.AspNetCore.Modules
{
    public sealed class RabbitMQEventBusModuleOption
    {
        private readonly IEventHandlerModuleFactory handlerFactory;

        public RabbitMQEventBusModuleOption(IEventHandlerModuleFactory handlerFactory)
        {
            this.handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        public void AddModule(IModuleHandle module)
        {
            handlerFactory.TryAddMoudle(module);
            //    handlerFactory.TryAddPubliushEventHandler(typeof(TType).Name, publishHandleAction);
            //    handlerFactory.TryAddSubscribeEventHandler(typeof(TType).Name, subscribeHandleAction);
            //}
        }
    }
}
