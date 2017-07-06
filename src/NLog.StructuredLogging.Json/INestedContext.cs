using System;

namespace NLog.StructuredLogging.Json
{
    public interface INestedContext
    {
        IDisposable WithProperties();
        IDisposable WithoutProperties();
        IDisposable SpecificProperties(object logProps);
    }
}