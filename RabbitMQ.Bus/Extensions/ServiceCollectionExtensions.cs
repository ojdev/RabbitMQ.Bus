using RabbitMQ.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册RabbitMQBus
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection RabbitMQBus(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRabbitMQBusHandler<>));
            // OR
            //var allhandles = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IRabbitMQBusHandler<>)))).ToArray();
            //foreach (var handleType in allhandles)
            //{
            //    services.AddScoped(handleType);
            //}
            return services;
        }
    }
}
