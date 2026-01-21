using HelloDev.Events;
using UnityEngine;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Event modifier for integer events.
    /// Use for events like OnScoreChanged(int), OnLevelReached(int), etc.
    /// Supports full comparison operators (Equals, NotEquals, GreaterThan, etc.).
    /// </summary>
    [CreateAssetMenu(menuName = "HelloDev/Conditions/World Flags/Event Modifier (Int)")]
    public class WorldFlagEventModifierInt_SO : WorldFlagEventModifier_SO<int>
    {
        [SerializeField] private GameEventInt_SO gameEvent;

        /// <summary>
        /// Whether this modifier has valid configuration.
        /// </summary>
        public override bool IsValid => gameEvent != null && modification.IsValid;

        /// <summary>
        /// Subscribes to the int event.
        /// </summary>
        protected override void SubscribeToEvent() => gameEvent?.AddListener(OnEventTriggered);

        /// <summary>
        /// Unsubscribes from the int event.
        /// </summary>
        protected override void UnsubscribeFromEvent() => gameEvent?.RemoveListener(OnEventTriggered);

        /// <summary>
        /// Compares integer values with full comparison support.
        /// </summary>
        protected override bool CompareValues(int eventValue, int target, ComparisonType comparison)
        {
            return comparison switch
            {
                ComparisonType.Equals => eventValue == target,
                ComparisonType.NotEquals => eventValue != target,
                ComparisonType.GreaterThan => eventValue > target,
                ComparisonType.LessThan => eventValue < target,
                ComparisonType.GreaterThanOrEqual => eventValue >= target,
                ComparisonType.LessThanOrEqual => eventValue <= target,
                _ => false
            };
        }
    }
}
