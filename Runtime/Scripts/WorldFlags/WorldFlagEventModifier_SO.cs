using UnityEngine;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Generic base for typed event modifiers with optional value filtering.
    /// Listens to typed game events and applies modifications when the filter condition is met.
    /// </summary>
    /// <typeparam name="T">The type of value the event carries.</typeparam>
    public abstract class WorldFlagEventModifier_SO<T> : WorldFlagEventModifierBase_SO
    {
        [Header("Filter")]
        [SerializeField] protected bool filterByValue = true;
        [SerializeField] protected T targetValue;
        [SerializeField] protected ComparisonType comparisonType = ComparisonType.Equals;

        /// <summary>
        /// Override to subscribe to the specific GameEvent type.
        /// </summary>
        protected abstract void SubscribeToEvent();

        /// <summary>
        /// Override to unsubscribe from the specific GameEvent type.
        /// </summary>
        protected abstract void UnsubscribeFromEvent();

        /// <summary>
        /// Override to implement comparison logic for the specific type.
        /// </summary>
        protected abstract bool CompareValues(T eventValue, T target, ComparisonType comparison);

        /// <summary>
        /// Subscribes to the underlying game event.
        /// </summary>
        public override void Subscribe()
        {
            if (IsSubscribed) return;
            SubscribeToEvent();
            IsSubscribed = true;
        }

        /// <summary>
        /// Unsubscribes from the underlying game event.
        /// </summary>
        public override void Unsubscribe()
        {
            if (!IsSubscribed) return;
            UnsubscribeFromEvent();
            IsSubscribed = false;
        }

        /// <summary>
        /// Called when the subscribed event is triggered.
        /// Applies the modification if the filter condition is met.
        /// </summary>
        protected void OnEventTriggered(T eventValue)
        {
            if (!filterByValue || CompareValues(eventValue, targetValue, comparisonType))
                ApplyModification();
        }
    }
}
