using System.Collections.Generic;

namespace NLog.StructuredLogging.Json.Helpers
{
    public static class PropertiesHelper
    {
        public static void Add(IDictionary<string, object> dest, string name, string value)
        {
            if (!dest.ContainsKey(name))
            {
                dest.Add(name, value);
                return;
            }

            const string propertyNamePrefix = "properties_";
            var namespacedName = propertyNamePrefix + name;
            if (!dest.ContainsKey(namespacedName))
            {
                dest.Add(namespacedName, value);
            }
        }
    }
}
