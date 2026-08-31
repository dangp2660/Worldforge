using System.Collections.Generic;
using UnityEngine;

namespace Worldforge.Building
{
    // Registry holding all StructureDefinition assets for Building System lookup.
    // Provides code-based and category-based queries.
    [CreateAssetMenu(
        fileName = "StructureDefinitionRegistry",
        menuName = "Worldforge/Building/Structure Definition Registry")]
    public sealed class StructureDefinitionRegistry : ScriptableObject
    {
        [SerializeField] private List<StructureDefinition> _definitions = new();

        public IReadOnlyList<StructureDefinition> Definitions
        {
            get { return _definitions; }
        }

        public int Count
        {
            get { return _definitions != null ? _definitions.Count : 0; }
        }

        public StructureDefinition GetByCode(string structureCode)
        {
            if (string.IsNullOrEmpty(structureCode) || _definitions == null)
            {
                return null;
            }

            for (var i = 0; i < _definitions.Count; i++)
            {
                var definition = _definitions[i];
                if (definition != null
                    && string.Equals(definition.StructureCode, structureCode, System.StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            return null;
        }

        public List<StructureDefinition> GetByCategory(StructureCategoryType category)
        {
            var results = new List<StructureDefinition>();

            if (_definitions == null)
            {
                return results;
            }

            for (var i = 0; i < _definitions.Count; i++)
            {
                var definition = _definitions[i];
                if (definition != null && definition.Category == category)
                {
                    results.Add(definition);
                }
            }

            return results;
        }

        public List<StructureDefinition> GetByFunction(StructureFunctionType functionType)
        {
            var results = new List<StructureDefinition>();

            if (_definitions == null)
            {
                return results;
            }

            for (var i = 0; i < _definitions.Count; i++)
            {
                var definition = _definitions[i];
                if (definition != null && definition.FunctionType == functionType)
                {
                    results.Add(definition);
                }
            }

            return results;
        }

        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();

            if (_definitions == null || _definitions.Count == 0)
            {
                errors.Add("Registry has no structure definitions.");
                return false;
            }

            var codeSet = new HashSet<string>();

            for (var i = 0; i < _definitions.Count; i++)
            {
                var definition = _definitions[i];

                if (definition == null)
                {
                    errors.Add($"Null definition at index {i}.");
                    continue;
                }

                if (!codeSet.Add(definition.StructureCode))
                {
                    errors.Add($"Duplicate StructureCode '{definition.StructureCode}' at index {i}.");
                }

                if (!definition.IsValid(out var reason))
                {
                    errors.Add($"Invalid definition at index {i}: {reason}");
                }
            }

            return errors.Count == 0;
        }

        private void OnValidate()
        {
            // Remove null entries
            if (_definitions != null)
            {
                _definitions.RemoveAll(d => d == null);
            }
        }
    }
}
