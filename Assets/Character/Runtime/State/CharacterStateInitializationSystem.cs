using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Worldforge.Character.Spawning;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Character.State
{
    /// <summary>
    /// Internal seam that allows <see cref="CharacterStateInitializationSystem"/> to attach
    /// a <see cref="CharacterStateBehaviour"/> without casting to the concrete implementation.
    /// </summary>
    internal interface ICharacterStateServiceInternal
    {
        void AttachToBehaviour(CharacterStateBehaviour behaviour);
    }

    /// <summary>
    /// Initializes the character state system after the player has spawned.
    /// Order 130 — runs after PlayerSpawn (120) and CharacterMovement (125).
    /// Listens for scene load events and attaches <see cref="CharacterStateBehaviour"/>
    /// to the active player object.
    /// </summary>
    internal sealed class CharacterStateInitializationSystem : IApplicationSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList =
            new[] { "Gameplay.PlayerSpawn", "Gameplay.CharacterMovement" };

        private ICharacterStateService _stateService;
        private IPlayerSpawnService _spawnService;
        private ILogService _logger;

        public string Name => "Gameplay.CharacterState";

        public int Order => 130;

        public ApplicationSystemCategory Category => ApplicationSystemCategory.Gameplay;

        public IReadOnlyList<string> Dependencies => DependenciesList;

        public void Initialize(ApplicationBootstrapContext context)
        {
            _stateService = context.Services.Resolve<ICharacterStateService>();
            _spawnService = context.Services.Resolve<IPlayerSpawnService>();

            if (context.Services.TryResolve<ILogService>(out var resolvedLogger))
            {
                _logger = resolvedLogger;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            context.RegisterEventSubscription(
                "Gameplay.CharacterState.SceneLoaded",
                () => SceneManager.sceneLoaded -= OnSceneLoaded,
                130);

            // Attach immediately if the player was already spawned (e.g. hot reload).
            TryAttachToActivePlayer();

            _logger?.Info("Gameplay.CharacterState", "Character state system initialized.");
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
            if (_stateService is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _stateService = null;
            _spawnService = null;
            _logger = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryAttachToActivePlayer();
        }

        private void TryAttachToActivePlayer()
        {
            if (_spawnService == null || !_spawnService.HasActivePlayer) return;
            if (_stateService == null) return;

            var playerObject = _spawnService.ActivePlayer;
            if (playerObject == null) return;

            // Idempotent — skip if already attached.
            if (playerObject.GetComponent<CharacterStateBehaviour>() != null) return;

            try
            {
                var behaviour = playerObject.AddComponent<CharacterStateBehaviour>();

                if (_stateService is ICharacterStateServiceInternal internalService)
                {
                    internalService.AttachToBehaviour(behaviour);
                }

                _logger?.Info(
                    "Gameplay.CharacterState",
                    $"CharacterStateBehaviour attached to '{playerObject.name}'.");
            }
            catch (Exception exception)
            {
                _logger?.Error(
                    "Gameplay.CharacterState",
                    "Failed to attach CharacterStateBehaviour to active player.",
                    exception);
            }
        }
    }
}
