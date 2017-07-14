using System;
using System.Collections.Generic;

namespace NLog.StructuredLogging.Json
{
    public interface IScope : IDisposable
    {
        Guid ScopeId { get; }
        string ScopeName { get; }
        IReadOnlyDictionary<string, object> Properties { get; }
    }
}
