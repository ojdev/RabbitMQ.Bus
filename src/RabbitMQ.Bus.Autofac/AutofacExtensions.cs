using Autofac;
using Microsoft.AspNetCore.Builder;
using RabbitMQ.Bus;
using RabbitMQ.Bus.Autofac;

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
        public static void AddAutofac(this RabbitMQConfig service, IServiceCollection services)
        {
            services.AddSingleton(options =>
            {
                var lifetime = options.GetRequiredService<ILifetimeScope>();
                var bus = options.GetRequiredService<RabbitMQBusService>();
                return new AutofacMessageReceive(lifetime, bus);
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
                var bus = app.ApplicationServices.GetRequiredService<RabbitMQBusService>();
                bus.AutoSubscribe();
            }
            var service = app.ApplicationServices.GetRequiredService<AutofacMessageReceive>();
            service.Active();
        }
    }
}
