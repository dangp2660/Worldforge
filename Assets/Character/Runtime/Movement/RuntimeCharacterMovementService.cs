using System;
using UnityEngine;
using Worldforge.Core.Services;

namespace Worldforge.Character.Movement
{
    internal sealed class RuntimeCharacterMovementService : ICharacterMovementService, IDisposable
    {
        private readonly CharacterMovementConfiguration _configuration;
        private readonly ILogService _logger;

        private CharacterMovementController _controller;
        private GameObject _attachedPlayer;

        public RuntimeCharacterMovementService(
            CharacterMovementConfiguration configuration,
            ILogService logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
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

            _controller.Initialize(_configuration, _logger);
            _attachedPlayer = playerObject;

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
    }
}
