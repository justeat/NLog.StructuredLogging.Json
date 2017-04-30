namespace NLog.StructuredLogging.Json
{
    public static class Platform
    {
#if NET452
        public static bool HasCallSite => true;
        public static bool HasProcessId => true;
#else
        public static bool HasCallSite => false;
        public static bool HasProcessId => true;
#endif
    }
}
