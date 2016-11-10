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
    public class JsonWithPropertiesLayout: Layout
    {

        [ArrayParameter(typeof(StructuredLoggingProperty), "property")]
        public IList<StructuredLoggingProperty> Properties { get; private set; }

        public JsonWithPropertiesLayout()
        {
            this.Properties = new List<StructuredLoggingProperty>();
        }

        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            var dictionary = Mapper.ToDictionary(logEvent);
            foreach (var property in Properties)
            {
                if (dictionary.ContainsKey(property.Name)) {
                    throw new NLogConfigurationException("There is already an entry for '{0}'. It is probably a property of the LogEventInfo object and you can't override this. Try giving your property a different name ", property.Name);
                }
                dictionary.Add(property.Name, property.Layout.Render(logEvent));
            }
            var json = ConvertJson.Serialize(dictionary);
            return json;
        }
    }
}