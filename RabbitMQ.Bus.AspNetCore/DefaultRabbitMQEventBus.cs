using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.EventBus.AspNetCore.Attributes;
using RabbitMQ.EventBus.AspNetCore.Events;
using RabbitMQ.EventBus.AspNetCore.Factories;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.EventBus.AspNetCore
{
    /// <summary>
    /// 
    /// </summary>
    internal class DefaultRabbitMQEventBus : IRabbitMQEventBus
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<DefaultRabbitMQEventBus> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DefaultRabbitMQEventBus(IRabbitMQPersistentConnection persistentConnection, ILogger<DefaultRabbitMQEventBus> logger, IServiceProvider serviceProvider)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        private IModel PublishChannel;
        public void Publish<TMessage>(TMessage message, string exchange, string routingKey, string type = ExchangeType.Topic)
        {
            if (PublishChannel?.IsOpen != true)
            {
                if (_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }
                PublishChannel = _persistentConnection.ExchangeDeclare(exchange, type: type);
                PublishChannel.BasicReturn += async (se, ex) => await Task.Delay(5000).ContinueWith(t => Publish(ex.Body, ex.Exchange, ex.RoutingKey));
            }
            IBasicProperties properties = PublishChannel.CreateBasicProperties();
            properties.DeliveryMode = 2; // persistent
            string body = message.Serialize();
            PublishChannel.BasicPublish(exchange: exchange,
                             routingKey: routingKey,
                             mandatory: true,
                             basicProperties: properties,
                             body: body.GetBytes());
            _logger.Information(body);
        }
        public void Subscribe<TEvent, THandler>(string type = ExchangeType.Topic)
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            Subscribe(typeof(TEvent), typeof(THandler));
            /*object attribute = typeof(TEvent).GetCustomAttributes(typeof(EventBusAttribute), true).FirstOrDefault();
            if (attribute is EventBusAttribute attr)
            {
                string queue = attr.Queue ?? $"{ attr.Exchange }.{ typeof(TEvent).Name }";
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }
                IModel channel;
                #region snippet
                try
                {
                    channel = _persistentConnection.ExchangeDeclare(exchange: attr.Exchange, type: type);
                    channel.QueueDeclarePassive(queue);
                }
                catch
                {
                    channel = _persistentConnection.ExchangeDeclare(exchange: attr.Exchange, type: type);
                    channel.QueueDeclare(queue: queue,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);
                }
                #endregion
                channel.QueueBind(queue, attr.Exchange, attr.RoutingKey, null);
                channel.BasicQos(0, 1, false);
                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    string body = Encoding.UTF8.GetString(ea.Body);
                    bool isAck = false;
                    try
                    {
                        await ProcessEvent<TEvent, THandler>(body);
                        channel.BasicAck(ea.DeliveryTag, multiple: false);
                        isAck = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(new EventId(ex.HResult), ex, ex.Message);
                    }
                    finally
                    {
                        _logger.Information($"RabbitMQEventBus\t{DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss")}\t{isAck}\t{ea.Exchange}\t{ea.RoutingKey}\t{body}");
                    }
                };
                channel.CallbackException += (sender, ex) =>
                {

                };
                channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
            }*/
        }

        public void Subscribe(Type eventType, Type eventHandleType, string type = ExchangeType.Topic)
        {
            object attribute = eventType.GetCustomAttributes(typeof(EventBusAttribute), true).FirstOrDefault();
            if (attribute is EventBusAttribute attr)
            {
                string queue = attr.Queue ?? $"{ attr.Exchange }.{ eventType.Name }";
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }
                IModel channel;
                #region snippet
                try
                {
                    channel = _persistentConnection.ExchangeDeclare(exchange: attr.Exchange, type: type);
                    channel.QueueDeclarePassive(queue);
                }
                catch
                {
                    channel = _persistentConnection.ExchangeDeclare(exchange: attr.Exchange, type: type);
                    channel.QueueDeclare(queue: queue,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);
                }
                #endregion
                channel.QueueBind(queue, attr.Exchange, attr.RoutingKey, null);
                channel.BasicQos(0, 1, false);
                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    string body = Encoding.UTF8.GetString(ea.Body);
                    bool isAck = false;
                    try
                    {
                        await ProcessEvent(body, eventType, eventHandleType);
                        channel.BasicAck(ea.DeliveryTag, multiple: false);
                        isAck = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(new EventId(ex.HResult), ex, ex.Message);
                    }
                    finally
                    {
                        _logger.Information($"RabbitMQEventBus\t{DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss")}\t{isAck}\t{ea.Exchange}\t{ea.RoutingKey}\t{body}");
                    }
                };
                channel.CallbackException += (sender, ex) =>
                {

                };
                channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
            }
        }
        private async Task ProcessEvent<TEvent, TEventHandle>(string body)
            where TEvent : IEvent
            where TEventHandle : IEventHandler<TEvent>
        {
            TEventHandle eventHandler = _serviceProvider.GetRequiredService<TEventHandle>();
            TEvent integrationEvent = JsonConvert.DeserializeObject<TEvent>(body);
            await eventHandler.Handle(integrationEvent);
        }
        private async Task ProcessEvent(string body, Type eventType, Type eventHandleType)
        {
            object eventHandler = _serviceProvider.GetRequiredService(eventHandleType);
            object integrationEvent = JsonConvert.DeserializeObject(body, eventType);
            Type concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
            await (Task)concreteType.GetMethod("Handle").Invoke(eventHandler, new object[] { integrationEvent });
        }
        public void Dispose()
        {
            //if (_consumerChannel != null)
            //{
            //    _consumerChannel.Dispose();
            //}

        }

    }
}
