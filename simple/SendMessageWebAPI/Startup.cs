using AspectCore.Extensions.Autofac;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.EventBus.AspNetCore;
using SendMessageWebAPI.Controllers;
using System;

namespace SendMessageWebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddRabbitMQEventBus("amqp://guest:guest@192.168.0.252:5672/", eventBusOptionAction: eventBusOption =>
              {
                  eventBusOption.ClientProvidedAssembly("RabbitMQEventBusTest");
                  eventBusOption.EnableRetryOnFailure(true, 5000, TimeSpan.FromSeconds(30));
              }); 
            services.AddScoped<SendMessageManager>();
            ContainerBuilder container = new ContainerBuilder();
            container.Populate(services);
            container.RegisterDynamicProxy();
            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.RabbitMQEventBusAutoSubscribe();
            //Task.Factory.StartNew(async () =>
            //{
            //    //为了验证先启动生产者发送消息，后启动消费者消费的情况
            //    await Task.Delay(20000);
            //    app.UseRabbitMQBus(true);
            //});
            app.UseMvc();
        }
    }
}
