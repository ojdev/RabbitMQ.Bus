namespace RabbitMQ.EventBus.AspNetCore.Modules
{
    /// <summary>
    /// 
    /// </summary>
    public class EventBusArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public string EndPoint { get; set; }
        /// <summary>
        /// 交换机
        /// </summary>
        public string Exchange { get; set; }
        /// <summary>
        /// 队列
        /// </summary>
        public string Queue { get; set; }
        /// <summary>
        /// 路由
        /// </summary>
        public string RoutingKey { get; set; }
        /// <summary>
        /// 消息模式
        /// </summary>
        public string ExchangeType { get; set; }
        /// <summary>
        /// 客户端
        /// </summary>
        public string ClientProvidedName { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint">连接点</param>
        /// <param name="exchange">交换机</param>
        /// <param name="queue">队列</param>
        /// <param name="routingKey">路由</param>
        /// <param name="exchangeType">消息模式</param>
        /// <param name="clientProvidedName">客户端</param>
        /// <param name="message">消息</param>
        public EventBusArgs(string endPoint, string exchange, string queue, string routingKey, string exchangeType, string clientProvidedName, string message)
        {
            EndPoint = endPoint;
            Exchange = exchange;
            Queue = queue;
            RoutingKey = routingKey;
            ExchangeType = exchangeType;
            ClientProvidedName = clientProvidedName;
            Message = message;
        }

        public override string ToString()
        {
            return $"{EndPoint}\t{ClientProvidedName}\t{Exchange}\t{ExchangeType}\t{Queue}\t{RoutingKey}\t{Message}";
        }
    }
}
