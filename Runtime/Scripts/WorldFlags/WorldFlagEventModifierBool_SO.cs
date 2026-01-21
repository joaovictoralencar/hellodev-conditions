using HelloDev.Events;
using UnityEngine;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Event modifier for boolean events.
    /// Use for events like OnDoorOpened(bool), OnToggleChanged(bool), etc.
    /// </summary>
    [CreateAssetMenu(menuName = "HelloDev/Conditions/World Flags/Event Modifier (Bool)")]
    public class WorldFlagEventModifierBool_SO : WorldFlagEventModifier_SO<bool>
    {
        [SerializeField] private GameEventBool_SO gameEvent;

        /// <summary>
        /// Whether this modifier has valid configuration.
        /// </summary>
        public override bool IsValid => gameEvent != null && modification.IsValid;

        /// <summary>
        /// Subscribes to the bool event.
        /// </summary>
        protected override void SubscribeToEvent() => gameEvent?.AddListener(OnEventTriggered);

        /// <summary>
        /// Unsubscribes from the bool event.
        /// </summary>
        protected override void UnsubscribeFromEvent() => gameEvent?.RemoveListener(OnEventTriggered);

        /// <summary>
        /// Compares boolean values. Only Equals and NotEquals are supported.
        /// </summary>
        protected override bool CompareValues(bool eventValue, bool target, ComparisonType comparison)
            => comparison == ComparisonType.Equals ? eventValue == target : eventValue != target;
    }
}
