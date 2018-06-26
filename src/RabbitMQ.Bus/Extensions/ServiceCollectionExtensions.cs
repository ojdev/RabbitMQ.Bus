using RabbitMQ.Bus;
using RabbitMQ.Bus.Extensions;
using System;
using System.Linq;

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
            services.AddSingleton(options => new RabbitMQBusService(config));
            var messageTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttributes(typeof(QueueAttribute), true).Any())).ToList();
            foreach (var messageType in messageTypes)
            {
                var handlers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IRabbitMQBusHandler<>).MakeGenericType(messageType)))).ToList();

                foreach (var handleType in handlers)
                {
                    Console.WriteLine(handleType);
                    services.AddScoped(handleType);
                }
            }
            return services;
        }
    }
}
