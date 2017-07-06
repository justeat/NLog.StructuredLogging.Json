using System;
using System.Collections.Generic;

namespace NLog.StructuredLogging.Json
{
    public class ScopeConfiguration
    {
        public bool IncludeProperties { get; set; }
        public bool IncludeNamedScopeTrace { get; set; }
    }

    public interface INestedContext : IDisposable
    {
        INestedContext WithInheritedConfiguration();
        INestedContext WithConfiguration(ScopeConfiguration scopeConfiguration);
        INestedContext WithConfiguration(Action<ScopeConfiguration> scopeConfiguration);
        IReadOnlyDictionary<string, object> Properties { get; }
    }

    public static class NestedContextExtensions
    {
        public static INestedContext WithProperties(this INestedContext nestedContext)
        {
            return nestedContext.WithConfiguration(configuration => configuration.IncludeProperties = true);
        }        

        public static INestedContext WithoutProperties(this INestedContext nestedContext)
        {
            return nestedContext.WithConfiguration(configuration => configuration.IncludeProperties = false);
        }

        public static INestedContext WithNamedScopeTrace(this INestedContext nestedContext)
        {
            return nestedContext.WithConfiguration(configuration => configuration.IncludeNamedScopeTrace = true);
        }

        public static INestedContext WithoutNamedScopeTrace(this INestedContext nestedContext)
        {
            return nestedContext.WithConfiguration(configuration => configuration.IncludeNamedScopeTrace = false);
        }
    }
}