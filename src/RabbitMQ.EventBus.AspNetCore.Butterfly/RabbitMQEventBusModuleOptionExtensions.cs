﻿using Housecool.Butterfly.Client.Tracing;
using RabbitMQ.EventBus.AspNetCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMQEventBusModuleOptionExtensions
    {
        public static RabbitMQEventBusModuleOption AddButterfly(this RabbitMQEventBusModuleOption build, IServiceTracer tracer)
        {
            build.AddModule(ButterflyModuleHandler.Handle(tracer));
            return build;
        }
    }
}
