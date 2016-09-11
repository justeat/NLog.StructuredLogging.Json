using NLog.Layouts;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayout
{
    public abstract class UnfortunatelyComplexFlattenedJsonLayoutTests :
        UnfortunatelyComplexEndToEndTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork
    {
        protected override Layout GivenLayout()
        {
            return new FlattenedJsonLayout();
        }
    }
}
