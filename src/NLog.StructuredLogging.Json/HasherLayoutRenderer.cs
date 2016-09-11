using System.Text;
using NLog.Config;
using NLog.StructuredLogging.Json.Helpers;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;
using NLog.Layouts;

namespace NLog.StructuredLogging.Json
{
    [LayoutRenderer("hasher")]
    [ThreadAgnostic]
    [AppDomainFixedOutput]
    public class HasherLayoutRenderer : WrapperLayoutRendererBase
    {
        public Layout Text
        {
            get { return Inner; }
            set { Inner = value; }
        }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (logEvent.Exception != null)
            {
                var text = ConvertException.ToFingerprint(logEvent.Exception);
                builder.Append(text);
            }
            else
            {
                base.Append(builder, logEvent);
            }
        }

        protected override string Transform(string text)
        {
            return Sha1Hasher.Hash(text);
        }
    }
}