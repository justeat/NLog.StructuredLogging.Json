using System;

namespace NLog.StructuredLogging.Json.Tests
{
    public static class Env
    {
#if NET462
        public static bool HasCallSite => true;
#else
        public static bool HasCallSite => false;
#endif
    }
}