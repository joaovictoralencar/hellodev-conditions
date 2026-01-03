using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HelloDev.Conditions
{
    /// <summary>
    /// Combines multiple conditions using AND/OR logic.
    /// Implements IConditionEventDriven to support event-based evaluation.
    /// Supports multiple subscribers.
    /// </summary>
    /// <remarks>
    /// For AND: all conditions must be met.
    /// For OR: at least one condition must be met.
    /// Supports both passive and event-driven child conditions.
    /// </remarks>
    [CreateAssetMenu(fileName = "CompositeCondition", menuName = "HelloDev/Conditions/Composite Condition")]
    public class CompositeCondition_SO : Condition_SO, IConditionEventDriven
    {
        [SerializeField] private List<Condition_SO> _conditions = new List<Condition_SO>();
        [SerializeField] private CompositeOperator _operator = CompositeOperator.And;

        private readonly Dictionary<Condition_SO, bool> _conditionStates = new Dictionary<Condition_SO, bool>();

        /// <summary>
        /// Multicast delegate for all registered callbacks.
        /// </summary>
        private Action _onConditionMet;

        /// <summary>
        /// Number of active subscribers.
        /// </summary>
        private int _subscriberCount;

        /// <summary>
        /// Whether we're subscribed to child condition events.
        /// </summary>
        private bool _isSubscribedToChildren;

        /// <summary>
        /// Stores callbacks for child conditions so we can properly unsubscribe.
        /// </summary>
        private readonly List<(IConditionEventDriven Condition, Action Callback)> _childSubscriptions = new();

        /// <summary>
        /// The child conditions in this composite.
        /// </summary>
        public IReadOnlyList<Condition_SO> Conditions => _conditions;

        /// <summary>
        /// The logical operator used to combine conditions.
        /// </summary>
        public CompositeOperator Operator => _operator;

        /// <summary>
        /// Subscribes to all event-driven child conditions.
        /// Multiple subscribers can register callbacks.
        /// </summary>
        /// <param name="onConditionMet">Callback when the composite condition is met.</param>
        public void SubscribeToEvent(Action onConditionMet)
        {
            if (onConditionMet == null) return;

            // Add callback to multicast delegate
            _onConditionMet += onConditionMet;
            _subscriberCount++;

            // Subscribe to child conditions on first subscriber
            if (!_isSubscribedToChildren)
            {
                InitializeConditionStates();

                foreach (Condition_SO condition in _conditions)
                {
                    if (condition is IConditionEventDriven eventDriven)
                    {
                        // Create and store a closure to track which condition triggered
                        var capturedCondition = condition;
                        Action callback = () => OnChildConditionMet(capturedCondition);
                        eventDriven.SubscribeToEvent(callback);
                        _childSubscriptions.Add((eventDriven, callback));
                    }
                }

                _isSubscribedToChildren = true;
            }
        }

        /// <summary>
        /// Unsubscribes a specific callback from the composite condition.
        /// </summary>
        public void UnsubscribeFromEvent(Action callback)
        {
            if (callback == null) return;

            // Remove callback from multicast delegate
            _onConditionMet -= callback;
            _subscriberCount = Math.Max(0, _subscriberCount - 1);

            // Unsubscribe from child conditions when no subscribers remain
            if (_subscriberCount == 0 && _isSubscribedToChildren)
            {
                foreach (var (condition, childCallback) in _childSubscriptions)
                {
                    condition.UnsubscribeFromEvent(childCallback);
                }
                _childSubscriptions.Clear();
                _isSubscribedToChildren = false;
            }
        }

        /// <summary>
        /// Forces all child conditions to be fulfilled.
        /// </summary>
        public void ForceFulfillCondition()
        {
            foreach (Condition_SO condition in _conditions)
            {
                if (condition is IConditionEventDriven eventDriven)
                {
                    eventDriven.ForceFulfillCondition();
                }
            }
        }

        /// <summary>
        /// Initializes the state dictionary with current condition states.
        /// </summary>
        private void InitializeConditionStates()
        {
            _conditionStates.Clear();
            foreach (Condition_SO condition in _conditions)
            {
                if (condition != null)
                {
                    _conditionStates[condition] = condition.Evaluate();
                }
            }
        }

        /// <summary>
        /// Called when a child condition is met.
        /// </summary>
        private void OnChildConditionMet(Condition_SO condition)
        {
            if (condition != null)
            {
                _conditionStates[condition] = true;
            }

            if (Evaluate())
            {
                _onConditionMet?.Invoke();
            }
        }

        /// <summary>
        /// Evaluates the composite condition based on child states and operator.
        /// </summary>
        public override bool Evaluate()
        {
            if (_conditions == null || _conditions.Count == 0)
            {
                return !IsInverted;
            }

            // If not subscribed, evaluate conditions directly
            if (!_isSubscribedToChildren)
            {
                InitializeConditionStates();
            }

            bool finalResult = _operator switch
            {
                CompositeOperator.And => _conditionStates.Values.All(state => state),
                CompositeOperator.Or => _conditionStates.Values.Any(state => state),
                _ => false
            };

            return IsInverted ? !finalResult : finalResult;
        }

        /// <summary>
        /// Clears all subscriptions. Used during reset and destroy.
        /// </summary>
        private void ClearAllSubscriptions()
        {
            if (_isSubscribedToChildren)
            {
                foreach (var (condition, callback) in _childSubscriptions)
                {
                    condition.UnsubscribeFromEvent(callback);
                }
                _childSubscriptions.Clear();
                _isSubscribedToChildren = false;
            }

            _onConditionMet = null;
            _subscriberCount = 0;
            _conditionStates.Clear();
        }

        /// <summary>
        /// Called when entering play mode.
        /// </summary>
        protected override void OnScriptableObjectReset()
        {
            ClearAllSubscriptions();
        }

        private void OnDestroy()
        {
            ClearAllSubscriptions();
        }
    }
}
