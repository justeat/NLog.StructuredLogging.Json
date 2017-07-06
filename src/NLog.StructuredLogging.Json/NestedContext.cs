using System;
using System.Collections.Generic;
using System.Linq;
using NLog.StructuredLogging.Json.Helpers;

namespace NLog.StructuredLogging.Json
{
    internal class NestedContext : IDisposable, INestedContext
    {
        private readonly ILogger _logger;
        private readonly IDisposable _disposable;
        private readonly NestedContext _parentScope;

        public INestedContext ParentScope => _parentScope;

        public Guid ScopeId { get; }
        public string Scope { get; }
        public string ScopeTrace { get; }

        public Dictionary<string, object> Properties { get; }

        public Dictionary<string, object> AttachProperties { get; private set; }

        public NestedContext(ILogger logger, string scope, IDictionary<string, object> properties)
        {
            _logger = logger;            

            _parentScope = (NestedDiagnosticsLogicalContext.GetAllObjects() as NestedContext[])?.FirstOrDefault();

            ScopeId = Guid.NewGuid();
            Scope = scope;
            ScopeTrace = GetScopeTrace();
            Properties = AttachScopeProperties(new Dictionary<string, object>(properties));
            AttachProperties = AttachScopeProperties(new Dictionary<string, object>(3));
            ;
            AttachParentProperties(Properties);
            AttachParentProperties(AttachProperties);

            _logger.Extended(LogLevel.Trace, "Start logical scope", Properties);

            _disposable = NestedDiagnosticsLogicalContext.Push(this);
        }

        public void Dispose()
        {
            _logger.Extended(LogLevel.Trace, "Finish logical scope", null);
            _disposable?.Dispose();
        }

        public IDisposable WithProperties()
        {
            AttachProperties = new Dictionary<string, object>(Properties);
            AttachParentProperties(AttachProperties);

            return this;
        }

        public IDisposable WithoutProperties()
        {
            return this;
        }

        public IDisposable SpecificProperties(object logProps)
        {
            var properties = ObjectDictionaryParser.ConvertObjectToDictionaty(logProps);
            properties = AttachScopeProperties(properties);
            AttachProperties = new Dictionary<string, object>(properties);
            AttachParentProperties(AttachProperties);

            return this;
        }

        private string GetScopeTrace()
        {
            return ParentScope == null ?
                $"{ScopeId}" :
                $"{_parentScope.ScopeTrace} -> {ScopeId}";
        }

        private void AttachParentProperties(Dictionary<string, object> target)
        {
            if (ParentScope == null)
                return;

            foreach (var property in _parentScope.AttachProperties)
            {
                if (property.Key == nameof(Scope) ||
                    property.Key == nameof(ScopeId) ||
                    property.Key == nameof(ScopeTrace)) continue;

                var prefix = string.Empty;
                while (target.ContainsKey(prefix + property.Key)) prefix += "scoped_";
                target.Add(prefix + property.Key, property.Value);
            }
        }

        private Dictionary<string, object> AttachScopeProperties(Dictionary<string, object> properties)
        {
            properties.Add(nameof(Scope), Scope);
            properties.Add(nameof(ScopeId), ScopeId.ToString());
            properties.Add(nameof(ScopeTrace), ScopeTrace);

            return properties;
        }
    }
}