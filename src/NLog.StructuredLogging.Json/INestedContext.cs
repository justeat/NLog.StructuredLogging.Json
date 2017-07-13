using System;
using System.Collections.Generic;

namespace NLog.StructuredLogging.Json
{
    public interface INestedContext : IDisposable
    {
        Guid ScopeId { get; }
        string Scope { get; }
        IReadOnlyDictionary<string, object> Properties { get; }
    }
}
