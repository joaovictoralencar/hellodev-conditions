using System;
using HelloDev.Events;
using HelloDev.Logging;
using UnityEngine;
using Logger = HelloDev.Logging.Logger;

namespace HelloDev.Conditions.Types
{
    /// <summary>
    /// Event-driven condition for parameterless void events.
    /// The condition is met immediately when the event is raised.
    /// </summary>
    /// <remarks>
    /// Use for events like OnGameStart, OnPause, OnLevelComplete, etc.
    /// where no value comparison is needed - the event firing IS the condition.
    /// </remarks>
    [CreateAssetMenu(fileName = "Void Condition", menuName = "HelloDev/Conditions/Void Condition")]
    public class ConditionVoid_SO : Condition_SO, IConditionEventDriven
    {
        [Header("Event Reference")]
        [SerializeField] private GameEventVoid_SO gameEvent;
        
        private System.Action _onConditionMet;
        private int _subscriberCount;
        private bool _isSubscribedToEvent;
        private bool _lastResult;
        private bool _hasBeenEvaluated;

        /// <summary>
        /// Returns the cached result from the last event evaluation.
        /// Returns false if no event has triggered yet.
        /// </summary>
        public override bool Evaluate() => _lastResult;

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
        public void SubscribeToEvent(System.Action onConditionMet)
        {
            if (onConditionMet == null) return;

            _onConditionMet += onConditionMet;
            _subscriberCount++;

            Logger.LogVerbose(LogSystems.Conditions, $"{name}: SubscribeToEvent - added callback (subscribers: {_subscriberCount})");

            if (!_isSubscribedToEvent)
            {
                _lastResult = false;
                _hasBeenEvaluated = false;

                if (gameEvent != null)
                {
                    gameEvent.AddListener(OnEventTriggered);
                }
                _isSubscribedToEvent = true;
                Logger.LogVerbose(LogSystems.Conditions, $"{name}: Subscribed to underlying GameEvent");
            }
        }

        /// <summary>
        /// Unsubscribes a specific callback from the underlying event.
        /// </summary>
        /// <param name="callback">The callback to remove.</param>
        public void UnsubscribeFromEvent(System.Action callback)
        {
            if (callback == null) return;

            _onConditionMet -= callback;
            _subscriberCount = System.Math.Max(0, _subscriberCount - 1);

            Logger.LogVerbose(LogSystems.Conditions, $"{name}: UnsubscribeFromEvent - removed callback (subscribers: {_subscriberCount})");

            if (_subscriberCount == 0 && _isSubscribedToEvent)
            {
                if (gameEvent != null)
                {
                    gameEvent.RemoveListener(OnEventTriggered);
                }
                _isSubscribedToEvent = false;
                Logger.LogVerbose(LogSystems.Conditions, $"{name}: Unsubscribed from underlying GameEvent");
            }
        }

        /// <summary>
        /// Forces the condition to be fulfilled (for debugging/testing).
        /// </summary>
        public void ForceFulfillCondition()
        {
            Logger.Log(LogSystems.Conditions, $"{name}: ForceFulfillCondition called");
            if (gameEvent != null)
            {
                gameEvent.Raise();
            }
        }

        /// <summary>
        /// Called when entering play mode. Resets subscription and cached state.
        /// </summary>
        protected override void OnScriptableObjectReset()
        {
            _onConditionMet = null;
            _subscriberCount = 0;

            if (_isSubscribedToEvent)
            {
                if (gameEvent != null)
                {
                    gameEvent.RemoveListener(OnEventTriggered);
                }
                _isSubscribedToEvent = false;
            }

            _lastResult = false;
            _hasBeenEvaluated = false;
        }

        private void OnEventTriggered()
        {
            // For void events, the condition is met when the event fires
            _lastResult = true;
            _hasBeenEvaluated = true;

            Logger.LogVerbose(LogSystems.Conditions, $"{name}: OnEventTriggered - subscribers={_subscriberCount}");

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

        private void OnDestroy()
        {
            _onConditionMet = null;
            _subscriberCount = 0;

            if (_isSubscribedToEvent)
            {
                if (gameEvent != null)
                {
                    gameEvent.RemoveListener(OnEventTriggered);
                }
                _isSubscribedToEvent = false;
            }
        }

        private void OnValidate()
        {
            IsInverted = false;
        }
    }
}
