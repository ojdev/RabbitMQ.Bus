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
        public string Exchange { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Queue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RoutingKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ExchangeType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ClientProvidedName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

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
