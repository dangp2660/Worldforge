#if UNITY_EDITOR || WORLDFORGE_DEVELOPMENT_BUILD || WORLDFORGE_DEBUG_TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Worldforge.Core.Attributes;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;
using Worldforge.Crafting;
using Worldforge.Item;

namespace Worldforge.Infrastructure.Development.MethodTester
{
    public static class DynamicMethodScanner
    {
        private static readonly HashSet<string> IgnoredMethodNames = new HashSet<string>
        {
            "Equals", "GetHashCode", "GetType", "ToString", "MemberwiseClone", "ReferenceEquals"
        };

        public static List<TestServiceDescriptor> ScanAllServices()
        {
            var servicesList = new List<TestServiceDescriptor>();
            var registeredTypes = new HashSet<Type>();

            // 1. Scan BootstrapManager services
            ScanFromBootstrap(servicesList, registeredTypes);

            // 2. Scan active MonoBehaviours with [TestTarget] or relevant components
            ScanFromSceneObjects(servicesList, registeredTypes);

            // 3. Sort services by Category and Order
            servicesList.Sort((a, b) =>
            {
                var catCompare = string.Compare(a.Category, b.Category, StringComparison.OrdinalIgnoreCase);
                if (catCompare != 0)
                {
                    return catCompare;
                }
                return a.Order.CompareTo(b.Order);
            });

            return servicesList;
        }

