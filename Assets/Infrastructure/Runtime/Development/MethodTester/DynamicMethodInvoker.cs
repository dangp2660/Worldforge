#if UNITY_EDITOR || WORLDFORGE_DEVELOPMENT_BUILD || WORLDFORGE_DEBUG_TOOLS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;
using Worldforge.Core.Bootstrap;
using Worldforge.Crafting;
using Worldforge.Inventory;
using Worldforge.Item;

namespace Worldforge.Infrastructure.Development.MethodTester
{
    public static class DynamicMethodInvoker
    {
        public static MethodExecutionReport Execute(TestMethodDescriptor descriptor)
        {
            if (descriptor == null || descriptor.Method == null)
            {
                return new MethodExecutionReport
                {
                    IsSuccess = false,
                    ReturnFormatted = "Error: Invalid descriptor or null MethodInfo."
                };
            }

            var report = new MethodExecutionReport
            {
                ReturnType = descriptor.Method.ReturnType
            };

            var logs = new List<ExecutionLogEntry>();
            void LogCallback(string condition, string stackTrace, LogType type)
            {
                logs.Add(new ExecutionLogEntry
                {
                    Type = type,
                    Message = condition,
                    StackTrace = stackTrace,
                    TimestampUtc = DateTime.UtcNow
                });
            }

            // Parse arguments
            object[] args;
            try
            {
                args = PrepareArguments(descriptor);
            }
            catch (Exception ex)
            {
                report.IsSuccess = false;
                report.Exception = ex;
                report.ReturnFormatted = $"[Parameter Preparation Error] {ex.Message}";
                return report;
            }

            // Hook logs
            Application.logMessageReceived += LogCallback;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var instance = descriptor.TargetInstance;
                if (instance == null && !descriptor.Method.IsStatic)
                {
                    // Attempt resolution from Bootstrap
                    if (BootstrapManager.HasInstance && BootstrapManager.Instance.Services != null)
                    {
                        BootstrapManager.Instance.Services.TryResolve(descriptor.TargetType, out instance);
                    }
                }

                if (instance == null && !descriptor.Method.IsStatic)
                {
                    throw new InvalidOperationException(
                        $"Cannot invoke instance method '{descriptor.Method.Name}' on null target of type '{descriptor.TargetType?.Name}'.");
                }

                var returnValue = descriptor.Method.Invoke(instance, args);
                stopwatch.Stop();

                report.IsSuccess = true;
                report.ReturnValue = returnValue;
                report.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                report.ReturnFormatted = FormatReturnValue(returnValue, descriptor.Method.ReturnType);
            }
            catch (TargetInvocationException tie)
            {
                stopwatch.Stop();
                var inner = tie.InnerException ?? tie;
                report.IsSuccess = false;
                report.Exception = inner;
                report.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                report.ReturnFormatted = $"[Invocation Exception] {inner.GetType().Name}: {inner.Message}\n{inner.StackTrace}";

                logs.Add(new ExecutionLogEntry
                {
                    Type = LogType.Exception,
                    Message = $"{inner.GetType().Name}: {inner.Message}",
                    StackTrace = inner.StackTrace,
                    TimestampUtc = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                report.IsSuccess = false;
                report.Exception = ex;
                report.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                report.ReturnFormatted = $"[Execution Error] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";

                logs.Add(new ExecutionLogEntry
                {
                    Type = LogType.Error,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    TimestampUtc = DateTime.UtcNow
                });
            }
            finally
            {
                Application.logMessageReceived -= LogCallback;
                report.Logs = logs;
            }

            return report;
        }

        private static object[] PrepareArguments(TestMethodDescriptor descriptor)
        {
            var parameters = descriptor.Parameters;
            var args = new object[parameters.Count];

            for (var i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                args[i] = ConvertParameterValue(p);
            }

            return args;
        }

        private static object ConvertParameterValue(TestMethodParameterInfo p)
        {
            var targetType = p.ParameterType;
            var rawStr = p.CurrentStringValue?.Trim() ?? string.Empty;

            if (p.Kind == ParameterKind.ScriptableObject)
            {
                var resolvedSo = p.ResolvedValue as ScriptableObject;
                if (resolvedSo == null && p.DropdownValues != null && p.SelectedDropdownIndex >= 0 &&
                    p.SelectedDropdownIndex < p.DropdownValues.Length)
                {
                    resolvedSo = p.DropdownValues[p.SelectedDropdownIndex] as ScriptableObject;
                }

                // If the target method expects a string (e.g. recipeCode, itemCode)
                if (targetType == typeof(string))
                {
                    if (resolvedSo is RecipeDefinition recipe)
                    {
                        return recipe.RecipeCode ?? recipe.name;
                    }
                    if (resolvedSo is ItemDefinition item)
                    {
                        return item.ItemCode ?? item.name;
                    }
                    if (resolvedSo != null)
                    {
                        return resolvedSo.name;
                    }
                    return rawStr;
                }

                if (resolvedSo != null)
                {
                    return resolvedSo;
                }

                if (!string.IsNullOrEmpty(rawStr))
                {
                    return Resources.Load(rawStr, targetType);
                }

                return null;
            }

            if (p.Kind == ParameterKind.Enum)
            {
                if (p.DropdownValues != null && p.SelectedDropdownIndex >= 0 &&
                    p.SelectedDropdownIndex < p.DropdownValues.Length)
                {
                    return p.DropdownValues[p.SelectedDropdownIndex];
                }

                if (Enum.TryParse(targetType, rawStr, true, out var enumVal))
                {
                    return enumVal;
                }

                return p.DefaultValue;
            }

            if (p.Kind == ParameterKind.Int)
            {
                if (int.TryParse(rawStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intVal))
                {
                    return intVal;
                }
                return p.DefaultValue ?? 0;
            }

            if (p.Kind == ParameterKind.Float)
            {
                if (float.TryParse(rawStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatVal))
                {
                    return floatVal;
                }
                return p.DefaultValue ?? 0f;
            }

            if (p.Kind == ParameterKind.Bool)
            {
                if (bool.TryParse(rawStr, out var boolVal))
                {
                    return boolVal;
                }
                return string.Equals(rawStr, "1", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(rawStr, "true", StringComparison.OrdinalIgnoreCase);
            }

            if (p.Kind == ParameterKind.String)
            {
                return rawStr;
            }

            if (p.Kind == ParameterKind.Vector2)
            {
                var parts = rawStr.Split(',');
                if (parts.Length == 2 &&
                    float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                {
                    return new Vector2(x, y);
                }
                return Vector2.zero;
            }

            if (p.Kind == ParameterKind.Vector3)
            {
                var parts = rawStr.Split(',');
                if (parts.Length == 3 &&
                    float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
                {
                    return new Vector3(x, y, z);
                }
                return Vector3.zero;
            }

            // Handle special interfaces like IInventoryContainer
            if (typeof(IInventoryContainer).IsAssignableFrom(targetType))
            {
                if (p.ResolvedValue is IInventoryContainer resolvedContainer)
                {
                    return resolvedContainer;
                }

                if (BootstrapManager.HasInstance && BootstrapManager.Instance.Services != null &&
                    BootstrapManager.Instance.Services.TryResolve<IInventoryContainer>(out var activeContainer) &&
                    activeContainer != null)
                {
                    return activeContainer;
                }

                // If not found in services, create default fallback container
                return new InventoryContainer("DebugAutoInventory", 20, 100f);
            }

            // Fallback for objects/JSON
            if (!string.IsNullOrEmpty(rawStr))
            {
                try
                {
                    return JsonUtility.FromJson(rawStr, targetType);
                }
                catch
                {
                    // Ignore and proceed
                }
            }

            return p.DefaultValue;
        }

        private static string FormatReturnValue(object value, Type returnType)
        {
            if (returnType == typeof(void))
            {
                return "(void) - Execution completed with no return value.";
            }

            if (value == null)
            {
                return "null";
            }

            var type = value.GetType();

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                return value.ToString();
            }

            var sb = new StringBuilder();
            sb.AppendLine($"// Return Type: {type.FullName}");

            try
            {
                var json = JsonUtility.ToJson(value, true);
                if (!string.IsNullOrEmpty(json) && json != "{}")
                {
                    sb.AppendLine(json);
                    return sb.ToString();
                }
            }
            catch
            {
                // Fallback to manual reflection dump
            }

            sb.AppendLine("{");
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (prop.GetIndexParameters().Length > 0) continue;
                try
                {
                    var val = prop.GetValue(value);
                    sb.AppendLine($"  \"{prop.Name}\": {FormatPropertyValue(val)},");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  \"{prop.Name}\": \"<Error: {ex.Message}>\",");
                }
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                try
                {
                    var val = field.GetValue(value);
                    sb.AppendLine($"  \"{field.Name}\": {FormatPropertyValue(val)},");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  \"{field.Name}\": \"<Error: {ex.Message}>\",");
                }
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string FormatPropertyValue(object val)
        {
            if (val == null) return "null";
            if (val is string s) return $"\"{s}\"";
            if (val is bool b) return b ? "true" : "false";
            if (val.GetType().IsPrimitive) return val.ToString();
            if (val is System.Collections.IEnumerable list && !(val is string))
            {
                var items = new List<string>();
                foreach (var item in list)
                {
                    items.Add(item != null ? item.ToString() : "null");
                }
                return $"[{string.Join(", ", items)}]";
            }
            return $"\"{val}\"";
        }
    }
}
#endif
