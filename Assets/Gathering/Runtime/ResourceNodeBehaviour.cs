using System;
using System.Collections.Generic;
using UnityEngine;
using Worldforge.Interaction;
using Worldforge.Item;

namespace Worldforge.Gathering
{
    /// <summary>
    /// Represents an interactive resource node in the world (e.g. Tree, Rock, Bush).
    /// Inherits from <see cref="InteractableBehaviour"/> to integrate with the Interaction System.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class ResourceNodeBehaviour : InteractableBehaviour
    {
        [Header("Resource Definition")]
        [SerializeField] private ResourceNodeDefinition _definition;

        [Header("Runtime State")]
        [SerializeField] private ResourceNodeState _state = ResourceNodeState.Available;
        [SerializeField] private float _currentHealth;
        [SerializeField] private float _remainingRespawnTime;

        [Header("Presentation & Visuals")]
        [SerializeField] private GameObject _intactVisual;
        [SerializeField] private GameObject _depletedVisual;
        [SerializeField] private bool _disableColliderWhenDepleted;
        [SerializeField] private bool _depleteOnSingleHarvest;

        private Collider _cachedCollider;
        private System.Random _random;

        public event Action<ResourceNodeBehaviour, ResourceNodeState> StateChanged;
        public event Action<ResourceNodeBehaviour, GatheringHarvestResult> Gathered;
        public event Action<ResourceNodeBehaviour> Depleted;
        public event Action<ResourceNodeBehaviour> Respawned;
        public event Action<ResourceNodeBehaviour, float, float> HealthChanged;

        public ResourceNodeDefinition Definition
        {
            get { return _definition; }
        }

        public ResourceNodeState State
        {
            get { return _state; }
        }

        public float CurrentHealth
        {
            get { return _currentHealth; }
        }

        public float MaxHealth
        {
            get { return _definition != null ? _definition.MaxHealth : 100f; }
        }

        public float HealthPercent
        {
            get { return MaxHealth > 0f ? Mathf.Clamp01(_currentHealth / MaxHealth) : 0f; }
        }

        public bool IsAvailable
        {
            get
            {
                return (_state == ResourceNodeState.Available || _state == ResourceNodeState.Gathering)
                       && isActiveAndEnabled
                       && _definition != null;
            }
        }

        public bool IsDepleted
        {
            get { return _state == ResourceNodeState.Depleted || _state == ResourceNodeState.Respawning; }
        }

        public bool IsRespawning
        {
            get { return _state == ResourceNodeState.Respawning; }
        }

        public float RemainingRespawnTime
        {
            get { return _remainingRespawnTime; }
        }

        public float RespawnProgress
        {
            get
            {
                if (_definition == null || _definition.RespawnTime <= 0f) return 1f;
                return Mathf.Clamp01(1f - (_remainingRespawnTime / _definition.RespawnTime));
            }
        }

        public GameObject IntactVisual
        {
            get { return _intactVisual; }
            set { _intactVisual = value; UpdateVisuals(); }
        }

        public GameObject DepletedVisual
        {
            get { return _depletedVisual; }
            set { _depletedVisual = value; UpdateVisuals(); }
        }

        public bool DisableColliderWhenDepleted
        {
            get { return _disableColliderWhenDepleted; }
            set { _disableColliderWhenDepleted = value; }
        }

        public bool DepleteOnSingleHarvest
        {
            get { return _depleteOnSingleHarvest; }
            set { _depleteOnSingleHarvest = value; }
        }

        public override InteractionType Type
        {
            get { return InteractionType.Gather; }
        }

        public override string InteractionPrompt
        {
            get { return BuildPrompt(); }
        }

        public override float InteractionDuration
        {
            get { return _definition != null ? Mathf.Max(0.1f, _definition.BaseGatherDuration) : 1.5f; }
        }

        public override bool IsInteractable
        {
            get { return base.IsInteractable && IsAvailable; }
        }

        private void Awake()
        {
            _cachedCollider = GetComponent<Collider>();
            _random = new System.Random();

            if (_definition != null && _currentHealth <= 0f && _state == ResourceNodeState.Available)
            {
                _currentHealth = _definition.MaxHealth;
            }

            SyncInteractableFields();
            UpdateVisuals();
        }

        private void Update()
        {
            if (_state != ResourceNodeState.Respawning)
            {
                return;
            }

            _remainingRespawnTime -= Time.deltaTime;
            if (_remainingRespawnTime <= 0f)
            {
                _remainingRespawnTime = 0f;
                Respawn();
            }
        }

        public void Initialize(ResourceNodeDefinition definition)
        {
            _definition = definition;
            _currentHealth = definition != null ? definition.MaxHealth : 100f;
            _remainingRespawnTime = 0f;
            SetState(ResourceNodeState.Available);

            SyncInteractableFields();
            UpdateVisuals();
        }

        public void SetDefinition(ResourceNodeDefinition definition)
        {
            Initialize(definition);
        }

        public void SetState(ResourceNodeState newState)
        {
            if (_state == newState)
            {
                return;
            }

            var previousState = _state;
            _state = newState;

            SetInteractable(IsAvailable);
            SyncInteractableFields();
            UpdateVisuals();

            StateChanged?.Invoke(this, newState);
        }

        public GatheringValidationResult ValidateGathering(IGatheringTool tool, float playerStamina, float distanceToNode)
        {
            if (_definition == null)
            {
                return GatheringValidationResult.Failed(
                    GatheringFailureReason.InvalidNode,
                    "Resource node has no definition assigned.");
            }

            if (IsDepleted || _state == ResourceNodeState.Disabled)
            {
                return GatheringValidationResult.Failed(
                    GatheringFailureReason.NodeDepleted,
                    "Resource node is depleted or unavailable.");
            }

            if (_definition.Requirements != null)
            {
                return _definition.Requirements.Validate(tool, playerStamina, distanceToNode);
            }

            return GatheringValidationResult.Success();
        }

        public float CalculateHarvestDamage(IGatheringTool tool)
        {
            if (_depleteOnSingleHarvest)
            {
                return _currentHealth;
            }

            var harvestPower = tool != null ? Mathf.Max(0.1f, tool.HarvestPower) : 1f;
            var hardness = _definition != null ? Mathf.Max(0.1f, _definition.Hardness) : 1f;

            // Damage formula scales with tool power vs hardness, normalized to baseline 50 dmg
            return Mathf.Max(1f, (harvestPower / hardness) * 50f);
        }

        public GatheringHarvestResult Harvest(IGatheringTool tool, GameObject interactor = null)
        {
            if (_definition == null)
            {
                return GatheringHarvestResult.Failed("Resource node definition is missing.");
            }

            if (IsDepleted || _state == ResourceNodeState.Disabled)
            {
                return GatheringHarvestResult.Failed("Resource node is not available.");
            }

            // 1. Calculate Primary Yield
            var min = Mathf.Max(1, _definition.PrimaryMinAmount);
            var max = Mathf.Max(min, _definition.PrimaryMaxAmount);
            var primaryAmount = _random.Next(min, max + 1);

            if (tool != null && tool.HarvestPower > _definition.Hardness && _definition.Hardness > 0f)
            {
                var surplusRatio = (tool.HarvestPower - _definition.Hardness) / _definition.Hardness;
                if (surplusRatio >= 1f)
                {
                    primaryAmount += Mathf.FloorToInt(surplusRatio);
                }
            }

            // 2. Calculate Bonus Yields
            List<BonusYieldResult> bonusResults = null;
            if (_definition.BonusYields != null && _definition.BonusYields.Length > 0)
            {
                bonusResults = new List<BonusYieldResult>();
                for (var i = 0; i < _definition.BonusYields.Length; i++)
                {
                    var entry = _definition.BonusYields[i];
                    if (entry == null || entry.Item == null) continue;

                    var roll = (float)_random.NextDouble();
                    if (roll <= entry.DropChance)
                    {
                        var bonusMin = Mathf.Max(1, entry.MinAmount);
                        var bonusMax = Mathf.Max(bonusMin, entry.MaxAmount);
                        var bonusAmount = _random.Next(bonusMin, bonusMax + 1);
                        bonusResults.Add(new BonusYieldResult(entry.Item, bonusAmount));
                    }
                }
            }

            // 3. Apply Damage and Depletion
            var damage = CalculateHarvestDamage(tool);
            _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            HealthChanged?.Invoke(this, _currentHealth, MaxHealth);

            var wasDepleted = _currentHealth <= 0f || _depleteOnSingleHarvest;
            if (wasDepleted)
            {
                Deplete(interactor);
            }
            else
            {
                SetState(ResourceNodeState.Available);
            }

            var result = GatheringHarvestResult.Success(
                _definition.PrimaryYield,
                primaryAmount,
                bonusResults,
                _definition.DiscoveryXP,
                damage,
                _currentHealth,
                wasDepleted);

            Gathered?.Invoke(this, result);

            return result;
        }

        public bool ApplyGatherDamage(float damage, out bool wasDepleted)
        {
            if (IsDepleted || _state == ResourceNodeState.Disabled)
            {
                wasDepleted = IsDepleted;
                return false;
            }

            damage = Mathf.Max(0f, damage);
            _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            HealthChanged?.Invoke(this, _currentHealth, MaxHealth);

            wasDepleted = _currentHealth <= 0f || _depleteOnSingleHarvest;
            if (wasDepleted)
            {
                Deplete();
            }

            return true;
        }

        public bool Deplete(GameObject interactor = null)
        {
            if (_state == ResourceNodeState.Depleted || _state == ResourceNodeState.Respawning)
            {
                return false;
            }

            _currentHealth = 0f;

            var canRespawn = _definition != null && _definition.CanRespawn && _definition.RespawnTime > 0f;
            if (canRespawn)
            {
                _remainingRespawnTime = _definition.RespawnTime;
                SetState(ResourceNodeState.Respawning);
            }
            else
            {
                _remainingRespawnTime = 0f;
                SetState(ResourceNodeState.Depleted);
            }

            if (_disableColliderWhenDepleted && _cachedCollider != null)
            {
                _cachedCollider.enabled = false;
            }

            Depleted?.Invoke(this);

            return true;
        }

        public void Respawn()
        {
            _currentHealth = MaxHealth;
            _remainingRespawnTime = 0f;
            SetState(ResourceNodeState.Available);

            if (_disableColliderWhenDepleted && _cachedCollider != null)
            {
                _cachedCollider.enabled = true;
            }

            Respawned?.Invoke(this);
        }

        public void SetDisabled(bool disabled)
        {
            if (disabled)
            {
                SetState(ResourceNodeState.Disabled);
            }
            else if (_state == ResourceNodeState.Disabled)
            {
                SetState(ResourceNodeState.Available);
            }
        }

        public void ResetNode()
        {
            _remainingRespawnTime = 0f;
            _currentHealth = MaxHealth;
            SetState(ResourceNodeState.Available);

            if (_cachedCollider != null)
            {
                _cachedCollider.enabled = true;
            }
        }

        public void CancelGathering()
        {
            if (_state == ResourceNodeState.Gathering)
            {
                SetState(ResourceNodeState.Available);
            }
        }

        public override void OnInteractionStarted(InteractionContext context)
        {
            base.OnInteractionStarted(context);
            if (_state == ResourceNodeState.Available)
            {
                SetState(ResourceNodeState.Gathering);
            }
        }

        public override void OnInteractionCancelled(InteractionContext context)
        {
            base.OnInteractionCancelled(context);
            CancelGathering();
        }

        public override void OnInteractionCompleted(InteractionContext context)
        {
            base.OnInteractionCompleted(context);
        }

        private void UpdateVisuals()
        {
            var depleted = IsDepleted;

            if (_intactVisual != null)
            {
                _intactVisual.SetActive(!depleted);
            }

            if (_depletedVisual != null)
            {
                _depletedVisual.SetActive(depleted);
            }
        }

        private void SyncInteractableFields()
        {
            SetInteractable(IsAvailable);
        }

        private string BuildPrompt()
        {
            if (_definition == null)
            {
                return "Gather Resource";
            }

            var nodeName = !string.IsNullOrWhiteSpace(_definition.DisplayName)
                ? _definition.DisplayName
                : "Resource";

            if (_definition.Requirements != null && _definition.Requirements.RequiresTool)
            {
                return $"Gather {nodeName} (Requires {_definition.Requirements.RequiredToolType})";
            }

            return $"Gather {nodeName}";
        }

        private void OnValidate()
        {
            if (_definition != null)
            {
                if (_currentHealth <= 0f && _state == ResourceNodeState.Available)
                {
                    _currentHealth = _definition.MaxHealth;
                }
            }

            SyncInteractableFields();
            UpdateVisuals();
        }
    }
}
