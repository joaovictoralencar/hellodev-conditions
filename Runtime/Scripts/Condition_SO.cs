using HelloDev.Logging;
using HelloDev.Utils;
using UnityEngine;
using Logger = HelloDev.Logging.Logger;

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
    /// Supports multiple subscribers - each task/system can register its own callback.
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

        /// <summary>
        /// Multicast delegate for all registered callbacks.
        /// Uses event-like pattern to support multiple subscribers.
        /// </summary>
        private System.Action _onConditionMet;

        /// <summary>
        /// Number of active subscribers. Used to manage underlying event subscription.
        /// </summary>
        private int _subscriberCount;

        /// <summary>
        /// Whether we're subscribed to the underlying GameEvent.
        /// </summary>
        private bool _isSubscribedToEvent;

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
        /// Gets the number of active subscribers.
        /// </summary>
        public int SubscriberCount => _subscriberCount;

        /// <summary>
        /// Subscribes to the underlying event. Multiple subscribers can register callbacks.
        /// </summary>
        /// <param name="onConditionMet">Callback invoked when condition is met.</param>
        public virtual void SubscribeToEvent(System.Action onConditionMet)
        {
            if (onConditionMet == null) return;

            // Add callback to multicast delegate
            _onConditionMet += onConditionMet;
            _subscriberCount++;

            Logger.LogVerbose(LogSystems.Conditions, $"{name}: SubscribeToEvent - added callback (subscribers: {_subscriberCount})");

            // Subscribe to underlying event on first subscriber
            if (!_isSubscribedToEvent)
            {
                // Reset cached result when first subscriber joins - ensures condition
                // only reports True for events that happen AFTER subscription
                _lastResult = false;
                _hasBeenEvaluated = false;

                SubscribeToSpecificEvent();
                _isSubscribedToEvent = true;
                Logger.LogVerbose(LogSystems.Conditions, $"{name}: Subscribed to underlying GameEvent");
            }
        }

        /// <summary>
        /// Unsubscribes a specific callback from the underlying event.
        /// </summary>
        /// <param name="callback">The callback to remove.</param>
        public virtual void UnsubscribeFromEvent(System.Action callback)
        {
            if (callback == null) return;

            // Remove callback from multicast delegate
            _onConditionMet -= callback;
            _subscriberCount = System.Math.Max(0, _subscriberCount - 1);

            Logger.LogVerbose(LogSystems.Conditions, $"{name}: UnsubscribeFromEvent - removed callback (subscribers: {_subscriberCount})");

            // Unsubscribe from underlying event when no subscribers remain
            if (_subscriberCount == 0 && _isSubscribedToEvent)
            {
                UnsubscribeFromSpecificEvent();
                _isSubscribedToEvent = false;
                Logger.LogVerbose(LogSystems.Conditions, $"{name}: Unsubscribed from underlying GameEvent");
            }
        }

        /// <summary>
        /// Forces the condition to be fulfilled (for debugging/testing).
        /// </summary>
        public void ForceFulfillCondition()
        {
            Logger.Log(LogSystems.Conditions, $"{name}: ForceFulfillCondition called (subscribers={_subscriberCount}, isSubscribedToEvent={_isSubscribedToEvent})");
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
            // Clear all callbacks
            _onConditionMet = null;
            _subscriberCount = 0;

            // Unsubscribe from underlying event if subscribed
            if (_isSubscribedToEvent)
            {
                UnsubscribeFromSpecificEvent();
                _isSubscribedToEvent = false;
            }

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

            Logger.LogVerbose(LogSystems.Conditions, $"{name}: OnEventTriggered - result={result}, subscribers={_subscriberCount}");

            if (result)
            {
                if (_onConditionMet != null)
                {
                    Logger.Log(LogSystems.Conditions, $"{name}: Invoking {_subscriberCount} callback(s)");
                    _onConditionMet.Invoke();
                }
                else
                {
                    Logger.LogWarning(LogSystems.Conditions, $"{name}: Condition met but NO CALLBACKS to invoke!");
                }
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
            _onConditionMet = null;
            _subscriberCount = 0;

            if (_isSubscribedToEvent)
            {
                UnsubscribeFromSpecificEvent();
                _isSubscribedToEvent = false;
            }
        }
    }
}
