using Microsoft.Extensions.Logging;
using System;

namespace MobileWebApi.Helper
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Logs an exception with a standardized structure so it can be correlated easily.
        /// </summary>
        public static void LogException(
            this ILogger logger,
            string exceptionCode,
            string methodName,
            Exception exception,
            int? userId = null)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (string.IsNullOrWhiteSpace(exceptionCode)) throw new ArgumentException("Exception code is required.", nameof(exceptionCode));
            if (string.IsNullOrWhiteSpace(methodName)) throw new ArgumentException("Method name is required.", nameof(methodName));
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            logger.LogError(
                exception,
                "ExceptionCode: {ExceptionCode} | Method: {Method} | UserId: {UserId} | TimestampUtc: {TimestampUtc}",
                exceptionCode,
                methodName,
                userId?.ToString() ?? "N/A",
                DateTime.UtcNow);
        }
    }
}

