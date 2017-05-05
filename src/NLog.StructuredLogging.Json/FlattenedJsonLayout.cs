using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Config;
using NLog.StructuredLogging.Json.Helpers;
using NLog.LayoutRenderers.Wrappers;
using NLog.Layouts;

namespace NLog.StructuredLogging.Json
{
    [Layout("FlattenedJsonLayout")]
    [AppDomainFixedOutput]
    public class FlattenedJsonLayout : JsonLayout
    {
        public FlattenedJsonLayout()
        {
            SuppressSpaces = true;
        }

        protected override void InitializeLayout()
        {
            AddAttributesForStandardThings();
            base.InitializeLayout();
        }

        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            var result = new Dictionary<string, object>();

            AppendDataFromAttributes(logEvent, result);
            AppendLogProperties(logEvent, result);
            AppendLogParameters(logEvent, result);
            AppendExceptionData(logEvent, result);

            return ConvertJson.Serialize(result);
        }

        private void AppendDataFromAttributes(LogEventInfo logEvent, IDictionary<string, object> result)
        {
            var layoutRendererWrapper = new JsonEncodeLayoutRendererWrapper();
            foreach (var jsonAttribute in Attributes)
            {
                AddRenderedValue(logEvent, result, layoutRendererWrapper, jsonAttribute);
            }
        }

        private static void AddRenderedValue(
            LogEventInfo source, IDictionary<string, object> dest,
            JsonEncodeLayoutRendererWrapper renderer, JsonAttribute attribute)
        {
            renderer.Inner = attribute.Layout;
            renderer.JsonEncode = attribute.Encode;

            string renderedValue;
            try
            {
                renderedValue = renderer.Render(source);
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
                attribute.Name, renderedValue, "attributes_");
        }

        private void AddAttributesForStandardThings()
        {
            Add("TimeStamp", "${date:format=yyyy-MM-ddTHH\\:mm\\:ss.fffZ}");
            Add("Level", "${level}");
            Add("LoggerName", "${logger}");
            Add("Message", "${message}");
            Add("ProcessId", "${processid}");
            Add("ThreadId", "${threadid}");

            if (Platform.HasCallSite)
            {
                Add("CallSite", "${callsite}");
            }

            Add("Exception", "${exception:format=ToString}");
            Add("ExceptionType", "${exception:format=ShortType}");
            Add("ExceptionMessage", "${exception:format=Message}");
            Add("ExceptionStackTrace", "${exception:format=StackTrace}");
            Add("ExceptionFingerprint", "${hasher:Inner=${exception:format=StackTrace}}");
        }

        private static void AppendExceptionData(LogEventInfo logEvent, IDictionary<string, object> result)
        {
            if (logEvent.Exception == null)
            {
                return;
            }

            Mapper.HarvestToDictionary(logEvent.Exception.Data, result, "ex_");
        }

        private static void AppendLogProperties(LogEventInfo logEvent, IDictionary<string, object> result)
        {
            Mapper.HarvestToDictionary(logEvent.Properties, result, "data_");
        }

        private static void AppendLogParameters(LogEventInfo logEvent, IDictionary<string, object> result)
        {
            if (logEvent.Parameters == null)
            {
                return;
            }

            var value = string.Join(",", logEvent.Parameters.Select(x => x ?? "null").Select(x => x.ToString()));

            result.Add("Parameters", value);
        }

        private void Add(string name, Layout layout, bool encode = false)
        {
            if (! Attributes.Any(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                Attributes.Add(new JsonAttribute(name, layout, encode));
            }
        }
    }
}
