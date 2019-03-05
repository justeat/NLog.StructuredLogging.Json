using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NLog.StructuredLogging.Json.Helpers
{
    internal static class StackHelper
    {
        internal static string CallSiteName(StackTrace stackTrace)
        {
            try
            {
                var stackFrames = stackTrace.GetFrames();
                if (stackFrames == null || (stackFrames.Length == 0))
                {
                    return "No stack frames";
                }

                var firstMethod = FirstCallingMethod(stackFrames);
                if (firstMethod == null)
                {
                    return "No method";
                }

                if (firstMethod.DeclaringType == null)
                {
                    return firstMethod.Name;
                }

                return firstMethod.DeclaringType.FullName + "." + firstMethod.Name;

            }
            catch (Exception ex)
            {
                return "Error reading CallSite: " + ex.Message;
            }
        }

        private static MethodBase FirstCallingMethod(StackFrame[] stackFrames)
        {
            return stackFrames
                .Select(x => x.GetMethod())
                .FirstOrDefault(IsUserMethod);
        }

        internal static int IndexOfFirstCallingMethod(StackFrame[] stackFrames)
        {
            for (var index = 0; index < stackFrames.Length; index++)
            {
                var method = stackFrames[index].GetMethod();
                if (IsUserMethod(method))
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool IsUserMethod(MethodBase method)
        {
            return method.DeclaringType != typeof(LoggerExtensions);
        }
    }
}
