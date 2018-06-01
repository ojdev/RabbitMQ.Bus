using RabbitMQ.Client;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// 
    /// </summary>
    sealed class RabbitMQBusFactory
    {
        public ConnectionFactory ConnectionFactory { set; get; }
        public IConnection GetConnection { set; get; }
        public IModel Channel { set; get; }
    }
}
