using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NLog.StructuredLogging.Json
{
    internal class NestedContext : IDisposable, INestedContext
    {
        private readonly object _lock = new object();
        private readonly ILogger _logger;
        private readonly IDisposable _disposable;
        private readonly NestedContext _parentScope;
        private readonly Dictionary<string, object> _properties;
        private CalculatedContext _calculatedLogProperties;
        private readonly Dictionary<string, object> _logProperties;

        public Guid ScopeId { get; }
        public string Scope { get; }
        public string ScopeIdTrace { get; }
        public string ScopeNameTrace { get; }
        public ScopeConfiguration ScopeConfiguration { get; private set; }
        public IReadOnlyDictionary<string, object> Properties => _properties;


        public NestedContext(ILogger logger, string scope, IDictionary<string, object> properties,
            ScopeConfiguration configuration = null)
        {
            _logger = logger;

            _parentScope = GetParentScope();

            CreateScopeConfiguration(configuration);

            ScopeId = Guid.NewGuid();
            Scope = scope;
            ScopeIdTrace = GetScopeTrace();
            ScopeNameTrace = GetScopeNameTrace();

            _properties = new Dictionary<string, object>(properties);

            _disposable = NestedDiagnosticsLogicalContext.Push(this);

            _logProperties = ScopeConfiguration.IncludeProperties ? null : _properties;

            _logger.Extended(LogLevel.Trace, "Start logical scope", _logProperties);            
        }

        public void Dispose()
        {
            _logger.Extended(LogLevel.Trace, "Finish logical scope", _logProperties);

            _disposable?.Dispose();

            _properties.Clear();

            GC.SuppressFinalize(this);
        }

        public IEnumerable<KeyValuePair<string, object>> GetOrCalculateProperties(Action<CalculatedContext> action)
        {
            if (_calculatedLogProperties == null)
            {
                lock (_lock)
                {
                    if (_calculatedLogProperties == null)
                    {
                        _calculatedLogProperties = new CalculatedContext();
                        action(_calculatedLogProperties);
                    }
                }
            }

            return _calculatedLogProperties;
        }

        private NestedContext GetParentScope()
        {
            var allObjects = NestedDiagnosticsLogicalContext.GetAllObjects();
            return allObjects?.FirstOrDefault() as NestedContext;
        }

        private string GetScopeTrace()
        {
            if (!ScopeConfiguration.IncludeScopeIdTrace)
            {
                return null;
            }

            return _parentScope == null ? $"{ScopeId}" : $"{_parentScope.ScopeIdTrace} -> {ScopeId}";
        }

        private string GetScopeNameTrace()
        {
            if (!ScopeConfiguration.IncludeScopeNameTrace)
            {
                return null;
            }

            return _parentScope == null ? $"{Scope}" : $"{_parentScope.ScopeNameTrace} -> {Scope}";
        }

        private void CreateScopeConfiguration(ScopeConfiguration configuration)
        {
            if (configuration == null)
            {
                ScopeConfiguration = new ScopeConfiguration();
            }
            else if(configuration.InheritConfiguration)
            {
                WithInheritedConfiguration();
            }
            else
            {
                ScopeConfiguration = new ScopeConfiguration(configuration);
            }            
        }

        private void WithInheritedConfiguration()
        {
            var parentScopeScopeConfiguration = _parentScope?.ScopeConfiguration;
            if (parentScopeScopeConfiguration != null)
            {
                ScopeConfiguration = new ScopeConfiguration(parentScopeScopeConfiguration);
            }
        }

        internal class CalculatedContext : IEnumerable<KeyValuePair<string, object>>
        {
            private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

            public void Add(string key, object obj) => _properties.Add(key, obj);

            public bool Contains(string key) => _properties.ContainsKey(key);

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                foreach (var property in _properties)
                {
                    yield return property;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
