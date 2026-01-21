using HelloDev.Events;
using UnityEngine;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Event modifier for float events.
    /// Use for events like OnHealthChanged(float), OnProgressUpdated(float), etc.
    /// Supports full comparison operators with approximate equality for Equals/NotEquals.
    /// </summary>
    [CreateAssetMenu(menuName = "HelloDev/Conditions/World Flags/Event Modifier (Float)")]
    public class WorldFlagEventModifierFloat_SO : WorldFlagEventModifier_SO<float>
    {
        [SerializeField] private GameEventFloat_SO gameEvent;

        /// <summary>
        /// Whether this modifier has valid configuration.
        /// </summary>
        public override bool IsValid => gameEvent != null && modification.IsValid;

        /// <summary>
        /// Subscribes to the float event.
        /// </summary>
        protected override void SubscribeToEvent() => gameEvent?.AddListener(OnEventTriggered);

        /// <summary>
        /// Unsubscribes from the float event.
        /// </summary>
        protected override void UnsubscribeFromEvent() => gameEvent?.RemoveListener(OnEventTriggered);

        /// <summary>
        /// Compares float values with full comparison support.
        /// Uses Mathf.Approximately for Equals/NotEquals comparisons.
        /// </summary>
        protected override bool CompareValues(float eventValue, float target, ComparisonType comparison)
        {
            return comparison switch
            {
                ComparisonType.Equals => Mathf.Approximately(eventValue, target),
                ComparisonType.NotEquals => !Mathf.Approximately(eventValue, target),
                ComparisonType.GreaterThan => eventValue > target,
                ComparisonType.LessThan => eventValue < target,
                ComparisonType.GreaterThanOrEqual => eventValue >= target,
                ComparisonType.LessThanOrEqual => eventValue <= target,
                _ => false
            };
        }
    }
}
