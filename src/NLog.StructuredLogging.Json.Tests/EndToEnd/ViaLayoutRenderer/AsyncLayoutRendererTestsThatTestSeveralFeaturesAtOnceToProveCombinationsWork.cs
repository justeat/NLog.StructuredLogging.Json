using NLog.Targets;
using NLog.Targets.Wrappers;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayoutRenderer
{
    public class AsyncLayoutRendererTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork : LayoutRendererTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork
    {
        protected override Target GivenTarget(string name)
        {
            return new AsyncTargetWrapper(base.GivenTarget(name))
            {
                Name = string.Format("wrapped_{0}", name),
                TimeToSleepBetweenBatches = 1,
                QueueLimit = 11
            };
        }
    }
}