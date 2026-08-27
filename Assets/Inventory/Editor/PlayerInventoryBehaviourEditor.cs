using System;
using UnityEditor;
using UnityEngine;
using Worldforge.Item;

namespace Worldforge.Inventory.Editor
{
    /// <summary>
    /// Custom Inspector editor for PlayerInventoryBehaviour providing live weight bars,
    /// occupied slot inspection, and test actions during Play Mode.
    /// </summary>
    [CustomEditor(typeof(PlayerInventoryBehaviour))]
    public sealed class PlayerInventoryBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var playerInventory = (PlayerInventoryBehaviour)target;
            if (playerInventory == null)
            {
                return;
            }

            var container = playerInventory.Container;
            if (container == null)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.HelpBox("Container not yet initialized. Enter Play Mode or click 'Initialize Container' below.", MessageType.Info);
                if (GUILayout.Button("Initialize Container"))
                {
                    playerInventory.InitializeContainer();
                    EditorUtility.SetDirty(playerInventory);
                }
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Live Inventory Visualizer", EditorStyles.boldLabel);

            var weightRatio = container.MaxWeight > 0 ? Mathf.Clamp01(container.CurrentWeight / container.MaxWeight) : 0f;
            var weightLabel = $"Weight: {container.CurrentWeight:F1} / {container.MaxWeight:F1} kg ({(weightRatio * 100f):F0}%)";

            var prevColor = GUI.color;
            if (container.IsOverencumbered)
            {
                GUI.color = new Color(1f, 0.4f, 0.4f);
            }
            else if (weightRatio > 0.8f)
            {
                GUI.color = new Color(1f, 0.85f, 0.3f);
            }
            else
            {
                GUI.color = new Color(0.4f, 1f, 0.5f);
            }

            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 20), weightRatio, weightLabel);
            GUI.color = prevColor;

            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Used Slots: {container.SlotCount - container.EmptySlotCount} / {container.SlotCount}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Total Items: {container.TotalItemCount}", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Occupied Slots", EditorStyles.boldLabel);

            var occupiedCount = 0;
            for (var i = 0; i < container.SlotCount; i++)
            {
                var slot = container.GetSlot(i);
                if (slot != null && !slot.IsEmpty && slot.Item != null)
                {
                    occupiedCount++;
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"Slot {i:D2}: {slot.Item.DisplayName}", EditorStyles.boldLabel);
                            EditorGUILayout.LabelField($"Qty: {slot.Quantity}/{slot.Item.MaxStack}", GUILayout.Width(80));
                            EditorGUILayout.LabelField($"{slot.TotalWeight:F1} kg", GUILayout.Width(60));
                        }
                        EditorGUILayout.LabelField($"Category: {slot.Item.Category} | Durability: {slot.CurrentDurability:F0}%", EditorStyles.miniLabel);
                    }
                }
            }

            if (occupiedCount == 0)
            {
                EditorGUILayout.HelpBox("Inventory is currently empty.", MessageType.None);
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Play Mode Actions", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Auto Sort"))
                    {
                        playerInventory.AutoSort();
                    }

                    if (GUILayout.Button("Clear All"))
                    {
                        container.Clear();
                        EditorUtility.SetDirty(playerInventory);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("+ 10 Wood"))
                    {
                        var wood = Resources.Load<ItemDefinition>("Definitions/Items/Item_Resource_Wood");
                        if (wood != null)
                        {
                            playerInventory.ReceiveItem(wood, 10);
                        }
                    }

                    if (GUILayout.Button("+ 5 Stone"))
                    {
                        var stone = Resources.Load<ItemDefinition>("Definitions/Items/Item_Resource_Stone");
                        if (stone != null)
                        {
                            playerInventory.ReceiveItem(stone, 5);
                        }
                    }

                    if (GUILayout.Button("+ 3 Potion"))
                    {
                        var potion = Resources.Load<ItemDefinition>("Definitions/Items/Item_Consumable_HealthPotion");
                        if (potion != null)
                        {
                            playerInventory.ReceiveItem(potion, 3);
                        }
                    }
                }
            }
        }
    }
}
