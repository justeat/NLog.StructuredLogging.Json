using System;
using NLog.Config;
using NLog.Layouts;

namespace NLog.StructuredLogging.Json
{
    [NLogConfigurationItem]
    public class StructuredLoggingProperty
    {
        public StructuredLoggingProperty() { }

        public StructuredLoggingProperty(string name, Layout layout)
        {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The 'name' property cannot be null, empty or whitespace.");
            }

            Name = name;
        }

        public string Name { get; set; }

        public Layout Layout { get; set; }
    }
}
