using System;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitMQConfig
    {
        private const string DEFAULT_EXCHANGE_NAME = "Default.RabbitMQBus.Exchange";
        /// <summary>
        /// 
        /// </summary>
        public string ClientProvidedName { set; get; }
        /// <summary>
        /// 是否开启网络自动恢复(默认开启)
        /// </summary>
        public bool AutomaticRecoveryEnabled { get; set; }
        /// <summary>
        /// 网络自动恢复时间间隔（默认5秒）
        /// </summary>
        public TimeSpan NetworkRecoveryInterval { get; set; }
        /// <summary>
        /// rabbitmq连接字串：amqp://guest:guest@172.0.0.1:5672/
        /// </summary>
        public string ConnectionString { get; }
        /// <summary>
        /// 默认名称为：Default.RabbitMQBus.Exchange
        /// </summary>
        public string ExchangeName { get; set; }
        /// <summary>
        /// <see cref="RabbitMQ.Client.ExchangeType"/>
        /// </summary>
        public string ExchangeType { get; }
        /// <summary>
        /// 是否持久化消息
        /// </summary>
        public bool Persistence { get; set; }
        /// <summary>
        /// 无消费者时消息的重试时间(默认1秒)
        /// </summary>
        public TimeSpan NoConsumerMessageRetryInterval { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">连接字符串（例：amqp://guest:guest@172.0.0.1:5672/）</param>
        public RabbitMQConfig(string connectionString) :
            this(
                connectionString,
                true,
                TimeSpan.FromSeconds(5),
                true,
                DEFAULT_EXCHANGE_NAME)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">连接字符串（例：amqp://guest:guest@172.0.0.1:5672/）</param>
        /// <param name="automaticRecoveryEnabled">是否开启网络自动恢复</param>
        public RabbitMQConfig(string connectionString, bool automaticRecoveryEnabled) :
            this(
                connectionString,
                automaticRecoveryEnabled,
                TimeSpan.FromSeconds(5),
                true,
                DEFAULT_EXCHANGE_NAME)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">连接字符串（例：amqp://guest:guest@172.0.0.1:5672/）</param>
        /// <param name="automaticRecoveryEnabled">是否开启网络自动恢复</param>
        /// <param name="networkRecoveryInterval">网络自动恢复时间间隔</param>
        public RabbitMQConfig(string connectionString, bool automaticRecoveryEnabled, TimeSpan networkRecoveryInterval) :
            this(
                connectionString,
                automaticRecoveryEnabled,
                networkRecoveryInterval,
                true,
                DEFAULT_EXCHANGE_NAME)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">连接字符串（例：amqp://guest:guest@172.0.0.1:5672/）</param>
        /// <param name="automaticRecoveryEnabled">是否开启网络自动恢复</param>
        /// <param name="networkRecoveryInterval">网络自动恢复时间间隔</param>
        /// <param name="persistence">是否持久化消息</param>
        /// <param name="exchangeName">默认名称：Default.RabbitMQBus.Exchange</param>
        public RabbitMQConfig(string connectionString, bool automaticRecoveryEnabled, TimeSpan networkRecoveryInterval, bool persistence, string exchangeName)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            AutomaticRecoveryEnabled = automaticRecoveryEnabled;
            ExchangeType = Client.ExchangeType.Topic;
            NetworkRecoveryInterval = networkRecoveryInterval;
            Persistence = persistence;
            ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
            NoConsumerMessageRetryInterval = TimeSpan.FromSeconds(1);
        }
    }
}
