using System;

namespace Worldforge.Character.State.Transition
{
    /// <summary>
    /// Defines a single state transition rule for the character state machine.
    /// Readonly struct — no heap allocation when created or evaluated.
    /// <para>
    /// <see cref="FromState"/> == <see cref="CharacterStateId.None"/> means the transition
    /// applies from every state (global transition).
    /// </para>
    /// </summary>
    public readonly struct CharacterStateTransition
    {
        /// <summary>
        /// Source state. Use <see cref="CharacterStateId.None"/> for a global transition.
        /// </summary>
        public CharacterStateId FromState { get; }

        /// <summary>Target state when the condition is satisfied.</summary>
        public CharacterStateId ToState { get; }

        /// <summary>
        /// Evaluation priority. Lower value = higher priority.
        /// Registration order breaks ties within the same priority.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Condition delegate. Returns true if the transition should fire.
        /// Must be side-effect free.
        /// </summary>
        public Func<CharacterStateContext, bool> Condition { get; }

        public CharacterStateTransition(
            CharacterStateId fromState,
            CharacterStateId toState,
            int priority,
            Func<CharacterStateContext, bool> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            FromState = fromState;
            ToState = toState;
            Priority = priority;
            Condition = condition;
        }

        /// <summary>Creates a global transition (applies from any state).</summary>
        public static CharacterStateTransition Global(
            CharacterStateId toState,
            int priority,
            Func<CharacterStateContext, bool> condition)
        {
            return new CharacterStateTransition(CharacterStateId.None, toState, priority, condition);
        }

        /// <summary>Evaluates this transition against the current state and context.</summary>
        public bool Evaluate(CharacterStateId currentStateId, in CharacterStateContext context)
        {
            var isApplicable = FromState == CharacterStateId.None || FromState == currentStateId;
            return isApplicable && Condition(context);
        }

        public override string ToString()
        {
            return $"{FromState} → {ToState} [P{Priority}]";
        }
    }
}
