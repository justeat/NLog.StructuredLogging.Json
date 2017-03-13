using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd
{
    public abstract class AggregateExceptionEndToEndTests : EndToEndTests
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
            var inner1 = new ApplicationException("Inner Exception 1");
            var inner2 = new ApplicationException("Inner Exception 2");
            var inner3 = new ApplicationException("Inner Exception 3");

            PutStackTraceOnException(inner1);
            PutStackTraceOnException(inner2);
            PutStackTraceOnException(inner3);

            var innerAggregate = new AggregateException("Inner Aggregate exception", inner2, inner3);

            var testEx = new AggregateException("Aggregate Exception", inner1, innerAggregate);

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
            Assert.That(Result.Count, Is.EqualTo(Iterations * 4));
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

                if (exMessage == "Aggregate Exception")
                {
                    ShouldHaveLoggedAggregateExceptionCorrectly(obj);
                }
                else if (exMessage == "Inner Exception 1")
                {
                    ShouldHaveLoggedInner1ExceptionCorrectly(obj);
                }
                else if (exMessage == "Inner Exception 2")
                {
                    ShouldHaveLoggedInner2ExceptionCorrectly(obj);
                }
                else if (exMessage == "Inner Exception 3")
                {
                    ShouldHaveLoggedInner3ExceptionCorrectly(obj);
                }
                else
                {
                    Assert.Fail("Unexpected exception message: " + exMessage);
                }
            }
        }

        private void ShouldHaveLoggedAggregateExceptionCorrectly(JObject obj)
        {
            obj.GetValue("Exception").ToString().ShouldMatch(@"System\.AggregateException: Aggregate Exception");
            obj.GetValue("ExceptionType").ToString().ShouldMatch("AggregateException");
            obj.GetValue("ExceptionMessage").ToString().ShouldMatch("Aggregate Exception");
            obj.GetValue("ExceptionStackTrace").ToString().ShouldMatch("   at NLog.StructuredLogging.Json.Tests.EndToEnd.AggregateExceptionEndToEndTests.PutStackTraceOnException");
        }

        private void ShouldHaveLoggedInner1ExceptionCorrectly(JObject obj)
        {
            obj.GetValue("Exception").ToString().ShouldMatch(@"System\.ApplicationException: Inner Exception 1");
            obj.GetValue("ExceptionType").ToString().ShouldMatch("ApplicationException");
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

        private void ShouldHaveLoggedInner3ExceptionCorrectly(JObject obj)
        {
            obj.GetValue("Exception").ToString().ShouldMatch(@"System\.ApplicationException: Inner Exception 3");
            obj.GetValue("ExceptionType").ToString().ShouldMatch("ApplicationException");
            obj.GetValue("ExceptionMessage").ToString().ShouldMatch("Inner Exception 3");
            ShouldHaveExpectedStacktrace(obj);
        }

        private void ShouldHaveExpectedStacktrace(JObject obj)
        {
            obj.GetValue("ExceptionStackTrace").ToString().ShouldMatch("   at NLog.StructuredLogging.Json.Tests.EndToEnd.AggregateExceptionEndToEndTests.PutStackTraceOnException");
        }

        [Test]
        public void ShouldHaveLoggedCallSite()
        {
            foreach (var line in Result)
            {
                line.ShouldMatch(@"CallSite"":""NLog\.StructuredLogging\.Json\.Tests\.EndToEnd\.AggregateExceptionEndToEndTests\.When");
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
