using System;

namespace RabbitMQ.EventBus.AspNetCore.Configurations
{
    public sealed class RabbitMQEventBusConnectionConfiguration
    {
        /// <summary>
        /// 是否显示日志
        /// </summary>
        public bool Logging { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ClientProvidedName { get; set; }
        /// <summary>
        /// 连接出现错误后重试连接的指数次数(默认：50)
        /// </summary>
        public int FailReConnectRetryCount { get; set; }
        /// <summary>
        /// 是否开启网络自动恢复(默认开启)
        /// </summary>
        public bool AutomaticRecoveryEnabled { get; set; }
        /// <summary>
        /// 网络自动恢复时间间隔（默认5秒）
        /// </summary>
        public TimeSpan NetworkRecoveryInterval { get; set; }

        public RabbitMQEventBusConnectionConfiguration()
        {
            Logging = false;
            FailReConnectRetryCount = 50;
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
            AutomaticRecoveryEnabled = true;
        }
    }
}
