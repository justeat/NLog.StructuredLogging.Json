using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLog.StructuredLogging.Json
{
    public interface INestedContext : IDisposable
    {
        Guid ScopeId { get; }
        string Scope { get; }
        IReadOnlyDictionary<string, object> Properties { get; }
    }
}
