using Housecool.Butterfly.Client.Tracing;
using Housecool.Butterfly.OpenTracing;
using RabbitMQ.EventBus.AspNetCore.Modules;
using System;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMQEventBusConnectionConfigurationBuildExtensions
    {
        public static RabbitMQEventBusModuleOption AddButterfly(this RabbitMQEventBusModuleOption build, IServiceTracer tracer)
        {
            build.AddModule(ButterflyHandler.Handle(tracer));
            return build;
        }
    }
    public class ButterflyHandler : IModuleHandle
    {
        private static ButterflyHandler _butterflyHandler;
        private readonly IServiceTracer _serviceTracer;
        public static ButterflyHandler Handle(IServiceTracer serviceTracer)
        {
            if (_butterflyHandler == null)
            {
                _butterflyHandler = new ButterflyHandler(serviceTracer);
            }
            return _butterflyHandler;
        }
        private ButterflyHandler(IServiceTracer serviceTracer)
        {
            _serviceTracer = serviceTracer;
        }
        public async Task PublishEvent(EventBusArgs e)
        {
            if (_serviceTracer != null)
            {
                try
                {
                    await _serviceTracer.ChildTraceAsync("RabbitMQ_publish", DateTimeOffset.Now, span =>
                    {
                        span.Tags.Client().Component("RabbitMQ_Publish")
                        .Set("ClientProvidedName", e.ClientProvidedName)
                        .Set(nameof(EventBusArgs.ExchangeType), e.ExchangeType)
                        .Set(nameof(EventBusArgs.Exchange), e.Exchange)
                        .Set(nameof(EventBusArgs.Queue), e.Queue)
                        .Set(nameof(EventBusArgs.RoutingKey), e.RoutingKey)
                        .Set(nameof(EventBusArgs.Message), e.Message)
                        .PeerAddress(e.EndPoint);
                        return Task.CompletedTask;
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Butterfly故障\t" + ex.Message);
                }
            }
        }

        public async Task SubscribeEvent(EventBusArgs e)
        {
            if (_serviceTracer != null)
            {
                try
                {
                    await _serviceTracer.ChildTraceAsync("RabbitMQ_Received", DateTimeOffset.Now, span =>
                     {
                         span.Tags.Client().Component("RabbitMQ_Received")
                         .Set("ClientProvidedName", e.ClientProvidedName)
                         .Set(nameof(EventBusArgs.ExchangeType), e.ExchangeType)
                         .Set(nameof(EventBusArgs.Exchange), e.Exchange)
                         .Set(nameof(EventBusArgs.Queue), e.Queue)
                         .Set(nameof(EventBusArgs.RoutingKey), e.RoutingKey)
                         .Set(nameof(EventBusArgs.Message), e.Message)
                         .PeerAddress(e.EndPoint);
                         return Task.CompletedTask;
                     });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Butterfly故障\t" + ex.Message);
                }
            }
        }
    }
}
