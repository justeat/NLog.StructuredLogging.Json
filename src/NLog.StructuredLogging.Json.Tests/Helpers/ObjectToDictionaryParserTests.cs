using NLog.StructuredLogging.Json.Helpers;
using NUnit.Framework;
using System;

namespace NLog.StructuredLogging.Json.Tests.Helpers
{
    [TestFixture]
    public class ObjectToDictionaryParserTests
    {
        private class FlatObject
        {
            public int Int { get; set; }
            public string Str { get; set; }
            public DateTime? Time { get; set; }

            public object this[string name] => name;
        }

        private class NonFlatObject
        {
            public int Int { get; set; }
            public FlatObject FlatObject { get; set; }

            public object this[string name] => name;
        }

        [Test]
        public void Should_Return_Same_Object_From_Cache_For_Anonymous_Object()
        {
            var firstAnonymousObject = new {Int = 1, Str = "str"};

            var first = ObjectToDictionaryConverter.GetConverter(firstAnonymousObject.GetType());

            var secondAnonymousObject = new {Int = 2, Str = "str2"};

            var second = ObjectToDictionaryConverter.GetConverter(secondAnonymousObject.GetType());

            Assert.AreSame(first, second);
        }

        [Test]
        public void Should_Return_Same_Object_From_Cache_For_Plain_Object()
        {
            var firstAnonymousObject = new FlatObject {Int = 1, Str = "str", Time = DateTime.UtcNow};

            var first = ObjectToDictionaryConverter.GetConverter(firstAnonymousObject.GetType());

            var secondAnonymousObject = new FlatObject {Int = 2, Str = "str2", Time = DateTime.Now};

            var second = ObjectToDictionaryConverter.GetConverter(secondAnonymousObject.GetType());

            Assert.AreSame(first, second);
        }

        [Test]
        public void Should_Return_Valid_Dictionary_Parser_For_Anonymous_Object()
        {
            var anonymousObject = new {Int = 1, Str = "str"};

            var converter = ObjectToDictionaryConverter.GetConverter(anonymousObject.GetType());

            var dictionary = converter.ConvertFromObject(anonymousObject);

            Assert.AreEqual(anonymousObject.Int, (int) dictionary[nameof(anonymousObject.Int)]);
            Assert.AreEqual(anonymousObject.Str, (string) dictionary[nameof(anonymousObject.Str)]);
        }

        [Test]
        public void Should_Return_Valid_Dictionary_Parser_For_Plain_Object()
        {
            var plainObject = new FlatObject {Int = 1, Str = "str", Time = DateTime.UtcNow};

            var parser = ObjectToDictionaryConverter.GetConverter(plainObject.GetType());

            var dictionary = parser.ConvertFromObject(plainObject);

            Assert.AreEqual(plainObject.Int, (int) dictionary[nameof(plainObject.Int)]);
            Assert.AreEqual(plainObject.Str, (string) dictionary[nameof(plainObject.Str)]);
            Assert.AreEqual(plainObject.Time, (DateTime) dictionary[nameof(plainObject.Time)]);
        }

        [Test]
        public void Should_Return_Valid_Dictionary_Parser_For_NonFlat_Object()
        {
            var nonFlatObject = new NonFlatObject
            {
                Int = 2,
                FlatObject = new FlatObject {Int = 1, Str = "str", Time = DateTime.UtcNow}
            };

            var parser = ObjectToDictionaryConverter.GetConverter(nonFlatObject.GetType());

            var dictionary = parser.ConvertFromObject(nonFlatObject);

            Assert.AreEqual(nonFlatObject.Int, (int) dictionary[nameof(nonFlatObject.Int)]);
            Assert.AreSame(nonFlatObject.FlatObject, (FlatObject) dictionary[nameof(nonFlatObject.FlatObject)]);
        }
    }
}
