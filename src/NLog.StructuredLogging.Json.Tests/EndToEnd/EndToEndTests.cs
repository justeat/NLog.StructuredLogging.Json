using System;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Time;
using NUnit.Framework;

namespace NLog.StructuredLogging.Json.Tests.EndToEnd
{
    public abstract class EndToEndTests
    {
        protected Logger Sut;
        protected string TargetName;
        protected string Message;
        protected string LoggerName;
        protected FakeTimeSource TimeSourceForTest;

        [OneTimeSetUp]
        public void BeforeEverything()
        {
            Given();
            Sut = LogManager.GetLogger(LoggerName);
            When();
        }

        protected virtual void Given()
        {
            TargetName = Guid.NewGuid().ToString();
            LoggerName = "ExampleLogger";
            GivenLoggingIsConfiguredForTest(TargetName);
            GivenTimeIsFaked();
        }

        protected abstract Layout GivenLayout();

        private void GivenTimeIsFaked()
        {
            TimeSourceForTest = GivenFakeTime();
            TimeSource.Current = TimeSourceForTest;
        }

        private static FakeTimeSource GivenFakeTime()
        {
            return new FakeTimeSource();
        }

        protected abstract void When();

        private void GivenLoggingIsConfiguredForTest(string name)
        {
            ProgrammaticallyRegisterExtensions();
            var config = LogManager.Configuration;
            var target = GivenTarget(name);
            config.AddTarget(target);
            SetUpRules(target, config);
            ModifyLoggingConfigurationBeforeCommit(name, config);
            LogManager.Configuration = config;
        }

        protected virtual void ModifyLoggingConfigurationBeforeCommit(string nameOfTargetForSut, LoggingConfiguration config) {}

        protected virtual void ProgrammaticallyRegisterExtensions()
        {
            ConfigurationItemFactory.Default.Layouts.RegisterDefinition("flattenedjsonlayout", typeof(FlattenedJsonLayout));
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("structuredlogging.json", typeof(StructuredLoggingLayoutRenderer));
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("hasher", typeof(HasherLayoutRenderer));
        }

        protected static void SetUpRules(Target target, LoggingConfiguration config)
        {
            var rule = new LoggingRule("*", LogLevel.Trace, target);
            config.LoggingRules.Insert(0, rule);
        }

        protected virtual Target GivenTarget(string name)
        {
            return new MemoryTarget { Name = name, Layout = GivenLayout() };
        }
    }
}
