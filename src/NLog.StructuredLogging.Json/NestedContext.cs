using System;
using System.Collections.Generic;
using System.Linq;

namespace NLog.StructuredLogging.Json
{
    internal class NestedContext : IDisposable, INestedContext
    {
        private const string ScopePropertyName = "Scope";
        private const string ScopeIdPropertyName = "ScopeId";
        private const string ScopeTracePropertyName = "ScopeTrace";

        private readonly ILogger _logger;
        private readonly IDisposable _disposable;
        private readonly NestedContext _parentScope;    
        private readonly ScopeConfiguration _scopeConfiguration;
        private readonly Guid _scopeId;
        private readonly string _scope;
        private readonly string _scopeTrace;
        private readonly string _scopeNameTrace;
        private readonly Dictionary<string, object> _properties;
        private Dictionary<string, object> _attachProperties;        
        public IReadOnlyDictionary<string, object> Properties => _attachProperties;

        public NestedContext(ILogger logger, string scope, IDictionary<string, object> properties)
        {
            _scopeConfiguration = new ScopeConfiguration();

            _logger = logger;

            _parentScope = GetParentScope();

            _scopeId = Guid.NewGuid();
            _scope = scope;
            _scopeTrace = GetScopeTrace();
            _scopeNameTrace = GetScopeNameTrace();

            _properties = AttachScopeProperties(new Dictionary<string, object>(properties));
            _attachProperties = AttachScopeProperties(new Dictionary<string, object>(3));
            
            AttachParentProperties(_properties);
            AttachParentProperties(_attachProperties);

            _logger.Extended(LogLevel.Trace, "Start logical scope", _properties);

            _disposable = NestedDiagnosticsLogicalContext.Push(this);
        }        

        public INestedContext WithInheritedConfiguration()
        {
            var parentScopeScopeConfiguration = _parentScope?._scopeConfiguration;
            if (parentScopeScopeConfiguration != null)
            {
                WithConfiguration(parentScopeScopeConfiguration);
            }

            return this;
        }

        public INestedContext WithConfiguration(ScopeConfiguration scopeConfiguration)
        {
            ApplyIncludePropertiesChanged(scopeConfiguration);
            ApplyIncludeNamedScopeTraceChanged(scopeConfiguration);

            return this;
        }

        public INestedContext WithConfiguration(Action<ScopeConfiguration> scopeConfiguration)
        {
            var newConfiguration = new ScopeConfiguration();
            scopeConfiguration(newConfiguration);

            WithConfiguration(newConfiguration);

            return this;
        }

        public void Dispose()
        {
            _logger.Extended(LogLevel.Trace, "Finish logical scope", _properties);
            _disposable?.Dispose();

            _properties.Clear();
            _attachProperties.Clear();

            GC.SuppressFinalize(this);
        }

        private void ApplyIncludePropertiesChanged(ScopeConfiguration scopeConfiguration)
        {
            if (_scopeConfiguration.IncludeProperties != scopeConfiguration.IncludeProperties)
            {
                if (scopeConfiguration.IncludeProperties)
                {
                    _attachProperties = _properties;
                }
                else
                {
                    _attachProperties = AttachScopeProperties(new Dictionary<string, object>(3));
                    _attachProperties = AttachParentProperties(_attachProperties);
                }

                _scopeConfiguration.IncludeProperties = scopeConfiguration.IncludeProperties;
            }
        }

        private void ApplyIncludeNamedScopeTraceChanged(ScopeConfiguration scopeConfiguration)
        {
            if (_scopeConfiguration.IncludeNamedScopeTrace != scopeConfiguration.IncludeNamedScopeTrace)
            {
                if (scopeConfiguration.IncludeNamedScopeTrace)
                {
                    _attachProperties[ScopeTracePropertyName] = _scopeNameTrace;
                }
                else
                {
                    _attachProperties.Remove(ScopeTracePropertyName);
                }

                _scopeConfiguration.IncludeNamedScopeTrace = scopeConfiguration.IncludeNamedScopeTrace;
            }
        }        

        private NestedContext GetParentScope()
        {
            var allObjects = NestedDiagnosticsLogicalContext.GetAllObjects();
            return allObjects?.FirstOrDefault() as NestedContext;
        }

        private string GetScopeTrace()
        {
            return _parentScope == null ?
                $"{_scopeId}" :
                $"{_parentScope._scopeTrace} -> {_scopeId}";
        }

        private string GetScopeNameTrace()
        {
            return _parentScope == null ?
                $"{_scope}" :
                $"{_parentScope._scopeNameTrace} -> {_scope}";
        }

        private Dictionary<string, object> AttachParentProperties(Dictionary<string, object> target)
        {
            if (_parentScope == null)
                return target;

            return AttachProperties(target, _parentScope._attachProperties);
        }

        private static Dictionary<string, object> AttachProperties(Dictionary<string, object> target, Dictionary<string, object> source)
        {            
            foreach (var property in source)
            {
                if (property.Key == ScopePropertyName ||
                    property.Key == ScopeIdPropertyName ||
                    property.Key == ScopeTracePropertyName) continue;

                var prefix = string.Empty;
                while (target.ContainsKey(prefix + property.Key)) prefix += "scoped_";
                target.Add(prefix + property.Key, property.Value);
            }

            return target;
        }

        private Dictionary<string, object> AttachScopeProperties(Dictionary<string, object> properties)
        {
            properties.Add(ScopePropertyName, _scope);
            properties.Add(ScopeIdPropertyName, _scopeId.ToString());
            properties.Add(ScopeTracePropertyName, _scopeTrace);

            return properties;
        }        
    }
}