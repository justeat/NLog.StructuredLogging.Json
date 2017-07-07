using NLog.Config;
using NLog.Layouts;

namespace NLog.StructuredLogging.Json
{
    public class ScopeConfiguration
    {
        public bool IncludeProperties { get; set; }
        public bool IncludeScopeNameTrace { get; set; }
        public bool IncludeScopeIdTrace { get; set; }
        public bool InheritConfiguration { get; set; }

        internal ScopeConfiguration(ScopeConfiguration scopeConfiguration)
        {
            IncludeProperties = scopeConfiguration.IncludeProperties;
            IncludeScopeNameTrace = scopeConfiguration.IncludeScopeNameTrace;
            IncludeScopeIdTrace = scopeConfiguration.IncludeScopeIdTrace;
        }

        public ScopeConfiguration()
        {
            LoggingConfiguration configuration = LogManager.Configuration;

            SimpleLayout simpleLayout;
            if (configuration.Variables.TryGetValue("inherit_scope_configuration", out simpleLayout))
            {
                InheritConfiguration = bool.Parse(simpleLayout.FixedText);
            }
            else
            {
                InheritConfiguration = false;
            }

            if (configuration.Variables.TryGetValue("include_scope_properties", out simpleLayout))
            {
                IncludeProperties = bool.Parse(simpleLayout.FixedText);
            }
            else
            {
                IncludeProperties = true;
            }

            if (configuration.Variables.TryGetValue("include_scope_name_trace", out simpleLayout))
            {
                IncludeScopeNameTrace = bool.Parse(simpleLayout.FixedText);
            }
            else
            {
                IncludeScopeNameTrace = true;
            }

            if (configuration.Variables.TryGetValue("include_scope_id_trace", out simpleLayout))
            {
                IncludeScopeIdTrace = bool.Parse(simpleLayout.FixedText);
            }
            else
            {
                IncludeScopeIdTrace = true;
            }
        }
    }
}