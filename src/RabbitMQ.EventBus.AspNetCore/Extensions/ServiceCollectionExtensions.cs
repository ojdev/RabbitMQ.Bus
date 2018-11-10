using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.EventBus.AspNetCore;
using RabbitMQ.EventBus.AspNetCore.Configurations;
using RabbitMQ.EventBus.AspNetCore.Events;
using RabbitMQ.EventBus.AspNetCore.Factories;
using RabbitMQ.EventBus.AspNetCore.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString"></param>
        /// <param name="eventBusOptionAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, string connectionString, Action<RabbitMQEventBusConnectionConfigurationBuild> eventBusOptionAction)
        {
            RabbitMQEventBusConnectionConfiguration configuration = new RabbitMQEventBusConnectionConfiguration();
            RabbitMQEventBusConnectionConfigurationBuild configurationBuild = new RabbitMQEventBusConnectionConfigurationBuild(configuration);
            eventBusOptionAction?.Invoke(configurationBuild);
            services.TryAddSingleton<IRabbitMQPersistentConnection>(options =>
            {
                ILogger<DefaultRabbitMQPersistentConnection> logger = options.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                IConnectionFactory factory = new ConnectionFactory
                {
                    AutomaticRecoveryEnabled = configuration.AutomaticRecoveryEnabled,
                    NetworkRecoveryInterval = configuration.NetworkRecoveryInterval,
                    Uri = new Uri(connectionString),
                };
                return new DefaultRabbitMQPersistentConnection(configuration.ClientProvidedName, factory, logger, configuration.FailReConnectRetryCount, configuration.ConsumerFailRetryInterval);
            });
            services.TryAddSingleton<IEventHandlerModuleFactory, EventHandlerModuleFactory>();
            services.TryAddSingleton<IRabbitMQEventBus, DefaultRabbitMQEventBus>();
            IEnumerable<Type> messageTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IEvent))));
            foreach (Type mType in messageTypes)
            {
                services.TryAddTransient(mType);
                IEnumerable<Type> handllerTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IEventHandler<>).MakeGenericType(mType))));
                foreach (Type hType in handllerTypes)
                {
                    services.TryAddTransient(hType);
                }
            }
            return services;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public static void RabbitMQEventBusAutoSubscribe(this IApplicationBuilder app)
        {
            IRabbitMQEventBus eventBus = app.ApplicationServices.GetRequiredService<IRabbitMQEventBus>();
            IEnumerable<Type> messageTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IEvent))));
            foreach (Type mType in messageTypes)
            {
                IEnumerable<Type> handllerTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IEventHandler<>).MakeGenericType(mType))));
                foreach (Type hType in handllerTypes)
                {
                    eventBus.Subscribe(mType, hType);
                }
            }
        }
        public static void RabbitMQEventBusModule(this IApplicationBuilder app, Action<RabbitMQEventBusModuleOption> moduleOptions)
        {
            IEventHandlerModuleFactory factory = app.ApplicationServices.GetRequiredService<IEventHandlerModuleFactory>();
            RabbitMQEventBusModuleOption moduleOption = new RabbitMQEventBusModuleOption(factory);
            moduleOptions?.Invoke(moduleOption);
        }
    }
}
