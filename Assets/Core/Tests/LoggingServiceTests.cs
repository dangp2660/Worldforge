using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Core.Tests
{
    public class LoggingServiceTests
    {
        private GameObject managerObject;

        [TearDown]
        public void TearDown()
        {
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
                managerObject = null;
            }
        }

        [Test]
        public void Initialize_RegistersLoggingServices()
        {
            var flow = new ApplicationStartupFlow();
            var context = CreateContext();

            flow.Initialize(context);

            Assert.NotNull(context.Services.Resolve<ILogService>());
            Assert.NotNull(context.Services.Resolve<ILogConfiguration>());

            flow.Shutdown("TestCleanup");
        }

        [Test]
        public void LoggingService_UsesConfigurableOutputAndLevelFiltering()
        {
            var flow = new ApplicationStartupFlow();
            var context = CreateContext();

            flow.Initialize(context);

            var logger = context.Services.Resolve<ILogService>();
            var configuration = context.Services.Resolve<ILogConfiguration>();
            var output = new RecordingLogOutput();

            configuration.MinimumLevel = LogLevel.Warning;
            configuration.SetOutputs(new ILogOutput[] { output });

            logger.Info("Test.Logging", "This info log should be filtered.");
            logger.Warning("Test.Logging", "Warning log is enabled.");
            logger.Error("Test.Logging", "Error log is enabled.");

            Assert.AreEqual(2, output.Entries.Count);
            Assert.AreEqual(LogLevel.Warning, output.Entries[0].Level);
            Assert.AreEqual("Warning log is enabled.", output.Entries[0].Message);
            Assert.AreEqual(LogLevel.Error, output.Entries[1].Level);
            Assert.AreEqual("Error log is enabled.", output.Entries[1].Message);
            Assert.AreEqual("Test.Logging", output.Entries[1].Category);

            flow.Shutdown("TestCleanup");
        }

        private ApplicationBootstrapContext CreateContext()
        {
            managerObject = new GameObject("LoggingServiceTests");
            var manager = managerObject.AddComponent<BootstrapManager>();
            return new ApplicationBootstrapContext(manager);
        }

        private sealed class RecordingLogOutput : ILogOutput
        {
            public List<LogEntry> Entries { get; } = new List<LogEntry>();

            public void Write(LogEntry entry)
            {
                Entries.Add(
                    new LogEntry
                    {
                        TimestampUtc = entry.TimestampUtc,
                        Level = entry.Level,
                        Category = entry.Category,
                        Message = entry.Message,
                        Exception = entry.Exception
                    });
            }
        }
    }
}
