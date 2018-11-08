using Autofac;
using Housecool.Butterfly.Client.Tracing;
using Housecool.Butterfly.OpenTracing;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System;
using System.Reflection;
using System.Threading.Tasks;
namespace RabbitMQ.Bus.Autofac
{
    /// <summary>
    /// 
    /// </summary>
    internal class AutofacMessageReceive
    {
        private readonly ILifetimeScope _lifetime;
        private readonly IRabbitMQBus _service;
        private readonly IServiceTracer _tracer;
        private readonly ILogger<AutofacMessageReceive> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lifetime"></param>
        /// <param name="service"></param>
        /// <param name="tracer"></param>
        /// <param name="logger"></param>
        public AutofacMessageReceive(ILifetimeScope lifetime, IRabbitMQBus service, IServiceTracer tracer, ILogger<AutofacMessageReceive> logger)
        {
            _lifetime = lifetime;
            _service = service;
            _tracer = tracer;
            _logger = logger;
            _service.OnMessageReceived += RabbitMQ_OnMessageReceived;
            _service.OnPublish += RabbitMQ_OnPublish;
        }

        private void RabbitMQ_OnPublish(object sender, OpenTracingMessage e)
        {
            if (_tracer != null)
            {
                try
                {
                    if (_service.Config != null)
                    {
                        _tracer.ChildTrace("RabbitMQ_publish", DateTimeOffset.Now, span =>
                        {
                            span.Tags.Client().Component("RabbitMQ_Publish")
                            .Set("ExchangeType", _service.Config.ExchangeType)
                            .Set("ClientProvidedName", _service.Config.ClientProvidedName)
                            .Set(nameof(OpenTracingMessage.ExchangeName), e.ExchangeName)
                            .Set(nameof(OpenTracingMessage.RoutingKey), e.RoutingKey)
                            .Set(nameof(OpenTracingMessage.Information), e.Information)
                            .Set(nameof(OpenTracingMessage.NoConsumer), e.NoConsumer)
                            .PeerAddress(_service.Config.ConnectionString);
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(ex.HResult), ex, ex.Message);
                }
            }
        }

        /// <summary>
        /// 激活
        /// </summary>
        public void Active()
        {
            _logger.LogInformation("RabbitMQ Bus for Autofac active.");
        }
        /// <summary>
        /// 消息的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RabbitMQ_OnMessageReceived(object sender, MessageContext e)
        {
            if ((sender is BasicDeliverEventArgs basicDeliver))
            {
                if (_tracer != null)
                {
                    try
                    {
                        if (_service.Config != null)
                        {
                            _tracer.ChildTrace("RabbitMQ_Received", DateTimeOffset.Now, span =>
                            {
                                span.Tags.Client().Component("RabbitMQ_Received")
                                .Set("ExchangeType", _service.Config.ExchangeType)
                                .Set("ClientProvidedName", _service.Config.ClientProvidedName)
                                .Set(nameof(OpenTracingMessage.ExchangeName), basicDeliver.Exchange)
                                .Set(nameof(OpenTracingMessage.RoutingKey), basicDeliver.RoutingKey)
                                .Set(nameof(OpenTracingMessage.Information), e.OriginalData)
                                .PeerAddress(_service.Config.ConnectionString);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(new EventId(ex.HResult), ex, ex.Message);
                    }
                }
                using (ILifetimeScope scope = _lifetime.BeginLifetimeScope())
                {
                    bool isAck = false;
                    try
                    {
                        Guid eventId = Guid.NewGuid();
                        object handle = scope.ResolveOptional(e.HandleType);
                        MethodInfo method = e.HandleType.GetMethod(nameof(IRabbitMQBusHandler.Handle));
                        Task task = (Task)method.Invoke(handle, new[] { e.Message });
                        await task.ConfigureAwait(false);
                        isAck = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(new EventId(ex.HResult), ex, ex.Message);
                    }
                    finally
                    {
                        _logger.LogInformation($"{DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss")}\tReceived\t{isAck}\t{basicDeliver.Exchange}\t{basicDeliver.RoutingKey}\t{e.OriginalData}");
                    }
                }
            }
        }
    }
}
