using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Worldforge.Character.Spawning;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Interaction
{
    /// <summary>
    /// Initializes the interaction system after the player and character state systems.
    /// Order 135 — runs after PlayerSpawn (120), CharacterMovement (125), CharacterState (130).
    /// Listens for scene load events and attaches <see cref="InteractionBehaviour"/> to the active player.
    /// </summary>
    internal sealed class InteractionInitializationSystem : IApplicationSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList =
            new[] { "Gameplay.PlayerSpawn", "Gameplay.CharacterState" };

        private IInteractionService _interactionService;
        private IPlayerSpawnService _spawnService;
        private ILogService _logger;

        public string Name
        {
            get { return "Gameplay.Interaction"; }
        }

        public int Order
        {
            get { return 135; }
        }

        public ApplicationSystemCategory Category
        {
            get { return ApplicationSystemCategory.Gameplay; }
        }

        public IReadOnlyList<string> Dependencies
        {
            get { return DependenciesList; }
        }

        public void Initialize(ApplicationBootstrapContext context)
        {
            _interactionService = context.Services.Resolve<IInteractionService>();
            _spawnService = context.Services.Resolve<IPlayerSpawnService>();

            if (context.Services.TryResolve<ILogService>(out var resolvedLogger))
            {
                _logger = resolvedLogger;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (_spawnService != null)
            {
                _spawnService.PlayerSpawned -= OnPlayerSpawned;
                _spawnService.PlayerSpawned += OnPlayerSpawned;
            }

            context.RegisterEventSubscription(
                "Gameplay.Interaction.SceneLoaded",
                () => SceneManager.sceneLoaded -= OnSceneLoaded,
                135);

            TryAttachToActivePlayer();

            _logger?.Info("Gameplay.Interaction", "Interaction system initialized.");
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
            if (_spawnService != null)
            {
                _spawnService.PlayerSpawned -= OnPlayerSpawned;
            }

            if (_interactionService is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _interactionService = null;
            _spawnService = null;
            _logger = null;
        }

        private void OnPlayerSpawned(GameObject player)
        {
            TryAttachToActivePlayer();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryAttachToActivePlayer();
        }

        private void TryAttachToActivePlayer()
        {
            if (_spawnService == null || !_spawnService.HasActivePlayer) return;
            if (_interactionService == null) return;

            var playerObject = _spawnService.ActivePlayer;
            if (playerObject == null) return;

            if (playerObject.GetComponent<InteractionBehaviour>() != null) return;

            try
            {
                var behaviour = playerObject.AddComponent<InteractionBehaviour>();

                if (_interactionService is IInteractionServiceInternal internalService)
                {
                    internalService.AttachToBehaviour(behaviour);
                }

                _logger?.Info(
                    "Gameplay.Interaction",
                    $"InteractionBehaviour attached to '{playerObject.name}'.");
            }
            catch (Exception exception)
            {
                _logger?.Error(
                    "Gameplay.Interaction",
                    "Failed to attach InteractionBehaviour to active player.",
                    exception);
            }
        }
    }
}
