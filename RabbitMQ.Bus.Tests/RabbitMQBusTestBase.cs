using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;


namespace RabbitMQ.Bus.Tests
{
    public abstract class RabbitMQBusTestBase
    {
        private readonly ContainerBuilder _containerBuilder;
        private readonly IContainer _container;

        protected virtual string RabbitMQConnectionString { get; set; }

        public RabbitMQBusTestBase()
        {
            RabbitMQConnectionString = "amqp://guest:guest@127.0.0.1:5672/";
            this._containerBuilder = new ContainerBuilder();
            this.InitializeBasic(this._containerBuilder);
            this.PreInitialize(this._containerBuilder);
            this._container = this._containerBuilder.Build();
            this.Initialize();
        }

        private void InitializeBasic(ContainerBuilder containerBuilder)
        {
            IServiceCollection services = new TestServiceCollection();
            services.AddRabbitMQBus(RabbitMQConnectionString);
            containerBuilder.Populate(services);
        }


        protected virtual void PreInitialize(ContainerBuilder containerBuilder)
        {

        }

        protected virtual void Initialize()
        {
            
        }

        protected internal T Resolve<T>() where T: class
        {
            return this._container.Resolve<T>();
        }

        protected internal IEnumerable<T> ResolveAll<T>() where T : class
        {
            return this._container.Resolve<IEnumerable<T>>();
        }

        private class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
        {
        }

    }
}
