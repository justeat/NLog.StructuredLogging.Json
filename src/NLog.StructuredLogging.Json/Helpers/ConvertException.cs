using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace NLog.StructuredLogging.Json.Helpers
{
    public static class ConvertException
    {
        public static IList<Exception> ToList(Exception ex)
        {
            var result = new List<Exception>();
            PopulateAllInnerExceptions(ex, result);
            return result;
        }

        private static void PopulateAllInnerExceptions(Exception ex, IList<Exception> result)
        {
            if (ex == null)
            {
                return;
            }

            result.Add(ex);

            var aggregateException = ex as AggregateException;
            if (aggregateException != null)
            {
                foreach (Exception inner in aggregateException.Flatten().InnerExceptions)
                {
                    PopulateAllInnerExceptions(inner, result);
                }
            }
            else if (ex.InnerException != null)
            {
                PopulateAllInnerExceptions(ex.InnerException, result);
            }
        }

        public static string ToFingerprint(Exception exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }

            return Sha1Hasher.Hash(StringForHashing(exception));
        }

        private static string StringForHashing(Exception exception)
        {
            try
            {
                if (exception.StackTrace != null)
                {
                    return StackTraceWithoutFilePaths(exception);
                }

                return exception.Message;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Since we find that with a CI server,
        /// file paths vary from build to build even if the code is identical
        /// Make a version of the stack trace but without file paths, for hashing
        /// </summary>
        /// <param name="exception"></param>
        /// <returns>A stack trace</returns>
        private static string StackTraceWithoutFilePaths(Exception exception)
        {
            var stackTrace = new StackTrace(exception, true);
            var frames = stackTrace.GetFrames();
            var firstFrame = true;
            var sb = new StringBuilder(255);

            for (var i = 0; i < frames.Length; ++i)
            {
                var stackFrame = frames[i];
                var method = stackFrame.GetMethod();
                if (method != null)
                {
                    if (!firstFrame)
                    {
                        sb.Append(Environment.NewLine);
                    }

                    firstFrame = false;

                    sb.Append("   at ");

                    // Method name
                    var declaringType = method.DeclaringType;
                    if (declaringType != null)
                    {
                        sb.Append(declaringType.FullName.Replace('+', '.'));
                        sb.Append(".");
                    }

                    sb.Append(method.Name);

                    var methodInfo = method as MethodInfo;

                    // Generic arguments
                    if (methodInfo != null && methodInfo.IsGenericMethod)
                    {
                        var genericArguments = methodInfo.GetGenericArguments();
                        sb.Append("[");

                        var firstGenericArgument = true;
                        foreach (var genericArgument in genericArguments)
                        {
                            if (!firstGenericArgument)
                            {
                                sb.Append(",");
                            }

                            firstGenericArgument = false;

                            sb.Append(genericArgument.Name);
                        }

                        sb.Append("]");
                    }

                    // Parameters
                    sb.Append("(");
                    var parameterInfos = method.GetParameters();
                    var firstParam = true;
                    foreach (var parameterInfo in parameterInfos)
                    {
                        if (!firstParam)
                        {
                            sb.Append(", ");
                        }

                        firstParam = false;

                        var typeName = parameterInfo.ParameterType.Name;
                        sb.Append(typeName + " " + parameterInfo.Name);
                    }

                    sb.Append(")");

                    // Line number
                    var lineNumber = stackFrame.GetFileLineNumber();
                    if (lineNumber != 0)
                    {
                        sb.Append($" line: {stackFrame.GetFileLineNumber()}");
                    }
                }
            }

            return sb.ToString();
        }
    }
}
