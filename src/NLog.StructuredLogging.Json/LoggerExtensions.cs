using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NLog.StructuredLogging.Json.Helpers;

namespace NLog.StructuredLogging.Json
{
    public static class LoggerExtensions
    {
        public static void ExtendedDebug(this ILogger logger, string message, object logProperties = null)
        {
            Extended(logger, LogLevel.Debug, message, logProperties, null);
        }

        public static void ExtendedInfo(this ILogger logger, string message, object logProperties = null)
        {
            Extended(logger, LogLevel.Info, message, logProperties, null);
        }

        public static void ExtendedWarn(this ILogger logger, string message, object logProperties = null)
        {
            Extended(logger, LogLevel.Warn, message, logProperties, null);
        }

        public static void ExtendedError(this ILogger logger, string message, object logProperties = null)
        {
            Extended(logger, LogLevel.Error, message, logProperties, null);
        }

        public static void ExtendedException(this ILogger logger, Exception ex, string message,
            object logProperties = null)
        {
            Extended(logger, LogLevel.Error, message, logProperties, ex);
        }

        public static void Extended(this ILogger logger, LogLevel logLevel, string message, object logProperties,
            Exception ex = null, string exceptionTag = null)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

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
                if (string.IsNullOrEmpty(exceptionTag))
                {
                    exceptionTag = Guid.NewGuid().ToString();
                }

                for (var index = 0; index < allExceptions.Count; index++)
                {
                    ExtendedWithException(logger, logLevel, message, logProperties,
                        allExceptions[index], index + 1, allExceptions.Count, exceptionTag);
                }
            }
        }

        public static IScope BeginScope(this ILogger logger, string scopeName, object logProps = null)
        {
            var properties = ObjectToDictionaryConverter.Convert(logProps);
            return new Scope(logger, scopeName, properties);
        }

        private static void ExtendedWithException(ILogger logger, LogLevel logLevel, string message,
            object logProperties,
            Exception ex, int exceptionIndex, int exceptionCount, string tag)
        {
            var log = new LogEventInfo(logLevel, logger.Name, message);
            TransferDataObjectToLogEventProperties(log, logProperties);
            TransferContextDataToLogEventProperties(log);
            TransferScopeDataToLogEventProperties(log);

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

        private static void TransferDataObjectToLogEventProperties(LogEventInfo log, object logProperties)
        {
            if (logProperties == null || logProperties is string)
            {
                return;
            }

            var properties = logProperties as IDictionary ??
                ObjectToDictionaryConverter.Convert(logProperties);

            foreach (var key in properties.Keys)
            {
                log.Properties.Add(key, properties[key]);
            }
        }

        private static void TransferContextDataToLogEventProperties(LogEventInfo log)
        {
            foreach (var contextItemName in MappedDiagnosticsLogicalContext.GetNames())
            {
                var key = contextItemName;
                if (log.Properties.ContainsKey(contextItemName))
                {
                    key = "log_context_" + contextItemName;
                }

                if (!log.Properties.ContainsKey(key))
                {
                    var value = MappedDiagnosticsLogicalContext.Get(contextItemName, CultureInfo.InvariantCulture);
                    log.Properties.Add(key, value);
                }
            }
        }

        private static void TransferScopeDataToLogEventProperties(LogEventInfo log)
        {
            var currentScope = NestedDiagnosticsLogicalContext.PeekObject() as Scope;
            if (currentScope == null)
            {
                return;
            }

            const string scopePropertyName = "Scope";
            log.Properties.Add(scopePropertyName, currentScope.ScopeName);
            log.Properties.Add(nameof(currentScope.ScopeTrace), currentScope.ScopeTrace);
            log.Properties.Add(nameof(currentScope.ScopeId), currentScope.ScopeId.ToString());
            log.Properties.Add(nameof(currentScope.ScopeIdTrace), currentScope.ScopeIdTrace);

            foreach (var property in currentScope.Properties)
            {
                var key = property.Key;
                if (log.Properties.ContainsKey(key))
                {
                    key = "log_scope_" + key;
                }

                // to omit multiple nesting
                if (!log.Properties.ContainsKey(key))
                {
                    log.Properties.Add(key, property.Value);
                }
            }
        }
    }
}
