using System;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayoutRenderer
{
    public class MessageContainsJson : AsyncLayoutRendererTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork
    {
        protected override string GivenMessage()
        {
            return "json " + JsonConvert.SerializeObject(new { foo = "bar", baz = new { wibble = "chip" } });
        }

        protected override int GivenExpectedNumberBraces()
        {
            return Iterations * 3;
        }

        [Test]
        public override void ShouldHaveLoggedAMessage()
        {
            foreach (var line in Result)
            {
                Console.WriteLine(line);
                line.ShouldMatch(@"\{\\""foo\\"":\\""bar\\"",\\""baz\\"":\{\\""wibble\\"":\\""chip\\""\}\}");
            }
        }

        [Test]
        public void ShouldHaveSensibleLengthLines()
        {
            foreach (var line in Result)
            {
                Assert.That(line.Length, Is.InRange(550, 1500));
            }
        }
    }
}
