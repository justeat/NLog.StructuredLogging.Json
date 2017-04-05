using System;

namespace NLog.StructuredLogging.Json.Tests
{
    public static class Env
    {
#if NET462
        public static bool HasCallSite { get  => true; }
#else
        public static bool HasCallSite { get => false; }
#endif
        public static string LocalLineEndings(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Replace("{br}", Environment.NewLine);
        }
    }
}