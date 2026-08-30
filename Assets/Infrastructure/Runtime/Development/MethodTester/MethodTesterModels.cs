#if UNITY_EDITOR || WORLDFORGE_DEVELOPMENT_BUILD || WORLDFORGE_DEBUG_TOOLS
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Worldforge.Infrastructure.Development.MethodTester
{
    public enum ParameterKind
    {
        String,
        Int,
        Float,
        Bool,
        Enum,
        Vector2,
        Vector3,
        ScriptableObject,
        ObjectOrJson
    }

    public sealed class TestMethodParameterInfo
    {
        public string Name { get; set; }

        public Type ParameterType { get; set; }

        public ParameterKind Kind { get; set; }

        public bool IsOptional { get; set; }

        public object DefaultValue { get; set; }

        public string CurrentStringValue { get; set; }

        public int SelectedDropdownIndex { get; set; }

        public string[] DropdownOptions { get; set; }

        public object[] DropdownValues { get; set; }

        public object ResolvedValue { get; set; }

        public Type AssociatedScriptableObjectType { get; set; }
    }

    public sealed class TestMethodDescriptor
    {
        public MethodInfo Method { get; set; }

        public object TargetInstance { get; set; }

        public Type TargetType { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public bool IsPrimary { get; set; }

        public int Order { get; set; }

        public List<TestMethodParameterInfo> Parameters { get; set; } = new List<TestMethodParameterInfo>();

        public string SignatureText { get; set; }
    }

    public sealed class TestServiceDescriptor
    {
        public Type ServiceType { get; set; }

        public object Instance { get; set; }

        public string Category { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public int Order { get; set; }

        public List<TestMethodDescriptor> PrimaryMethods { get; set; } = new List<TestMethodDescriptor>();

        public List<TestMethodDescriptor> AllMethods { get; set; } = new List<TestMethodDescriptor>();

        public bool IsExpandedInUI { get; set; } = true;
    }

    public sealed class ExecutionLogEntry
    {
        public LogType Type { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class MethodExecutionReport
    {
        public bool IsSuccess { get; set; }

        public object ReturnValue { get; set; }

        public Type ReturnType { get; set; }

        public string ReturnFormatted { get; set; }

        public double ExecutionTimeMs { get; set; }

        public List<ExecutionLogEntry> Logs { get; set; } = new List<ExecutionLogEntry>();

        public Exception Exception { get; set; }

        public DateTime ExecutedAt { get; set; } = DateTime.Now;
    }

    [Serializable]
    public sealed class MethodTestPreset
    {
        public string PresetName;

        public string ServiceTypeName;

        public string MethodName;

        public List<string> ParameterNames = new List<string>();

        public List<string> ParameterValues = new List<string>();
    }
}
#endif
