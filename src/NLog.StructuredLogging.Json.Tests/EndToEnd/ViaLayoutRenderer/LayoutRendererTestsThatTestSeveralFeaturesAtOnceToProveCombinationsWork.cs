using System.Collections.Generic;
using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayoutRenderer
{
    public class LayoutRendererTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork : UnfortunatelyComplexEndToEndTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork
    {
        protected override IDictionary<string, string> GivenAttributesNotYetAssertable()
        {
            return new Dictionary<string,string>
            {
                {"ProcessId", "StructuredLoggingLayoutRenderer does not output this"},
                {"ThreadId", "StructuredLoggingLayoutRenderer does not output this"}
            };
        }

        protected override Layout GivenLayout()
        {
            return "${structuredlogging.json}";
        }
    }
}
