using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NLog.Config;
using NLog.Layouts;
using NLog.StructuredLogging.Json.Helpers;

namespace NLog.StructuredLogging.Json
{
    /// <summary>
    ///     Similar to the <see cref="StructuredLoggingLayoutRenderer"/> with additional
    ///     properties that take a name and a layout.
    /// </summary>
    /// <example>
    ///   <target name="MyTarget"
    ///          xsi:type="file"
    ///          fileName="MyFilePath"
    ///          encoding="utf-8">
    ///     <layout xsi:type="JsonWithProperties">
    ///       <property name="SomePropertyName" layout="${literal:text=SomePropertyValue}" />
    ///       <property name="NormalisedMessage" layout="${replace:inner=${message}:searchFor=\\r\\n|\\n:replaceWith=~~~~~:regex=true}" />
    ///       <property name="NormalisedException" layout="${replace:inner=${exception:format=type,message,method,stacktrace:maxInnerExceptionLevel=10:innerFormat=shortType,message,method,stacktrace}:searchFor=\\r\\n|\\n:replaceWith=~~~~~:regex=true}" />
    ///       <property name="Uri" layout="${event-context:item=Uri}" />
    ///       <property name="MachineName" layout="${machinename}" />
    ///       <property name="ComponentVersion" layout="1.0.0.0" />
    ///     </layout>
    ///   </target>
    /// </example>
    [Layout("jsonwithproperties")]
    [ThreadSafe]
    public class JsonWithPropertiesLayout : Layout
    {
        private JsonSerializer JsonSerializer => _jsonSerializer ?? (_jsonSerializer = ConvertJson.CreateJsonSerializer());
        private JsonSerializer _jsonSerializer;

        [ArrayParameter(typeof(StructuredLoggingProperty), "property")]
        public IList<StructuredLoggingProperty> Properties { get; private set; }

        public const string PropertyNamePrefix = "properties_";

        public JsonWithPropertiesLayout()
        {
            Properties = new List<StructuredLoggingProperty>();
        }

        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            Dictionary<string, object> dictionary = BuildPropertiesDictionary(logEvent);
            return ConvertJson.Serialize(dictionary);
        }

        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            var result = BuildPropertiesDictionary(logEvent);
            var orgLength = target.Length;

            try
            {
                // Ensure we are threadsafe
                var jsonSerializer = JsonSerializer;
                lock (jsonSerializer)
                {
                    // Serialize directly into StringBuilder
                    ConvertJson.Serialize(result, target, jsonSerializer);
                }
            }
            catch
            {
                _jsonSerializer = null; // Do not reuse JsonSerializer, as it might have become broken
                target.Length = orgLength;  // Skip invalid JSON
                throw;
            }
        }

        private Dictionary<string, object> BuildPropertiesDictionary(LogEventInfo logEvent)
        {
            var dictionary = Mapper.ToDictionary(logEvent);

            // Enumerate without allocation of GetEnumerator()
            for (int i = 0; i < Properties.Count; ++i)
            {
                var property = Properties[i];
                AddRenderedValue(logEvent, dictionary, property);
            }

            return dictionary;
        }

        private static void AddRenderedValue(
            LogEventInfo source, IDictionary<string, object> dest,
            StructuredLoggingProperty property)
        {
            string renderedValue;
            try
            {
                renderedValue = property.Layout.Render(source);
            }
            catch (Exception ex)
            {
                renderedValue = $"Render failed: {ex.GetType().Name} {ex.Message}";
            }

            if (string.IsNullOrEmpty(renderedValue))
            {
                return;
            }

            Mapper.HarvestStringToDictionary(dest,
                property.Name, renderedValue, "properties_");
        }
    }
}
