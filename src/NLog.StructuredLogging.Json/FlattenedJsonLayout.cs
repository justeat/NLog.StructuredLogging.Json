using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog.Config;
using NLog.StructuredLogging.Json.Helpers;
using NLog.LayoutRenderers.Wrappers;
using NLog.Layouts;
using Newtonsoft.Json;

namespace NLog.StructuredLogging.Json
{
    [Layout("FlattenedJsonLayout")]
    [ThreadSafe]
    public class FlattenedJsonLayout : JsonLayout
    {
        private JsonSerializer JsonSerializer => _jsonSerializer ?? (_jsonSerializer = ConvertJson.CreateJsonSerializer());
        private JsonSerializer _jsonSerializer;

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
            var result = BuildPropertiesDictionaryFlattened(logEvent);
            return ConvertJson.Serialize(result);
        }

        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            var result = BuildPropertiesDictionaryFlattened(logEvent);
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

        private Dictionary<string, object> BuildPropertiesDictionaryFlattened(LogEventInfo logEvent)
        {
            var result = new Dictionary<string, object>();
            AppendDataFromAttributes(logEvent, result);
            AppendLogProperties(logEvent, result);
            AppendLogParameters(logEvent, result);
            AppendExceptionData(logEvent, result);
            return result;
        }

        private void AppendDataFromAttributes(LogEventInfo logEvent, IDictionary<string, object> result)
        {
            if (Attributes.Count == 0)
                return;

            var layoutRendererWrapper = new JsonEncodeLayoutRendererWrapper();
            // Enumerate without allocation of GetEnumerator()
            for (int i = 0; i < Attributes.Count; ++i)
            {
                var jsonAttribute = Attributes[i];
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

            Add("CallSite", "${callsite}");

            Add("Exception", "${exception:format=ToString}");
            Add("ExceptionType", "${exception:format=ShortType}");
            Add("ExceptionMessage", "${exception:format=Message}");
            Add("ExceptionStackTrace", "${exception:format=StackTrace}");
            Add("ExceptionFingerprint", "${hasher:Inner=${exception:format=StackTrace}}");
        }

        private static void AppendExceptionData(LogEventInfo logEvent, IDictionary<string, object> result)
        {
            if (logEvent.Exception?.Data?.Count > 0)
            {
                Mapper.HarvestToDictionary(logEvent.Exception.Data, result, "ex_");
            }
        }

        private static void AppendLogProperties(LogEventInfo logEvent, IDictionary<string, object> result)
        {
            if (logEvent.HasProperties)
            {
                Mapper.HarvestToDictionary(logEvent.Properties, result, "data_");
            }
        }

        private static void AppendLogParameters(LogEventInfo logEvent, IDictionary<string, object> result)
        {
            if (logEvent.Parameters != null)
            {
                var value = string.Join(",", logEvent.Parameters.Select(x => x ?? "null").Select(x => x.ToString()));
                result.Add("Parameters", value);
            }
        }

        private void Add(string name, Layout layout, bool encode = false)
        {
            if (!Attributes.Any(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                Attributes.Add(new JsonAttribute(name, layout, encode));
            }
        }
    }
}
