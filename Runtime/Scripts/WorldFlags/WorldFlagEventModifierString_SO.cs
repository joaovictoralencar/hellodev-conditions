using HelloDev.Events;
using UnityEngine;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Event modifier for string events.
    /// Use for events like OnDialogueChoice(string), OnCommandEntered(string), etc.
    /// </summary>
    [CreateAssetMenu(menuName = "HelloDev/Conditions/World Flags/Event Modifier (String)")]
    public class WorldFlagEventModifierString_SO : WorldFlagEventModifier_SO<string>
    {
        [SerializeField] private GameEventString_SO gameEvent;

        /// <summary>
        /// Whether this modifier has valid configuration.
        /// </summary>
        public override bool IsValid => gameEvent != null && modification.IsValid;

        /// <summary>
        /// Subscribes to the string event.
        /// </summary>
        protected override void SubscribeToEvent() => gameEvent?.AddListener(OnEventTriggered);

        /// <summary>
        /// Unsubscribes from the string event.
        /// </summary>
        protected override void UnsubscribeFromEvent() => gameEvent?.RemoveListener(OnEventTriggered);

        /// <summary>
        /// Compares string values. Only Equals and NotEquals are supported.
        /// </summary>
        protected override bool CompareValues(string eventValue, string target, ComparisonType comparison)
            => comparison == ComparisonType.Equals ? eventValue == target : eventValue != target;
    }
}
