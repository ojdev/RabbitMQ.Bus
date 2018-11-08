using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Extensions.Logging
{
    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> _informationRequested;
        static LoggerExtensions()
        {
            _informationRequested = LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(Information)), "{LogContent}");
        }
        public static void Information(this ILogger logger, string LogContent)
        {
            _informationRequested(logger, LogContent, null);
        }
    }
}
