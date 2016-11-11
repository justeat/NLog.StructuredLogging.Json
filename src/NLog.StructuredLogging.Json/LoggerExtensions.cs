﻿using System;
 using System.Collections;
 using System.Collections.Generic;
 using System.Diagnostics;
﻿using System.Linq;
﻿using NLog.StructuredLogging.Json.Helpers;

namespace NLog.StructuredLogging.Json
{
    public static class LoggerExtensions
    {
        public static void ExtendedDebug(this ILogger logger, string message, object logProperties)
        {
            Extended(logger, LogLevel.Debug, message, logProperties, null);
        }

        public static void ExtendedInfo(this ILogger logger, string message, object logProperties)
        {
            Extended(logger, LogLevel.Info, message, logProperties, null);
        }

        public static void ExtendedWarn(this ILogger logger, string message, object logProperties)
        {
            Extended(logger, LogLevel.Warn, message, logProperties, null);
        }

        public static void ExtendedError(this ILogger logger, string message, object logProperties)
        {
            Extended(logger, LogLevel.Error, message, logProperties, null);
        }

        public static void ExtendedException(this ILogger logger, Exception ex, string message, object logProperties = null)
        {
            Extended(logger, LogLevel.Error, message, logProperties, ex);
        }

        public static void Extended(this ILogger logger, LogLevel logLevel, string message, object logProperties, Exception ex = null)
        {
            if (ex == null)
            {
                ExtendedWithException(logger, logLevel, message, logProperties, null, 0, 0, null);
            }
            else if (ex.InnerException == null)
            {
                ExtendedWithException(logger, logLevel, message, logProperties, ex, 1, 1, null);
            }
            else
            {
                var allExceptions = ConvertException.ToList(ex);
                var tag = Guid.NewGuid().ToString();

                for (var index = 0; index < allExceptions.Count; index++)
                {
                    ExtendedWithException(logger, logLevel, message, logProperties,
                        allExceptions[index], index + 1, allExceptions.Count, tag);
                }
            }
        }

        private static void ExtendedWithException(ILogger logger, LogLevel logLevel, string message, object logProperties,
            Exception ex, int exceptionIndex, int exceptionCount, string tag)
        {
            var log = new LogEventInfo(logLevel, logger.Name, message);
            TransferDataToLogEventProperties(log, logProperties);

            if (ex != null)
            {
                log.Exception = ex;
                log.Properties.Add("ExceptionIndex", exceptionIndex);
                log.Properties.Add("ExceptionCount", exceptionCount);

                if (!string.IsNullOrEmpty(tag))
                {
                    log.Properties.Add("ExceptionTag", tag);
                }
            }

            var stackTrace = new StackTrace(0);
            log.SetStackTrace(stackTrace, StackHelper.IndexOfFirstCallingMethod(stackTrace.GetFrames()));

            logger.Log(log);
        }

        private static void TransferDataToLogEventProperties(LogEventInfo log, object logProperties)
        {
            if (logProperties == null)
            {
                return;
            }

            if (logProperties is string)
            {
                return;
            }

            if (IsDictionary(logProperties))
            {                
                foreach (var logProperty in (IDictionary<object, object>) logProperties)
                {
                    log.Properties.Add(logProperty.Key, logProperty.Value);
                };
            }
            else
            {
                logProperties.GetType()
                    .GetProperties()
                    .Where(p => p.GetIndexParameters().Length == 0)
                    .ToList()
                    .ForEach(x => log.Properties.Add(x.Name, x.GetValue(logProperties)));
            }
        }

        private static bool IsDictionary(object logProperties)
        {
            var type = logProperties.GetType();
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }
    }
}
