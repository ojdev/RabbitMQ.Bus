using System;

namespace RabbitMQ.EventBus.AspNetCore.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitMQEventBusConnectionConfigurationBuild
    {
        private RabbitMQEventBusConnectionConfiguration Configuration { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionConfiguration"></param>
        public RabbitMQEventBusConnectionConfigurationBuild(RabbitMQEventBusConnectionConfiguration connectionConfiguration)
        {
            Configuration = connectionConfiguration;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        public void ClientProvidedAssembly(string assembly)
        {
            Configuration.ClientProvidedName = assembly;
        }
        /// <summary>
        /// 网络恢复配置
        /// </summary>
        /// <param name="automaticRecovery">是否开启网络自动恢复</param>
        /// <param name="maxRetryCount">连接出现错误后重试连接的指数次数</param>
        /// <param name="maxRetryDelay">网络自动恢复时间间隔</param>
        public void EnableRetryOnFailure(bool automaticRecovery, int maxRetryCount, TimeSpan maxRetryDelay)
        {
            Configuration.AutomaticRecoveryEnabled = automaticRecovery;
            Configuration.FailReConnectRetryCount = maxRetryCount;
            Configuration.NetworkRecoveryInterval = maxRetryDelay;
        }
    }
}
