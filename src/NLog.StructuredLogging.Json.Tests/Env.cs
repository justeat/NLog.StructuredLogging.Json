using System;

namespace NLog.StructuredLogging.Json.Tests
{
    public static class Env
    {
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