using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace NLog.StructuredLogging.Json.Helpers
{
    public static class Convert
    {
        public static string ToUtcIso8601(DateTimeOffset source)
        {
            return string.Concat(source.ToUniversalTime().ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff", CultureInfo.InvariantCulture), "Z");
        }

        public static string ValueAsString(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is DateTime)
            {
                return DateTimeValueAsIso8601String((DateTime)value);
            }

            if (value is DateTimeOffset)
            {
                return DateTimeOffsetValueAsIso8601String((DateTimeOffset)value);
            }

            return value.ToString();
        }

        private static string DateTimeOffsetValueAsIso8601String(DateTimeOffset value)
        {
            if (value.Offset == TimeSpan.Zero)
            {
                // do this to make it end in "Z" not "+00:00"
                return DateTimeValueAsIso8601String(value.UtcDateTime);
            }

            return value.ToString("O", CultureInfo.InvariantCulture);
        }

        private static string DateTimeValueAsIso8601String(DateTime value)
        {
            return value.ToString("O", CultureInfo.InvariantCulture);
        }
    }

    internal static class ObjectDictionaryParser
    {
        private static readonly ConcurrentDictionary<Type, DictionaryParser> DictionaryParsers;

        static ObjectDictionaryParser()
        {
            DictionaryParsers = new ConcurrentDictionary<Type, DictionaryParser>();
        }

        public static Dictionary<string, object> ConvertObjectToDictionaty(object obj)
        {
            if (obj == null)
            {
                return new Dictionary<string, object>();
            }

            var dictionary = obj as Dictionary<string, object>;
            if (dictionary != null)
            {
                return dictionary;
            }

            var dictionaryParser = DictionaryParsers.GetOrAdd(obj.GetType(), t => new DictionaryParser(t));
            return dictionaryParser.Parse(obj);
        }

        internal class DictionaryParser
        {
            private static readonly MethodInfo MethodInfo = typeof(Dictionary<string, object>)
#if NET452
                .GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string), typeof(object) }, null);
#else
                .GetMethod("Add", new[] {typeof(string), typeof(object)});
#endif

            private readonly Func<object, Dictionary<string, object>> _parser;

            public DictionaryParser(Type type)
            {
                _parser = CreateParser(type);
            }

            public Dictionary<string, object> Parse(object obj)
            {
                return _parser(obj);
            }

            private static Func<object, Dictionary<string, object>> CreateParser(Type objType)
            {
                var dictionary = Expression.Variable(typeof(Dictionary<string, object>));
                var parameter = Expression.Parameter(objType, "obj");

                var body = new List<Expression>
                {
                    Expression.Assign(dictionary, Expression.New(typeof(Dictionary<string, object>)))
                };

                var properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                for (int i = 0; i < properties.Length; i++)
                {
                    // Skip write only or indexers
                    var property = properties[i];
                    if (!property.CanRead || property.GetIndexParameters().Length != 0)
                    {
                        continue;
                    }

                    var key = Expression.Constant(property.Name);
                    var value = Expression.Property(parameter, property);
                    // Boxing must be done manually... For reference type it isn't a problem casting to object
                    var valueAsObject = Expression.Convert(value, typeof(object));
                    body.Add(Expression.Call(dictionary, MethodInfo, key, valueAsObject));
                }

                // Return value
                body.Add(dictionary);

                var block = Expression.Block(new[] { dictionary }, body);

                var extPar = Expression.Parameter(typeof(object), "extPar");
                var invocationExpression = Expression.Invoke(Expression.Lambda(block, parameter), Expression.Convert(extPar, objType));
                var lambda = Expression.Lambda<Func<object, Dictionary<string, object>>>(invocationExpression, extPar);
                return lambda.Compile();
            }
        }
    }
}
