using System;
using System.Collections.Generic;
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
    public class JsonWithPropertiesLayout : Layout
    {
        [ArrayParameter(typeof(StructuredLoggingProperty), "property")]
        public IList<StructuredLoggingProperty> Properties { get; private set; }

        public const string PropertyNamePrefix = "properties_";

        public JsonWithPropertiesLayout()
        {
            Properties = new List<StructuredLoggingProperty>();
        }

        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            var dictionary = Mapper.ToDictionary(logEvent);

            foreach (var property in Properties)
            {
                AddRenderedValue(logEvent, dictionary, property);
            }

            return ConvertJson.Serialize(dictionary);
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

            PropertiesHelper.Add(dest, property.Name, renderedValue);
        }
    }
}
