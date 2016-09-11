using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;
using System.Linq;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd
{
    public abstract class NestedExceptionEndToEndTests : EndToEndTests
    {
        private Exception _exception;
        protected IList<string> Result;
        protected int Iterations;

        protected override void Given()
        {
            Iterations = 10;
            _exception = GivenException();
            Message = "Test message";
            base.Given();
        }

        private static Exception GivenException()
        {
            var inner2 = new ApplicationException("Inner Exception 2");
            var inner1 = new ArgumentException("Inner Exception 1", inner2);
            var testEx = new InvalidOperationException("Outer Exception", inner1);

            PutStackTraceOnException(inner1);
            PutStackTraceOnException(inner2);

            testEx.Data.Add("ex_key_1", "ex_data_1");
            testEx.Data.Add("ex_key_2", "ex_data_2");

            return PutStackTraceOnException(testEx);
        }

        private static Exception PutStackTraceOnException(Exception inputEx)
        {
            try
            {
                throw inputEx;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        protected override void When()
        {
            for (var i = 0; i < Iterations; i++)
            {
                Sut.ExtendedException(_exception, Message, new { PropertyOne = "one", PropertyTwo = 2, Iteration = i });
            }
            LogManager.Flush();
            Result = LogManager.Configuration.LogMessage(TargetName);
        }

        [Test]
        public virtual void ShouldHaveExpectedNumberOfLines()
        {
            Result.Count.ShouldBe(Iterations * 3);
        }

        [Test]
        public void ShouldHaveLoggedAtErrorLevel()
        {
            foreach (var line in Result)
            {
                line.ShouldMatch("Error");
            }
        }

        [Test]
        public void ShouldHaveLoggedExceptionCorrectly()
        {
            foreach (var line in Result)
            {
                var obj = JObject.Parse(line);
                var exMessage = obj.GetValue("ExceptionMessage").ToString();

                if (exMessage == "Outer Exception")
                {
                    ShouldHaveLoggedOuterExceptionCorrectly(obj);
                }
                else if (exMessage == "Inner Exception 1")
                {
                    ShouldHaveLoggedInner1ExceptionCorrectly(obj);
                }
                else if (exMessage == "Inner Exception 2")
                {
                    ShouldHaveLoggedInner2ExceptionCorrectly(obj);
                }
                else
                {
                    Assert.Fail("Unexpected exception message: " + exMessage);
                }
            }
        }

        private void ShouldHaveLoggedOuterExceptionCorrectly(JObject obj)
        {
            obj.GetValue("Exception").ToString().ShouldMatch(@"System\.InvalidOperationException: Outer Exception");
            obj.GetValue("ExceptionType").ToString().ShouldMatch("InvalidOperationException");
            obj.GetValue("ExceptionMessage").ToString().ShouldMatch("Outer Exception");
            ShouldHaveExpectedStacktrace(obj);
        }

        private void ShouldHaveLoggedInner1ExceptionCorrectly(JObject obj)
        {
            obj.GetValue("Exception").ToString().ShouldMatch(@"System\.ArgumentException: Inner Exception 1");
            obj.GetValue("ExceptionType").ToString().ShouldMatch("ArgumentException");
            obj.GetValue("ExceptionMessage").ToString().ShouldMatch("Inner Exception 1");
            ShouldHaveExpectedStacktrace(obj);
        }

        private void ShouldHaveLoggedInner2ExceptionCorrectly(JObject obj)
        {
            obj.GetValue("Exception").ToString().ShouldMatch(@"System\.ApplicationException: Inner Exception 2");
            obj.GetValue("ExceptionType").ToString().ShouldMatch("ApplicationException");
            obj.GetValue("ExceptionMessage").ToString().ShouldMatch("Inner Exception 2");
            ShouldHaveExpectedStacktrace(obj);
        }

        private void ShouldHaveExpectedStacktrace(JObject obj)
        {
            obj.GetValue("ExceptionStackTrace").ToString().ShouldMatch("   at NLog.StructuredLogging.Json.Tests.EndToEnd.NestedExceptionEndToEndTests.PutStackTraceOnException");
        }
        private static void StringShouldStartWithOneOf(string value, params string[] targets)
        {
            var pass = targets.Any(t => value.StartsWith(t));

            pass.ShouldBeTrue("Got " + value + ", expected one of" + string.Join(",", targets));
        }

        [Test]
        public void ShouldHaveLoggedCallSite()
        {
            foreach (var line in Result)
            {
                line.ShouldMatch(@"CallSite"":""NLog\.StructuredLogging\.Json\.Tests\.EndToEnd\.NestedExceptionEndToEndTests\.When");
            }
        }

        [Test]
        public void ShouldHaveLoggedExceptionDataBagCorrectly()
        {
            foreach (var line in Result)
            {
                if (line.Contains("InvalidOperationException"))
                {
                    line.ShouldMatch(@"""ex_key_1"":""ex_data_1");
                    line.ShouldMatch(@"""ex_key_2"":""ex_data_2");
                }
            }
        }
    }
}
