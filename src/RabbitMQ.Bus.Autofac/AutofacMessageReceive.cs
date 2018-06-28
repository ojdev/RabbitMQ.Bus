using Autofac;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lifetime">Autofac的Lefttime</param>
        /// <param name="service">RabbitMQBus的服务</param>
        public AutofacMessageReceive(ILifetimeScope lifetime, IRabbitMQBus service)
        {
            _lifetime = lifetime;
            _service = service;
            _service.OnMessageReceived += RabbitMQ_OnMessageReceived;
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
