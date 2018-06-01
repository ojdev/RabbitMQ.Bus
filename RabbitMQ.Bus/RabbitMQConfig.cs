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
        public RabbitMQConfig(string connectionString) : this()
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            AutomaticRecoveryEnabled = true;
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">连接字符串（例：amqp://guest:guest@172.0.0.1:5672/）</param>
        /// <param name="automaticRecoveryEnabled">是否开启网络自动恢复</param>
        public RabbitMQConfig(string connectionString, bool automaticRecoveryEnabled) : this()
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            AutomaticRecoveryEnabled = automaticRecoveryEnabled;
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">连接字符串（例：amqp://guest:guest@172.0.0.1:5672/）</param>
        /// <param name="automaticRecoveryEnabled">是否开启网络自动恢复</param>
        /// <param name="networkRecoveryInterval">网络自动恢复时间间隔</param>
        public RabbitMQConfig(string connectionString, bool automaticRecoveryEnabled, TimeSpan networkRecoveryInterval) : this()
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            AutomaticRecoveryEnabled = automaticRecoveryEnabled;
            NetworkRecoveryInterval = networkRecoveryInterval;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">连接字符串（例：amqp://guest:guest@172.0.0.1:5672/）</param>
        /// <param name="automaticRecoveryEnabled">是否开启网络自动恢复</param>
        /// <param name="networkRecoveryInterval">网络自动恢复时间间隔</param>
        /// <param name="exchangeName">默认名称：Default.RabbitMQBus.Exchange</param>
        public RabbitMQConfig(string connectionString, bool automaticRecoveryEnabled, TimeSpan networkRecoveryInterval, string exchangeName)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            AutomaticRecoveryEnabled = automaticRecoveryEnabled;
            NetworkRecoveryInterval = networkRecoveryInterval;
            ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
        }
    }
}
