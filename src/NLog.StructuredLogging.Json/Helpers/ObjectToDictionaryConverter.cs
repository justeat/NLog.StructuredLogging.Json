using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NLog.StructuredLogging.Json.Helpers
{
    public static class ObjectToDictionaryConverter
    {
        private static readonly ConcurrentDictionary<Type, DictionaryConverter> DictionaryConverters
        = new ConcurrentDictionary<Type, DictionaryConverter>();

        public static Dictionary<string, object> Convert(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is Dictionary<string, object> dictionary)
            {
                return dictionary;
            }

            var dictionaryConverter = GetConverter(value.GetType());
            return dictionaryConverter.ConvertFromObject(value);
        }

        public static DictionaryConverter GetConverter(Type type)
        {
            var dictionaryConverter = DictionaryConverters.GetOrAdd(type, t => new DictionaryConverter(t));
            return dictionaryConverter;
        }
    }

    public class DictionaryConverter
    {
        private static readonly MethodInfo MethodInfo = typeof(Dictionary<string, object>)
            .GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string), typeof(object) }, null);

        private readonly Func<object, Dictionary<string, object>> _parser;

        internal DictionaryConverter(Type type)
        {
            _parser = CreateParser(type);
        }

        public Dictionary<string, object> ConvertFromObject(object value)
        {
            return _parser(value);
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

            var expressionParameter = Expression.Parameter(typeof(object), "o");
            var invocationExpression = Expression.Invoke(Expression.Lambda(block, parameter), Expression.Convert(expressionParameter, objType));
            var lambda = Expression.Lambda<Func<object, Dictionary<string, object>>>(invocationExpression, expressionParameter);
            return lambda.Compile();
        }
    }
}
