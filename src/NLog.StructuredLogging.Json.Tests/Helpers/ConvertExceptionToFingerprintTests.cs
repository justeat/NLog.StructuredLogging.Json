using System;
using NLog.StructuredLogging.Json.Helpers;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.Helpers
{
    [TestFixture]
    public class ConvertExceptionToFingerprintTests
    {
        [Test]
        public void NullExceptionFingerprintIsEmpty()
        {
            Exception ex = null;

            var fingerprint = ConvertException.ToFingerprint(ex);

            Assert.That(fingerprint, Is.Empty);
        }

        [Test]
        public void ExceptionFingerprintIsPresent()
        {
            Exception ex = new ApplicationException("test 1");

            var fingerprint = ConvertException.ToFingerprint(ex);

            AssertIsHash(fingerprint);
        }

        [Test]
        public void IdenticalMessageExceptionsHaveSameFingerprint()
        {
            Exception ex1 = new ApplicationException("test 1");
            Exception ex2 = new ApplicationException("test 1");

            var fingerprint1 = ConvertException.ToFingerprint(ex1);
            var fingerprint2 = ConvertException.ToFingerprint(ex2);

            AssertIsHash(fingerprint1);
            AssertIsHash(fingerprint2);
            Assert.That(fingerprint1, Is.EqualTo(fingerprint2));
        }

        [Test]
        public void DifferentMessageExceptionsHaveDifferentFingerprint()
        {
            Exception ex1 = new ApplicationException("test 1");
            Exception ex2 = new ApplicationException("test 2");

            var fingerprint1 = ConvertException.ToFingerprint(ex1);
            var fingerprint2 = ConvertException.ToFingerprint(ex2);

            AssertIsHash(fingerprint1);
            AssertIsHash(fingerprint2);
            Assert.That(fingerprint1, Is.Not.EqualTo(fingerprint2));
        }

        [Test]
        public void StackTraceIsUsedWhenPresent()
        {
            Exception ex1 = new ApplicationException("test 1");
            Exception ex2 = PutStackTraceOnException(new ApplicationException("test 1"));

            var fingerprint1 = ConvertException.ToFingerprint(ex1);
            var fingerprint2 = ConvertException.ToFingerprint(ex2);

            AssertIsHash(fingerprint1);
            AssertIsHash(fingerprint2);
            Assert.That(fingerprint1, Is.Not.EqualTo(fingerprint2));
        }

        [Test]
        public void IdenticalStacktraceExceptionsHaveSameFingerprint()
        {
            Exception ex1 = PutStackTraceOnException(new ApplicationException("test 1"));
            Exception ex2 = PutStackTraceOnException(new ApplicationException("test 1"));

            var fingerprint1 = ConvertException.ToFingerprint(ex1);
            var fingerprint2 = ConvertException.ToFingerprint(ex2);

            AssertIsHash(fingerprint1);
            AssertIsHash(fingerprint2);
            Assert.That(fingerprint1, Is.EqualTo(fingerprint2));
        }

        [Test]
        public void IdenticalExceptionsFromDifferentLinesInAMethodHaveADifferentFingerprint()
        {
            Exception ex1 = PutStackTraceOnExceptionGeneric(new ApplicationException("test 1"), false, "Do stuff");
            Exception ex2 = PutStackTraceOnExceptionGeneric(new ApplicationException("test 1"), true, "Do stuff");

            var fingerprint1 = ConvertException.ToFingerprint(ex1);
            var fingerprint2 = ConvertException.ToFingerprint(ex2);

            AssertIsHash(fingerprint1);
            AssertIsHash(fingerprint2);
            Assert.That(fingerprint1, Is.Not.EqualTo(fingerprint2));
        }

        private void AssertIsHash(string hash)
        {
            Assert.That(hash, Is.Not.Null);
            Assert.That(hash, Is.Not.Empty);
            Assert.That(hash.Length, Is.EqualTo(40));
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

        private static Exception PutStackTraceOnExceptionGeneric<TObject>(Exception inputEx, bool secondLine, TObject message)
        {
            try
            {
                if (!secondLine)
                {
                    throw inputEx;
                }

                //Console.WriteLine(message);

                throw inputEx;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}
