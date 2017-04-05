using System;
using System.Collections.Generic;
using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayoutRenderer
{
    public class LayoutRendererTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork : UnfortunatelyComplexEndToEndTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork
    {
        protected override IDictionary<string, string> GivenAttributesNotYetAssertable()
        {
            var result = new Dictionary<string,string>
            {
                {"ProcessId", "StructuredLoggingLayoutRenderer does not output this"},
                {"ThreadId", "StructuredLoggingLayoutRenderer does not output this"}
            };

            if (!Env.HasCallSite)
            {
                result.Add("CallSite", "Cannot yet generate CallSite in dotNet core");
            }

            return result;
        }

        protected override Layout GivenLayout()
        {
            return "${structuredlogging.json}";
        }
    }
}
