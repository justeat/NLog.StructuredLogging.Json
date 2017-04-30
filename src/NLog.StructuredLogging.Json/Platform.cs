namespace NLog.StructuredLogging.Json
{
    public static class Platform
    {
#if NET452
        public static bool HasCallSite => true;
#else
        public static bool HasCallSite => false;
#endif
    }
}
