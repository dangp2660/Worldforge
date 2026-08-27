using System;
using System.Collections.Generic;
using UnityEngine;
using Worldforge.Core.Services;
using Worldforge.Inventory.Services;
using Worldforge.Item;

namespace Worldforge.Inventory
{
    [Serializable]
    public struct StartingItemEntry
    {
        [SerializeField] private ItemDefinition _item;
        [SerializeField] private int _amount;

        public StartingItemEntry(ItemDefinition item, int amount)
        {
            _item = item;
            _amount = amount;
        }

        public ItemDefinition Item
        {
            get { return _item; }
        }

        public int Amount
        {
            get { return _amount; }
        }
    }

    /// <summary>
    /// Player inventory MonoBehaviour component integrating the player with inventory services and gathered item reception.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Worldforge/Inventory/Player Inventory Behaviour")]
    public sealed class PlayerInventoryBehaviour : MonoBehaviour, IGatheredItemReceiver
    {
        [Header("Container Configuration")]
        [SerializeField] private string _containerId = "PlayerInventory";
        [SerializeField, Range(1, 100)] private int _initialSlotCount = 20;
        [SerializeField] private float _maxWeightLimit = 50f;

        [Header("Starting Items")]
        [SerializeField] private List<StartingItemEntry> _startingItems = new();

        [Header("Runtime State")]
        [SerializeField] private InventoryContainer _container;

        public InventoryContainer Container
        {
            get { return _container; }
        }

        public string ContainerId
        {
            get { return _containerId; }
        }

        public int SlotCount
        {
            get { return _container != null ? _container.SlotCount : _initialSlotCount; }
        }

        public float CurrentWeight
        {
            get { return _container != null ? _container.CurrentWeight : 0f; }
        }

        public float MaxWeight
        {
            get { return _container != null ? _container.MaxWeight : _maxWeightLimit; }
        }

        public bool IsOverencumbered
        {
            get { return _container != null && _container.IsOverencumbered; }
        }

        private void Awake()
        {
            InitializeContainer();
        }

        public void InitializeContainer()
        {
            if (_container == null || _container.SlotCount == 0)
            {
                _container = new InventoryContainer(_containerId, _initialSlotCount, _maxWeightLimit);

                if (_startingItems != null && _startingItems.Count > 0)
                {
                    for (var i = 0; i < _startingItems.Count; i++)
                    {
                        var entry = _startingItems[i];
                        if (entry.Item != null && entry.Amount > 0)
                        {
                            _container.AddItem(entry.Item, entry.Amount);
                        }
                    }
                }
            }
        }

        public bool ReceiveItem(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return false;
            }

            if (_container == null)
            {
                InitializeContainer();
            }

            var accepted = _container.AddItem(item, amount);
            if (accepted > 0)
            {
                Debug.Log(
                    $"[PlayerInventory] Received {accepted}x '{item.DisplayName}'. " +
                    $"Total in bag: {_container.GetItemCount(item)}. " +
                    $"Weight: {_container.CurrentWeight:F1}/{_container.MaxWeight:F1}kg.");
                return true;
            }

            Debug.LogWarning(
                $"[PlayerInventory] Could not receive '{item.DisplayName}' (Inventory full or overweight).");
            return false;
        }
    }
}
