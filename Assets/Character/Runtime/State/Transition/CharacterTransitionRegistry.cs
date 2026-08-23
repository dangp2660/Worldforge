using System.Collections.Generic;

namespace Worldforge.Character.State.Transition
{
    /// <summary>
    /// Holds the ordered list of <see cref="CharacterStateTransition"/> rules.
    /// Built once at composition time — no MonoBehaviour dependency.
    /// Can be extended to load from a ScriptableObject asset in future milestones.
    /// </summary>
    public sealed class CharacterTransitionRegistry
    {
        private readonly CharacterStateTransition[] _transitions;

        private CharacterTransitionRegistry(CharacterStateTransition[] transitions)
        {
            _transitions = transitions;
        }

        /// <summary>Sorted transition list (ascending priority).</summary>
        public IReadOnlyList<CharacterStateTransition> Transitions => _transitions;

        /// <summary>
        /// Creates the default transition table for Worldforge v0.1.
        /// Priority ordering: Death (0) > Airborne (10) > Walk/Idle (20).
        /// </summary>
        public static CharacterTransitionRegistry CreateDefault()
        {
            var transitions = new[]
            {
                // Global — death takes priority over everything.
                CharacterStateTransition.Global(
                    toState: CharacterStateId.Dead,
                    priority: 0,
                    condition: ctx => !ctx.IsAlive),

                // Idle
                new CharacterStateTransition(
                    fromState: CharacterStateId.Idle,
                    toState: CharacterStateId.Airborne,
                    priority: 10,
                    condition: ctx => ctx.IsAlive && !ctx.IsGrounded),

                new CharacterStateTransition(
                    fromState: CharacterStateId.Idle,
                    toState: CharacterStateId.Locomotion,
                    priority: 20,
                    condition: ctx => ctx.IsAlive && ctx.IsGrounded && ctx.HasMoveInput),

                // Locomotion
                new CharacterStateTransition(
                    fromState: CharacterStateId.Locomotion,
                    toState: CharacterStateId.Airborne,
                    priority: 10,
                    condition: ctx => ctx.IsAlive && !ctx.IsGrounded),

                new CharacterStateTransition(
                    fromState: CharacterStateId.Locomotion,
                    toState: CharacterStateId.Idle,
                    priority: 20,
                    condition: ctx => ctx.IsAlive && ctx.IsGrounded && !ctx.HasMoveInput),

                // Airborne
                new CharacterStateTransition(
                    fromState: CharacterStateId.Airborne,
                    toState: CharacterStateId.Locomotion,
                    priority: 10,
                    condition: ctx => ctx.IsAlive && ctx.IsGrounded && ctx.HasMoveInput),

                new CharacterStateTransition(
                    fromState: CharacterStateId.Airborne,
                    toState: CharacterStateId.Idle,
                    priority: 20,
                    condition: ctx => ctx.IsAlive && ctx.IsGrounded && !ctx.HasMoveInput),

                // Interacting — exits only via ForceTransition from the Interaction System.
            };

            System.Array.Sort(transitions, (a, b) => a.Priority.CompareTo(b.Priority));

            return new CharacterTransitionRegistry(transitions);
        }
    }
}
