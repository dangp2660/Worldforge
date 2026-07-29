using System;
using UnityEngine;

namespace Worldforge.Character.Player
{
    public sealed class PlayerAvatar : MonoBehaviour
    {
        public string PlayerId { get; private set; }
        public bool IsGamePlayerReady { get; private set; }
        public void Initialize(string playerId)
        {
            if(string.IsNullOrWhiteSpace(playerId))
            {
                throw new ArgumentException("Player id must be a non-empty value.", nameof(playerId));
            }

            if(IsGamePlayerReady)
            {
                throw new InvalidOperationException("Player avatar has already been initialized.");
            }

            PlayerId = playerId;
            IsGamePlayerReady = true;
        }
    }
}
