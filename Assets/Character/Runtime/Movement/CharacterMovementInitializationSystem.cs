using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Worldforge.Character.Spawning;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Character.Movement
{
    internal sealed class CharacterMovementInitializationSystem : IApplicationSystem
    {
        private static readonly IReadOnlyList<string> DependenciesList =
            new[] { "Gameplay.PlayerSpawn" };

        private ICharacterMovementService _movementService;
        private IPlayerSpawnService _spawnService;
        private ILogService _logger;

        public string Name
        {
            get { return "Gameplay.CharacterMovement"; }
        }

        public int Order
        {
            get { return 125; }
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
            _movementService = context.Services.Resolve<ICharacterMovementService>();
            _spawnService = context.Services.Resolve<IPlayerSpawnService>();

            if (context.Services.TryResolve<ILogService>(out var resolvedLogger))
            {
                _logger = resolvedLogger;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            context.RegisterEventSubscription(
                "Gameplay.CharacterMovement.SceneLoaded",
                () => SceneManager.sceneLoaded -= OnSceneLoaded,
                125);

            TryAttachToActivePlayer();

            _logger?.Info("Gameplay.CharacterMovement", "Character movement system initialized.");
        }

        public void Shutdown(ApplicationBootstrapContext context)
        {
            _movementService?.DetachFromPlayer();

            _movementService = null;
            _spawnService = null;
            _logger = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryAttachToActivePlayer();
        }

        private void TryAttachToActivePlayer()
        {
            if (_spawnService == null || !_spawnService.HasActivePlayer)
            {
                return;
            }

            if (_movementService == null)
            {
                return;
            }

            if (_movementService.IsAttached)
            {
                return;
            }

            try
            {
                _movementService.AttachToPlayer(_spawnService.ActivePlayer);
            }
            catch (Exception exception)
            {
                _logger?.Error(
                    "Gameplay.CharacterMovement",
                    "Failed to attach movement controller to active player.",
                    exception);
            }
        }
    }
}
