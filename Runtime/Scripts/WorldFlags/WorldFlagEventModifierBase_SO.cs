using HelloDev.Utils;
using UnityEngine;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Base class for event-driven world flag modifiers.
    /// Listens to game events and applies world flag modifications when triggered.
    /// </summary>
    public abstract class WorldFlagEventModifierBase_SO : RuntimeScriptableObject
    {
        [SerializeField] protected WorldFlagModification modification;

        /// <summary>
        /// Whether this modifier is currently subscribed to its event.
        /// </summary>
        public bool IsSubscribed { get; protected set; }

        /// <summary>
        /// The modification to apply when the event triggers.
        /// </summary>
        public WorldFlagModification Modification => modification;

        /// <summary>
        /// Whether this modifier has valid configuration (event and modification).
        /// </summary>
        public abstract bool IsValid { get; }

        /// <summary>
        /// Subscribes to the underlying game event.
        /// </summary>
        public abstract void Subscribe();

        /// <summary>
        /// Unsubscribes from the underlying game event.
        /// </summary>
        public abstract void Unsubscribe();

        /// <summary>
        /// Applies the configured modification to the world flag.
        /// </summary>
        protected void ApplyModification() => modification.Apply();

        /// <summary>
        /// Called when entering play mode. Ensures modifier is unsubscribed.
        /// </summary>
        protected override void OnScriptableObjectReset() => Unsubscribe();
    }
}
