using Newtonsoft.Json;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd.ViaLayout
{
    public class MessageContainsJson : AsyncFlattenedJsonTestsThatTestSeveralFeaturesAtOnceToProveCombinationsWork
    {
        protected override string GivenMessage()
        {
            return string.Format("json start {0} json end", JsonConvert.SerializeObject(new {foo = "bar", baz = new {wibble = "chip"}}));
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
                line.ShouldMatch(@"\{\\""foo\\"":\\""bar\\"",\\""baz\\"":\{\\""wibble\\"":\\""chip\\""\}\}");
            }
        }

        [Test]
        public void ShouldBeSensibleNumberOfCharacters()
        {
            var all = string.Join("\n", Result);

            Assert.That(all.Length,
                Is.InRange(1350 * Iterations, 1550 * Iterations));
        }

        [Test]
        public void ShouldHaveSensibleLengthLines()
        {
            foreach (var line in Result)
            {
                Assert.That(line.Length, Is.InRange(1350, 1550),
                    "zzzzline start\n\n" +line + "\n\nzzzzzline end");
            }
        }
    }
}