        private static void ScanFromBootstrap(List<TestServiceDescriptor> list, HashSet<Type> registeredTypes)
        {
            if (!BootstrapManager.HasInstance || BootstrapManager.Instance.Services == null)
            {
                return;
            }

            var resolver = BootstrapManager.Instance.Services;

            // Known core/domain service contracts
            var knownServiceTypes = new[]
            {
                typeof(Worldforge.Crafting.ICraftingService),
                typeof(Worldforge.Inventory.IInventoryContainer),
                typeof(Worldforge.Core.Services.IApplicationInfoService),
                typeof(Worldforge.Core.Services.ILogService),
                typeof(Worldforge.Core.Services.IClockService)
            };

            foreach (var serviceType in knownServiceTypes)
            {
                if (resolver.TryResolve(serviceType, out var instance) && instance != null)
                {
                    var descriptor = CreateServiceDescriptor(serviceType, instance);
                    if (descriptor != null && descriptor.AllMethods.Count > 0)
                    {
                        list.Add(descriptor);
                        registeredTypes.Add(serviceType);
                    }
                }
            }

            // Also scan all types in current assemblies with [TestTarget]
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    var assemblyName = assembly.GetName().Name;
                    if (!assemblyName.StartsWith("Worldforge", StringComparison.OrdinalIgnoreCase) &&
                        !assemblyName.StartsWith("Assembly-CSharp", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    foreach (var type in assembly.GetTypes())
                    {
                        if (registeredTypes.Contains(type))
                        {
                            continue;
                        }

                        var targetAttr = type.GetCustomAttribute<TestTargetAttribute>(true);
                        if (targetAttr != null && resolver.TryResolve(type, out var serviceInstance) && serviceInstance != null)
                        {
                            var descriptor = CreateServiceDescriptor(type, serviceInstance, targetAttr);
                            if (descriptor != null && descriptor.AllMethods.Count > 0)
                            {
                                list.Add(descriptor);
                                registeredTypes.Add(type);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MethodTester] Assembly scan error: {ex.Message}");
            }
        }

        private static void ScanFromSceneObjects(List<TestServiceDescriptor> list, HashSet<Type> registeredTypes)
        {
            var monoBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);
            foreach (var mb in monoBehaviours)
            {
                if (mb == null)
                {
                    continue;
                }

                var type = mb.GetType();
                var targetAttr = type.GetCustomAttribute<TestTargetAttribute>(true);
                if (targetAttr != null && !registeredTypes.Contains(type))
                {
                    var descriptor = CreateServiceDescriptor(type, mb, targetAttr);
                    if (descriptor != null && descriptor.AllMethods.Count > 0)
                    {
                        list.Add(descriptor);
                        registeredTypes.Add(type);
                    }
                }
            }
        }

        public static TestServiceDescriptor CreateServiceDescriptor(Type serviceType, object instance, TestTargetAttribute targetAttr = null)
        {
            if (serviceType == null || instance == null)
            {
                return null;
            }

            targetAttr ??= serviceType.GetCustomAttribute<TestTargetAttribute>(true);

            var category = targetAttr?.Category ?? InferCategory(serviceType);
            var displayName = targetAttr?.DisplayName ?? FormatTypeName(serviceType);
            var description = targetAttr?.Description ?? string.Empty;
            var order = targetAttr?.Order ?? 50;

            var descriptor = new TestServiceDescriptor
            {
                ServiceType = serviceType,
                Instance = instance,
                Category = category,
                DisplayName = displayName,
                Description = description,
                Order = order
            };

            // Scan public methods
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            var methods = serviceType.GetMethods(bindingFlags);

            foreach (var method in methods)
            {
                if (ShouldIgnoreMethod(method))
                {
                    continue;
                }

                // Only include methods explicitly designated as primary test methods
                var methodAttr = method.GetCustomAttribute<TestMethodAttribute>(true);
                if (methodAttr == null || !methodAttr.IsPrimary)
                {
                    continue;
                }

                var methodDesc = CreateMethodDescriptor(method, instance, serviceType, category, methodAttr);
                if (methodDesc != null)
                {
                    descriptor.AllMethods.Add(methodDesc);
                    descriptor.PrimaryMethods.Add(methodDesc);
                }
            }

            // Sort methods by Order then DisplayName
            descriptor.AllMethods.Sort((a, b) =>
            {
                var orderComp = a.Order.CompareTo(b.Order);
                if (orderComp != 0)
                {
                    return orderComp;
                }
                return string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase);
            });

            descriptor.PrimaryMethods = descriptor.AllMethods;

            return descriptor;
        }

        private static bool ShouldIgnoreMethod(MethodInfo method)
        {
            if (method == null || method.IsSpecialName)
            {
                return true; // Exclude property getters/setters, event add/remove
            }

            if (IgnoredMethodNames.Contains(method.Name))
            {
                return true;
            }

            if (method.GetCustomAttribute<TestMethodIgnoreAttribute>(true) != null)
            {
                return true;
            }

            // Exclude compiler-generated or generic definition methods
            if (method.IsGenericMethodDefinition)
            {
                return true;
            }

            return false;
        }

        private static TestMethodDescriptor CreateMethodDescriptor(
            MethodInfo method,
            object instance,
            Type targetType,
            string defaultCategory,
            TestMethodAttribute methodAttr)
        {
            var isPrimary = methodAttr != null && methodAttr.IsPrimary;
            var displayName = methodAttr?.DisplayName ?? method.Name;
            var description = methodAttr?.Description ?? string.Empty;
            var order = methodAttr?.Order ?? 100;
            var category = methodAttr?.Category ?? defaultCategory;

            var descriptor = new TestMethodDescriptor
            {
                Method = method,
                TargetInstance = instance,
                TargetType = targetType,
                DisplayName = displayName,
                Description = description,
                Category = category,
                IsPrimary = isPrimary,
                Order = order
            };

            // Build parameter info list
            var parameters = method.GetParameters();
            foreach (var p in parameters)
            {
                var paramInfo = CreateParameterInfo(p);
                descriptor.Parameters.Add(paramInfo);
            }

            descriptor.SignatureText = BuildSignature(method, descriptor.Parameters);
            return descriptor;
        }

        private static TestMethodParameterInfo CreateParameterInfo(ParameterInfo p)
        {
            var paramType = p.ParameterType;
            var kind = ParameterKind.String;
            var defaultValue = p.HasDefaultValue ? p.DefaultValue : GetDefaultTypeValue(paramType);
            var currentStr = defaultValue != null ? defaultValue.ToString() : string.Empty;

            string[] dropdownOptions = null;
            object[] dropdownValues = null;

            if (paramType == typeof(int) || paramType == typeof(long) || paramType == typeof(short))
            {
                kind = ParameterKind.Int;
                currentStr = defaultValue != null ? defaultValue.ToString() : "0";
            }
            else if (paramType == typeof(float) || paramType == typeof(double))
            {
                kind = ParameterKind.Float;
                currentStr = defaultValue != null ? defaultValue.ToString() : "0";
            }
            else if (paramType == typeof(bool))
            {
                kind = ParameterKind.Bool;
                currentStr = defaultValue != null ? defaultValue.ToString().ToLower() : "false";
            }
            else if (paramType.IsEnum)
            {
                kind = ParameterKind.Enum;
                var enumNames = Enum.GetNames(paramType);
                var enumVals = Enum.GetValues(paramType).Cast<object>().ToArray();
                dropdownOptions = enumNames;
                dropdownValues = enumVals;
            }
            else if (paramType == typeof(Vector2))
            {
                kind = ParameterKind.Vector2;
                currentStr = "0, 0";
            }
            else if (paramType == typeof(Vector3))
            {
                kind = ParameterKind.Vector3;
                currentStr = "0, 0, 0";
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(paramType))
            {
                kind = ParameterKind.ScriptableObject;
                var loadedAssets = Resources.LoadAll("", paramType);
                if (loadedAssets != null && loadedAssets.Length > 0)
                {
                    dropdownOptions = loadedAssets.Select(a => a.name).ToArray();
                    dropdownValues = loadedAssets.Cast<object>().ToArray();
                }
                else
                {
                    dropdownOptions = new[] { "None (No assets in Resources)" };
                    dropdownValues = new object[] { null };
                }

                return new TestMethodParameterInfo
                {
                    Name = p.Name,
                    ParameterType = paramType,
                    Kind = kind,
                    AssociatedScriptableObjectType = paramType,
                    IsOptional = p.IsOptional,
                    DefaultValue = defaultValue,
                    CurrentStringValue = currentStr,
                    DropdownOptions = dropdownOptions,
                    DropdownValues = dropdownValues,
                    SelectedDropdownIndex = 0,
                    ResolvedValue = dropdownValues != null && dropdownValues.Length > 0 ? dropdownValues[0] : null
                };
            }
            else if (paramType == typeof(string))
            {
                Type associatedSoType = null;
                var lowerName = p.Name.ToLowerInvariant();
                if (lowerName.Contains("recipe"))
                {
                    associatedSoType = typeof(RecipeDefinition);
                }
                else if (lowerName.Contains("item"))
                {
                    associatedSoType = typeof(ItemDefinition);
                }

                if (associatedSoType != null)
                {
                    kind = ParameterKind.ScriptableObject;
                    var loadedAssets = Resources.LoadAll("", associatedSoType);
                    if (loadedAssets != null && loadedAssets.Length > 0)
                    {
                        dropdownOptions = loadedAssets.Select(a => a.name).ToArray();
                        dropdownValues = loadedAssets.Cast<object>().ToArray();
                    }
                    else
                    {
                        dropdownOptions = new[] { "None (No assets in Resources)" };
                        dropdownValues = new object[] { null };
                    }

                    var initialSo = dropdownValues != null && dropdownValues.Length > 0 ? dropdownValues[0] as ScriptableObject : null;
                    var initialCode = string.Empty;
                    if (initialSo is RecipeDefinition r) initialCode = r.RecipeCode ?? r.name;
                    else if (initialSo is ItemDefinition i) initialCode = i.ItemCode ?? i.name;
                    else if (initialSo != null) initialCode = initialSo.name;

                    return new TestMethodParameterInfo
                    {
                        Name = p.Name,
                        ParameterType = paramType,
                        Kind = kind,
                        AssociatedScriptableObjectType = associatedSoType,
                        IsOptional = p.IsOptional,
                        DefaultValue = defaultValue,
                        CurrentStringValue = initialCode,
                        DropdownOptions = dropdownOptions,
                        DropdownValues = dropdownValues,
                        SelectedDropdownIndex = 0,
                        ResolvedValue = initialSo
                    };
                }

                kind = ParameterKind.String;
            }
            else
            {
                kind = ParameterKind.ObjectOrJson;
            }

            return new TestMethodParameterInfo
            {
                Name = p.Name,
                ParameterType = paramType,
                Kind = kind,
                IsOptional = p.IsOptional,
                DefaultValue = defaultValue,
                CurrentStringValue = currentStr,
                DropdownOptions = dropdownOptions,
                DropdownValues = dropdownValues,
                SelectedDropdownIndex = 0
            };
        }

        private static object GetDefaultTypeValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }
            return null;
        }

        private static string InferCategory(Type type)
        {
            var ns = type.Namespace ?? string.Empty;
            if (ns.Contains("Crafting")) return "Crafting";
            if (ns.Contains("Inventory")) return "Inventory";
            if (ns.Contains("Interaction")) return "Interaction";
            if (ns.Contains("Gathering")) return "Gathering";
            if (ns.Contains("Character")) return "Character";
            if (ns.Contains("Save")) return "Save";
            if (ns.Contains("Core")) return "Core / System";
            return "General";
        }

        private static string FormatTypeName(Type type)
        {
            var name = type.Name;
            if (name.StartsWith("I") && name.Length > 2 && char.IsUpper(name[1]))
            {
                name = name.Substring(1); // e.g. ICraftingService -> CraftingService
            }
            return name;
        }

        private static string BuildSignature(MethodInfo method, List<TestMethodParameterInfo> parameters)
        {
            var retName = method.ReturnType == typeof(void) ? "void" : method.ReturnType.Name;
            var paramStrings = parameters.Select(p => $"{p.ParameterType.Name} {p.Name}");
            return $"{retName} {method.Name}({string.Join(", ", paramStrings)})";
        }
    }
}
#endif
