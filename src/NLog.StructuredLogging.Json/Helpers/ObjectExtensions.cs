using System;

namespace NLog.StructuredLogging.Json.Helpers
{
    public static class ObjectExtensions
    {
        public static bool IsNonStringValueType(this object value)
        {
            switch (System.Convert.GetTypeCode(value))
            {
                case TypeCode.Boolean:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt64:
                case TypeCode.UInt32:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.Byte:
                case TypeCode.SByte:
                    return true;

                default: return false;
            }
        }
    }
}
