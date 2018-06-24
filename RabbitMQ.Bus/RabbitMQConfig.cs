using System;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitMQConfig
    {
        /// <summary>
        /// 是否开启网络自动恢复(默认开启)
        /// </summary>
        public bool AutomaticRecoveryEnabled { private set; get; }
        /// <summary>
        /// 网络自动恢复时间间隔（默认5秒）
        /// </summary>
        public TimeSpan NetworkRecoveryInterval { private set; get; }
        /// <summary>
        /// rabbitmq连接字串：amqp://guest:guest@172.0.0.1:5672/
        /// </summary>
        public string ConnectionString { private set; get; }
        /// <summary>
        /// 默认名称为：Default.RabbitMQBus.Exchange
        /// </summary>
        public string ExchangeName { private set; get; }
        /// <summary>
        /// <see cref="RabbitMQ.Client.ExchangeType"/>
        /// </summary>
        public string ExchangeType { private set; get; }
        /// <summary>
        /// 
        /// </summary>
        protected RabbitMQConfig()
        {
            ExchangeName = "Default.RabbitMQBus.Exchange";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">连接字符串（例：amqp://guest:guest@172.0.0.1:5672/）</param>
        /// <param name="exchangeType"><see cref="RabbitMQ.Client.ExchangeType"/></param>
        public RabbitMQConfig(string connectionString, string exchangeType) : this()
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            AutomaticRecoveryEnabled = true;
            ExchangeType = exchangeType;
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">连接字符串（例：amqp://guest:guest@172.0.0.1:5672/）</param>
        /// <param name="exchangeType"><see cref="RabbitMQ.Client.ExchangeType"/></param>
        /// <param name="automaticRecoveryEnabled">是否开启网络自动恢复</param>
        public RabbitMQConfig(string connectionString, string exchangeType, bool automaticRecoveryEnabled) : this()
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            AutomaticRecoveryEnabled = automaticRecoveryEnabled;
            ExchangeType = exchangeType;
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">连接字符串（例：amqp://guest:guest@172.0.0.1:5672/）</param>
        /// <param name="exchangeType"><see cref="RabbitMQ.Client.ExchangeType"/></param>
        /// <param name="automaticRecoveryEnabled">是否开启网络自动恢复</param>
        /// <param name="networkRecoveryInterval">网络自动恢复时间间隔</param>
        public RabbitMQConfig(string connectionString, string exchangeType, bool automaticRecoveryEnabled, TimeSpan networkRecoveryInterval) : this()
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            AutomaticRecoveryEnabled = automaticRecoveryEnabled;
            ExchangeType = exchangeType;
            NetworkRecoveryInterval = networkRecoveryInterval;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">连接字符串（例：amqp://guest:guest@172.0.0.1:5672/）</param>
        /// <param name="exchangeType"><see cref="RabbitMQ.Client.ExchangeType"/></param>
        /// <param name="automaticRecoveryEnabled">是否开启网络自动恢复</param>
        /// <param name="networkRecoveryInterval">网络自动恢复时间间隔</param>
        /// <param name="exchangeName">默认名称：Default.RabbitMQBus.Exchange</param>
        public RabbitMQConfig(string connectionString, string exchangeType, bool automaticRecoveryEnabled, TimeSpan networkRecoveryInterval, string exchangeName)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            AutomaticRecoveryEnabled = automaticRecoveryEnabled;
            ExchangeType = exchangeType;
            NetworkRecoveryInterval = networkRecoveryInterval;
            ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
        }
    }
}
