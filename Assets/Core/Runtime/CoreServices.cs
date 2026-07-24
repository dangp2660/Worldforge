using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Worldforge.Core.Services
{
    public enum LogLevel
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    public sealed class LogEntry
    {
        public DateTime TimestampUtc { get; set; }

        public LogLevel Level { get; set; }

        public string Category { get; set; }

        public string Message { get; set; }

        public Exception Exception { get; set; }
    }

    public interface ILogOutput
    {
        void Write(LogEntry entry);
    }

    public interface ILogConfiguration
    {
        LogLevel MinimumLevel { get; set; }

        IReadOnlyList<ILogOutput> Outputs { get; }

        void SetOutputs(IEnumerable<ILogOutput> outputs);

        void AddOutput(ILogOutput output);

        void ClearOutputs();
    }

    public interface ILogService
    {
        ILogConfiguration Configuration { get; }

        bool IsEnabled(LogLevel level);

        void Log(LogLevel level, string category, string message, Exception exception = null);

        void Info(string category, string message);

        void Warning(string category, string message);

        void Error(string category, string message, Exception exception = null);
    }

    public interface IApplicationInfoService
    {
        IReadOnlyList<string> LoadedSystems { get; }

        IReadOnlyList<string> LoadedGameplayModules { get; }

        string StartupScenePath { get; }

        string ActiveScenePath { get; }
    }

    public interface IClockService
    {
        float TimeSinceStartup { get; }

        float UnscaledTimeSinceStartup { get; }
    }

    public interface IInputActionsService
    {
        InputActionAsset Actions { get; }
    }

    internal sealed class RuntimeLogConfiguration : ILogConfiguration
    {
        private readonly List<ILogOutput> outputs = new();

        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

        public IReadOnlyList<ILogOutput> Outputs
        {
            get { return outputs; }
        }

        public static RuntimeLogConfiguration CreateDefault()
        {
            var configuration = new RuntimeLogConfiguration();
            configuration.AddOutput(new UnityDebugLogOutput());
            return configuration;
        }

        public void SetOutputs(IEnumerable<ILogOutput> configuredOutputs)
        {
            outputs.Clear();

            if (configuredOutputs == null)
            {
                return;
            }

            foreach (var output in configuredOutputs)
            {
                if (output != null)
                {
                    outputs.Add(output);
                }
            }
        }

        public void AddOutput(ILogOutput output)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            outputs.Add(output);
        }

        public void ClearOutputs()
        {
            outputs.Clear();
        }
    }

    internal sealed class RuntimeLogService : ILogService
    {
        private readonly ILogConfiguration configuration;

        public RuntimeLogService(ILogConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public ILogConfiguration Configuration
        {
            get { return configuration; }
        }

        public bool IsEnabled(LogLevel level)
        {
            return level >= configuration.MinimumLevel;
        }

        public void Log(LogLevel level, string category, string message, Exception exception = null)
        {
            if (!IsEnabled(level))
            {
                return;
            }

            var outputs = configuration.Outputs;
            if (outputs == null || outputs.Count == 0)
            {
                return;
            }

            var entry = new LogEntry
            {
                TimestampUtc = DateTime.UtcNow,
                Level = level,
                Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
                Message = message ?? string.Empty,
                Exception = exception
            };

            for (var i = 0; i < outputs.Count; i++)
            {
                outputs[i]?.Write(entry);
            }
        }

        public void Info(string category, string message)
        {
            Log(LogLevel.Info, category, message);
        }

        public void Warning(string category, string message)
        {
            Log(LogLevel.Warning, category, message);
        }

        public void Error(string category, string message, Exception exception = null)
        {
            Log(LogLevel.Error, category, message, exception);
        }
    }

    internal sealed class UnityDebugLogOutput : ILogOutput
    {
        public void Write(LogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var formattedMessage = LogMessageFormatter.Format(entry);

            switch (entry.Level)
            {
                case LogLevel.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    Debug.LogError(formattedMessage);
                    break;
                default:
                    Debug.Log(formattedMessage);
                    break;
            }

            if (entry.Exception != null)
            {
                Debug.LogException(entry.Exception);
            }
        }
    }

    internal static class LogMessageFormatter
    {
        public static string Format(LogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var baseMessage = string.Format(
                CultureInfo.InvariantCulture,
                "[Worldforge] [{0}] [{1}] {2}",
                entry.Level,
                entry.Category,
                entry.Message ?? string.Empty);

            if (entry.Exception == null)
            {
                return baseMessage;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} Exception: {1}: {2}",
                baseMessage,
                entry.Exception.GetType().Name,
                entry.Exception.Message);
        }
    }
}

namespace Worldforge.Core.Bootstrap
{
    using Worldforge.Core.Services;

    internal sealed class CoreServiceRegistrationProvider : IServiceRegistrationProvider
    {
        public int Order
        {
            get { return 0; }
        }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            services.AddSingleton(context.Manager);
            services.AddSingleton(context);
            services.AddSingleton<ILogConfiguration>(_ => RuntimeLogConfiguration.CreateDefault());
            services.AddSingleton<ILogService>(resolver => new RuntimeLogService(resolver.Resolve<ILogConfiguration>()));
            services.AddSingleton<IApplicationInfoService>(_ => new ApplicationInfoService(context));
            services.AddSingleton<IClockService>(_ => new UnityClockService());
            services.AddSingleton<IInputActionsService>(_ => new InputActionsService(context));
        }
    }

    internal sealed class ApplicationInfoService : IApplicationInfoService
    {
        private readonly ApplicationBootstrapContext context;

        public ApplicationInfoService(ApplicationBootstrapContext context)
        {
            this.context = context;
        }

        public IReadOnlyList<string> LoadedSystems
        {
            get { return context.LoadedSystems; }
        }

        public IReadOnlyList<string> LoadedGameplayModules
        {
            get { return context.LoadedGameplayModules; }
        }

        public string StartupScenePath
        {
            get { return context.StartupScenePath; }
        }

        public string ActiveScenePath
        {
            get { return context.ActiveScenePath; }
        }
    }

    internal sealed class UnityClockService : IClockService
    {
        public float TimeSinceStartup
        {
            get { return Time.time; }
        }

        public float UnscaledTimeSinceStartup
        {
            get { return Time.unscaledTime; }
        }
    }

    internal sealed class InputActionsService : IInputActionsService
    {
        private readonly ApplicationBootstrapContext context;

        public InputActionsService(ApplicationBootstrapContext context)
        {
            this.context = context;
        }

        public InputActionAsset Actions
        {
            get { return context.ProjectWideInputActions; }
        }
    }
}
