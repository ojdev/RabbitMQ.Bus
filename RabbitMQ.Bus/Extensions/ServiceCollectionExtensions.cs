using RabbitMQ.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Bus.Extensions;
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
        /// <param name="connectionString">RabbitMQ连接字符串（例：amqp://guest:guest@172.0.0.1:5672/）</param>
        /// <param name="actionSetup"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQBus(this IServiceCollection services, string connectionString, Action<RabbitMQConfig> actionSetup = null)
        {
            if (connectionString.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(connectionString));
            var config = new RabbitMQConfig(connectionString);
            actionSetup?.Invoke(config);
            services.AddSingleton(options => new RabbitMQBusService(options, config));
            var allhandles = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => !t.IsInterface).Where(t => t.GetInterfaces().Contains(typeof(IRabbitMQBusHandler)))).ToArray();
            foreach (var handleType in allhandles)
            {
                services.AddScoped(typeof(IRabbitMQBusHandler<>), handleType);
            }
            return services;
        }
    }
}
