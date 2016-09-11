using System.Collections.Generic;
using System.Linq;
using NLog.Layouts;
using NUnit.Framework;
using Shouldly;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayout
{
    public class FlattenedJsonTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork : UnfortunatelyComplexFlattenedJsonLayoutTests
    {
        protected override IList<string> GivenAttributesOnLogEvent()
        {
            var attributesOnLogEvent = base.GivenAttributesOnLogEvent();
            attributesOnLogEvent.Add("flat1");
            return attributesOnLogEvent;
        }

        protected override IEnumerable<JsonAttribute> GivenControlOutputAttributes()
        {
            var control = base.GivenControlOutputAttributes().ToList();
            control.Add(new JsonAttribute("flat1", "flat1"));
            return control;
        }

        protected override Layout GivenLayout()
        {
            var layout = new FlattenedJsonLayout();
            layout.Attributes.Add(new JsonAttribute("flat1", "flat1"));
            return layout;
        }

        [Test]
        public void MessageShouldHaveCustomFieldFromConfig()
        {
            foreach (var line in Result)
            {
                line.ShouldMatch(@"""flat1"":""flat1""");
            }
        }
    }
}
