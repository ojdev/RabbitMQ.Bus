using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Net.Sockets;

namespace RabbitMQ.Bus
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class RabbitMQBusFactory
    {
        /// <summary>
        /// 
        /// </summary>
        public RabbitMQConfig Config { set; get; }
        public ConnectionFactory ConnectionFactory { set; get; }
        public IConnection Connect { set; get; }

        private readonly object sync_root = new object();
        public bool IsConnected => Connect != null && Connect.IsOpen;
        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }
            return Connect.CreateModel();
        }
        public string ErrorMessage(Exception ex)
        {
            if (ex.InnerException != null)
            {
                return $"{ex.Message}{Environment.NewLine}{ErrorMessage(ex.InnerException)}";
            }
            return ex.Message;
        }
        public bool TryConnect()
        {
           // RetryPolicy policy = Policy.Handle<SocketException>().Or<BrokerUnreachableException>()
           // .WaitAndRetry(Config.FailConnectRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
           // {
           //     Console.WriteLine(DateTimeOffset.Now.ToString("[yyyy-MM-dd HH:mm:ss]"));
           //     Console.WriteLine("--------RabbitMQ.Bus TryConnect Exception--------");
           //     Console.WriteLine(ErrorMessage(ex));
           //     Console.WriteLine($"{time.TotalSeconds}秒后重试。");
           //     Console.WriteLine("--------RabbitMQ.Bus TryConnect Exception::END--------");
           // });
           // policy.Execute(() =>
           //{
           //});
            Connect = ConnectionFactory.CreateConnection(clientProvidedName: Config.ClientProvidedName);
            if (IsConnected)
            {
                Connect.ConnectionShutdown += (sender, ex) => TryConnect();
                Connect.CallbackException += (sender, ex) => TryConnect();
                Connect.ConnectionBlocked += (sender, ex) => TryConnect();
                Console.WriteLine("[RabbitMQ已重新连接]");
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
