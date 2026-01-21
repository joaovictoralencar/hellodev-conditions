using HelloDev.Events;
using HelloDev.IDs;
using UnityEngine;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Event modifier for ID-based events.
    /// Use for events like OnMonsterKilled(ID), OnItemCollected(ID), etc.
    /// </summary>
    /// <remarks>
    /// Requires a GameEvent_SO&lt;ID_SO&gt; implementation such as GameEventID_SO.
    /// </remarks>
    [CreateAssetMenu(menuName = "HelloDev/Conditions/World Flags/Event Modifier (ID)")]
    public class WorldFlagEventModifierID_SO : WorldFlagEventModifier_SO<ID_SO>
    {
        [SerializeField] private GameEvent_SO<ID_SO> gameEvent;

        /// <summary>
        /// Whether this modifier has valid configuration.
        /// </summary>
        public override bool IsValid => gameEvent != null && modification.IsValid;

        /// <summary>
        /// Subscribes to the ID event.
        /// </summary>
        protected override void SubscribeToEvent() => gameEvent?.AddListener(OnEventTriggered);

        /// <summary>
        /// Unsubscribes from the ID event.
        /// </summary>
        protected override void UnsubscribeFromEvent() => gameEvent?.RemoveListener(OnEventTriggered);

        /// <summary>
        /// Compares ID values. Only Equals and NotEquals are supported.
        /// </summary>
        protected override bool CompareValues(ID_SO eventValue, ID_SO target, ComparisonType comparison)
            => comparison == ComparisonType.Equals ? eventValue == target : eventValue != target;
    }
}
