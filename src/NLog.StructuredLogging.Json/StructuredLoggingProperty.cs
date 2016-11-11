using System;
using NLog.Layouts;

namespace NLog.StructuredLogging.Json
{
    public class StructuredLoggingProperty
    {
        public StructuredLoggingProperty() {}

        public StructuredLoggingProperty(string name, Layout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException("layout");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The 'name' property cannot be null, empty or whitespace.");
            }

            Name = name;
            Layout = layout;
        }

        public string Name { get; set; }

        public Layout Layout { get; set; }
    }
}
