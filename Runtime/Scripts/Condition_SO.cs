using HelloDev.Utils;
using UnityEngine;

namespace HelloDev.Conditions
{
    /// <summary>
    /// Abstract ScriptableObject base class for passive conditions.
    /// Inherit from this for conditions that are polled/evaluated on demand.
    /// </summary>
    public abstract class Condition_SO : RuntimeScriptableObject, ICondition
    {
        [SerializeField] private bool _isInverted;

        /// <summary>
        /// When true, the evaluation result is inverted (NOT logic).
        /// </summary>
        public bool IsInverted
        {
            get => _isInverted;
            set => _isInverted = value;
        }

        /// <summary>
        /// Evaluates the condition. Override in derived classes.
        /// </summary>
        public abstract bool Evaluate();

        /// <summary>
        /// Called when entering play mode. Base conditions have no runtime state to reset.
        /// </summary>
        protected override void OnScriptableObjectReset()
        {
            // Base conditions have no runtime state to reset
        }
    }

    /// <summary>
    /// Generic base class for event-driven conditions that react to GameEvents.
    /// </summary>
    /// <typeparam name="T">The type of value the condition compares against.</typeparam>
    /// <remarks>
    /// Inherit from this and implement SubscribeToSpecificEvent/UnsubscribeFromSpecificEvent
    /// to connect to your specific GameEvent type.
    /// </remarks>
    public abstract class ConditionEventDriven_SO<T> : Condition_SO, IConditionEventDriven
    {
        [SerializeField] protected T targetValue;
        [SerializeField] protected ComparisonType _comparisonType = ComparisonType.Equals;

        private System.Action _onConditionMet;
        private bool _isSubscribed;
        private bool _lastResult;
        private bool _hasBeenEvaluated;

        /// <summary>
        /// Returns the cached result from the last event evaluation.
        /// Returns false if no event has triggered yet.
        /// </summary>
        public override bool Evaluate()
        {
            return _lastResult;
        }

        /// <summary>
        /// True if the condition has been evaluated at least once.
        /// </summary>
        public bool HasBeenEvaluated => _hasBeenEvaluated;

        /// <summary>
        /// Subscribes to the underlying event.
        /// </summary>
        /// <param name="onConditionMet">Callback invoked when condition is met.</param>
        public virtual void SubscribeToEvent(System.Action onConditionMet)
        {
            if (_isSubscribed) return;

            _onConditionMet = onConditionMet;
            SubscribeToSpecificEvent();
            _isSubscribed = true;
        }

        /// <summary>
        /// Unsubscribes from the underlying event.
        /// </summary>
        public virtual void UnsubscribeFromEvent()
        {
            if (!_isSubscribed) return;

            UnsubscribeFromSpecificEvent();
            _isSubscribed = false;
            _onConditionMet = null;
        }

        /// <summary>
        /// Forces the condition to be fulfilled (for debugging/testing).
        /// </summary>
        public void ForceFulfillCondition()
        {
            DebugForceFulfillCondition();
        }

        /// <summary>
        /// Override to implement debug force-fulfill logic.
        /// </summary>
        protected abstract void DebugForceFulfillCondition();

        /// <summary>
        /// Called when entering play mode. Resets subscription and cached state.
        /// </summary>
        protected override void OnScriptableObjectReset()
        {
            UnsubscribeFromEvent();
            _lastResult = false;
            _hasBeenEvaluated = false;
        }

        /// <summary>
        /// Override to subscribe to your specific GameEvent.
        /// </summary>
        protected abstract void SubscribeToSpecificEvent();

        /// <summary>
        /// Override to unsubscribe from your specific GameEvent.
        /// </summary>
        protected abstract void UnsubscribeFromSpecificEvent();

        /// <summary>
        /// Called when the subscribed event is triggered.
        /// </summary>
        /// <param name="eventParameter">The value from the event.</param>
        protected void OnEventTriggered(T eventParameter)
        {
            bool result = EvaluateCondition(eventParameter);
            _lastResult = result;
            _hasBeenEvaluated = true;

            if (result)
            {
                _onConditionMet?.Invoke();
            }
        }

        /// <summary>
        /// Evaluates the condition against the event parameter.
        /// </summary>
        protected virtual bool EvaluateCondition(T eventParameter)
        {
            bool result = CompareValues(eventParameter, targetValue, _comparisonType);
            return IsInverted ? !result : result;
        }

        /// <summary>
        /// Override to implement comparison logic for your type.
        /// </summary>
        protected abstract bool CompareValues(T eventValue, T targetValue, ComparisonType comparisonType);

        /// <summary>
        /// Cleanup on destroy.
        /// </summary>
        protected virtual void OnDestroy()
        {
            UnsubscribeFromEvent();
        }
    }
}
