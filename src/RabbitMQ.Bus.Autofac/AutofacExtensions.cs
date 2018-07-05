using Autofac;
using Butterfly.Client.AspNetCore;
using Butterfly.Client.Tracing;
using Microsoft.AspNetCore.Builder;
using RabbitMQ.Bus;
using RabbitMQ.Bus.Autofac;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class AutofacExtensions
    {
        /// <summary>
        /// 使用Autofac的方式进行对象反射
        /// </summary>
        /// <param name="service"></param>
        /// <param name="services"></param>
        /// <param name="butterflySetup"></param>
        public static void AddAutofac(this RabbitMQConfig service, IServiceCollection services, Action<ButterflyOptions> butterflySetup = null)
        {
            if (butterflySetup != null)
            {
                services.AddButterfly(butterflySetup);
            }
            services.AddSingleton(options =>
            {
                var lifetime = options.GetRequiredService<ILifetimeScope>();
                var bus = options.GetRequiredService<IRabbitMQBus>();
                IServiceTracer tracer = null;
                if (butterflySetup != null)
                    tracer = options.GetRequiredService<IServiceTracer>();
                return new AutofacMessageReceive(lifetime, bus, tracer);
            });
        }
        /// <summary>
        /// 激活RabbitMQBus的Autofac
        /// </summary>
        /// <param name="app"></param>
        /// <param name="autoSubscribe"></param>
        public static void UseRabbitMQBus(this IApplicationBuilder app, bool autoSubscribe = false)
        {
            if (autoSubscribe)
            {
                var bus = app.ApplicationServices.GetRequiredService<IRabbitMQBus>();
                bus.AutoSubscribe();
            }
            var service = app.ApplicationServices.GetRequiredService<AutofacMessageReceive>();
            service.Active();
        }
    }
}
