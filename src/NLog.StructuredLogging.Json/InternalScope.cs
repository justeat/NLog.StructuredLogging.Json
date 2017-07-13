using System;
using System.Collections.Generic;
using System.Linq;

namespace NLog.StructuredLogging.Json
{
    internal class InternalScope : IScope
    {
        private readonly ILogger _logger;
        private readonly IDisposable _disposable;
        private readonly InternalScope _parentScope;
        private readonly Dictionary<string, object> _properties;

        public string Scope { get; }
        public string ScopeTrace { get; }
        public Guid ScopeId { get; }        
        public string ScopeIdTrace { get; }
        public IReadOnlyDictionary<string, object> Properties => _properties;

        public InternalScope(ILogger logger, string scope, IDictionary<string, object> properties)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _parentScope = GetParentScope();

            Scope = scope;
            ScopeId = Guid.NewGuid();
            ScopeIdTrace = GetScopeTrace();
            ScopeTrace = GetScopeNameTrace();

            _properties = ConstructScopeProperties(properties);

            _disposable = NestedDiagnosticsLogicalContext.Push(this);

            _logger.Extended(LogLevel.Trace, "Start logical scope", null);
        }

        public void Dispose()
        {
            _logger.Extended(LogLevel.Trace, "Finish logical scope", null);

            _disposable?.Dispose();

            _properties.Clear();

            GC.SuppressFinalize(this);
        }

        private Dictionary<string, object> ConstructScopeProperties(IDictionary<string, object> currentScopeProperties)
        {
            var properties = currentScopeProperties == null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(currentScopeProperties);

            if (_parentScope != null)
            {
                foreach (var property in _parentScope.Properties)
                {
                    var key = property.Key;
                    if (properties.ContainsKey(key))
                    {
                        key = "nested_" + key;
                    }

                    if (!properties.ContainsKey(key))
                    {
                        properties.Add(key, property.Value);
                    }
                }
            }

            return properties;
        }        

        private static InternalScope GetParentScope()
        {
            var allObjects = NestedDiagnosticsLogicalContext.GetAllObjects();
            return allObjects?.FirstOrDefault() as InternalScope;
        }

        private string GetScopeTrace() => _parentScope == null ? $"{ScopeId}" : $"{_parentScope.ScopeIdTrace} -> {ScopeId}";

        private string GetScopeNameTrace() => _parentScope == null ? $"{Scope}" : $"{_parentScope.ScopeTrace} -> {Scope}";
    }
}
