using System;
using UnityEngine;
using Worldforge.Character.Traversal;
using Worldforge.Core.Services;

namespace Worldforge.Character.Movement
{
    internal sealed class RuntimeCharacterMovementService : ICharacterMovementService, IDisposable
    {
        private readonly CharacterMovementConfiguration _configuration;
        private readonly TraversalConfiguration _traversalConfiguration;
        private readonly ILogService _logger;

        private CharacterMovementController _controller;
        private GameObject _attachedPlayer;
        private TraversalServiceAdapter _traversalAdapter;

        public RuntimeCharacterMovementService(
            CharacterMovementConfiguration configuration,
            ILogService logger,
            TraversalConfiguration traversalConfiguration = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
            _traversalConfiguration = traversalConfiguration;
        }

        public bool IsAttached
        {
            get { return _controller != null && _attachedPlayer != null; }
        }

        public bool IsGrounded
        {
            get { return _controller != null && _controller.IsGrounded; }
        }

        public bool IsSprinting
        {
            get { return _controller != null && _controller.IsSprinting; }
        }

        public Vector3 CurrentVelocity
        {
            get { return _controller != null ? _controller.CurrentVelocity : Vector3.zero; }
        }

        public ITraversalService Traversal
        {
            get { return _traversalAdapter; }
        }

        public void AttachToPlayer(GameObject playerObject)
        {
            if (playerObject == null)
            {
                throw new ArgumentNullException(nameof(playerObject));
            }

            if (_attachedPlayer == playerObject && _controller != null)
            {
                return;
            }

            DetachFromPlayer();

            _controller = playerObject.GetComponent<CharacterMovementController>();

            if (_controller == null)
            {
                _controller = playerObject.AddComponent<CharacterMovementController>();
            }

            _controller.Initialize(_configuration, _logger, _traversalConfiguration);
            _attachedPlayer = playerObject;

            if (_controller.HasTraversalSystem)
            {
                _traversalAdapter = new TraversalServiceAdapter(_controller);
            }

            _logger?.Info(
                "Gameplay.CharacterMovement",
                $"Movement controller attached to '{playerObject.name}'.");
        }

        public void DetachFromPlayer()
        {
            if (_controller != null)
            {
                _controller.Shutdown();
                _controller = null;
            }

            _traversalAdapter = null;

            if (_attachedPlayer != null)
            {
                _logger?.Info(
                    "Gameplay.CharacterMovement",
                    $"Movement controller detached from '{_attachedPlayer.name}'.");
            }

            _attachedPlayer = null;
        }

        public void Dispose()
        {
            DetachFromPlayer();
        }

        /// <summary>
        /// Adapter that bridges the CharacterMovementController's traversal state
        /// to the ITraversalService interface for external consumers.
        /// </summary>
        private sealed class TraversalServiceAdapter : ITraversalService
        {
            private readonly CharacterMovementController _controller;

            public TraversalServiceAdapter(CharacterMovementController controller)
            {
                _controller = controller;
            }

            public bool IsTraversalActive
            {
                get { return _controller != null && _controller.HasTraversalSystem; }
            }

            public TraversalCheckResult LastResult
            {
                get
                {
                    return _controller != null
                        ? _controller.LastTraversalResult
                        : TraversalCheckResult.DefaultAllowed;
                }
            }

            public SurfaceType CurrentSurface
            {
                get
                {
                    return _controller != null
                        ? _controller.LastTraversalResult.SurfaceType
                        : SurfaceType.Default;
                }
            }

            public float CurrentSpeedMultiplier
            {
                get
                {
                    return _controller != null
                        ? _controller.LastTraversalResult.EffectiveSpeedMultiplier
                        : 1f;
                }
            }
        }
    }
}

