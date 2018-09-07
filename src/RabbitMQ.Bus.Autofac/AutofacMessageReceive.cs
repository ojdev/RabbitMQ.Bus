using Autofac;
using Butterfly.Client.Tracing;
using Butterfly.OpenTracing;
using RabbitMQ.Client.Events;
using System;
using System.Reflection;
using System.Threading.Tasks;
namespace RabbitMQ.Bus.Autofac
{
    /// <summary>
    /// 
    /// </summary>
    class AutofacMessageReceive
    {
        private readonly ILifetimeScope _lifetime;
        private readonly IRabbitMQBus _service;
        private readonly IServiceTracer _tracer;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lifetime">Autofac的Lefttime</param>
        /// <param name="service">RabbitMQBus的服务</param>
        /// <param name="tracer">OpenTracer</param>
        public AutofacMessageReceive(ILifetimeScope lifetime, IRabbitMQBus service, IServiceTracer tracer)
        {
            _lifetime = lifetime;
            _service = service;
            _tracer = tracer;
            _service.OnMessageReceived += RabbitMQ_OnMessageReceived;
            _service.OnPublish += RabbitMQ_OnPublish;
        }

        private void RabbitMQ_OnPublish(object sender, OpenTracingMessage e)
        {
            if (_tracer != null)
            {
                try
                {
                    _tracer.ChildTrace("rabbitMQ_publish", DateTimeOffset.Now, span =>
                    {
                        span.Tags.Client().Component("RabbitMQ_Publish")
                        .Set("ExchangeType", _service.Config.ExchangeType)
                        .Set("ClientProvidedName", _service.Config.ClientProvidedName)
                        .PeerAddress(_service.Config.ConnectionString);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// 激活
        /// </summary>
        public void Active()
        {
            Console.WriteLine("RabbitMQ Bus for Autofac active.");
        }
        /// <summary>
        /// 消息的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RabbitMQ_OnMessageReceived(object sender, MessageContext e)
        {
            using (var scope = _lifetime.BeginLifetimeScope())
            {
                //if ((sender is BasicDeliverEventArgs basicDeliver))
                //{
                //    // TODO 暂留位置
                //}
                try
                {
                    var handle = scope.ResolveOptional(e.HandleType);
                    var method = e.HandleType.GetMethod(nameof(IRabbitMQBusHandler.Handle));
                    var task = (Task)method.Invoke(handle, new[] { e.Message });
                    await task.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                finally
                {
                    Console.WriteLine(e.OriginalData);
                }
            }
        }
    }
}
