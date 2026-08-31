#if UNITY_EDITOR || WORLDFORGE_DEVELOPMENT_BUILD || WORLDFORGE_DEBUG_TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Worldforge.Infrastructure.Development.MethodTester
{
    public sealed class MethodTesterManager
    {
        private static MethodTesterManager instance;

        public static MethodTesterManager Instance => instance ??= new MethodTesterManager();

        public bool IsWindowOpen { get; set; }

        public string SearchText { get; set; } = string.Empty;

        public List<TestServiceDescriptor> Services { get; private set; } = new List<TestServiceDescriptor>();

        public TestServiceDescriptor SelectedService { get; set; }

        public TestMethodDescriptor SelectedMethod { get; set; }

        public MethodExecutionReport LastReport { get; private set; }

        public List<MethodTestPreset> Presets { get; private set; } = new List<MethodTestPreset>();

        public int SelectedPresetIndex { get; set; } = 0;

        public string NewPresetName { get; set; } = "My Preset";

        public void Initialize()
        {
            RefreshServices();
        }

        public void ToggleWindow()
        {
            IsWindowOpen = !IsWindowOpen;
            if (IsWindowOpen && (Services == null || Services.Count == 0))
            {
                RefreshServices();
            }
        }

        public void RefreshServices()
        {
            Services = DynamicMethodScanner.ScanAllServices();

            // Retain or select first method
            if (SelectedMethod != null)
            {
                var match = Services
                    .SelectMany(s => s.AllMethods)
                    .FirstOrDefault(m => m.DisplayName == SelectedMethod.DisplayName && m.TargetType == SelectedMethod.TargetType);

                if (match != null)
                {
                    SelectedMethod = match;
                    SelectedService = Services.FirstOrDefault(s => s.AllMethods.Contains(match));
                }
                else
                {
                    SelectFirstAvailableMethod();
                }
            }
            else
            {
                SelectFirstAvailableMethod();
            }
        }

        private void SelectFirstAvailableMethod()
        {
            var firstService = Services.FirstOrDefault(s => s.AllMethods.Count > 0);
            if (firstService != null)
            {
                SelectedService = firstService;
                SelectedMethod = firstService.AllMethods.FirstOrDefault();
            }
            else
            {
                SelectedService = null;
                SelectedMethod = null;
            }
        }

        public void ExecuteSelectedMethod()
        {
            if (SelectedMethod == null)
            {
                return;
            }

            LastReport = DynamicMethodInvoker.Execute(SelectedMethod);
        }

        public void SaveCurrentAsPreset(string presetName)
        {
            if (SelectedMethod == null || string.IsNullOrWhiteSpace(presetName))
            {
                return;
            }

            var preset = new MethodTestPreset
            {
                PresetName = presetName,
                ServiceTypeName = SelectedMethod.TargetType?.FullName,
                MethodName = SelectedMethod.Method.Name
            };

            foreach (var p in SelectedMethod.Parameters)
            {
                preset.ParameterNames.Add(p.Name);
                preset.ParameterValues.Add(p.CurrentStringValue);
            }

            // Replace existing or add new
            Presets.RemoveAll(p => p.PresetName == presetName && p.MethodName == SelectedMethod.Method.Name);
            Presets.Add(preset);
        }

        public void ApplyPreset(MethodTestPreset preset)
        {
            if (preset == null || SelectedMethod == null)
            {
                return;
            }

            for (var i = 0; i < preset.ParameterNames.Count; i++)
            {
                var pName = preset.ParameterNames[i];
                var pVal = preset.ParameterValues[i];
                var targetParam = SelectedMethod.Parameters.FirstOrDefault(p => p.Name == pName);
                if (targetParam != null)
                {
                    targetParam.CurrentStringValue = pVal;
                }
            }
        }
    }
}
#endif
