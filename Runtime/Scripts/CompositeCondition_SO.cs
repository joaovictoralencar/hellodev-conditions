using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HelloDev.Conditions
{
    /// <summary>
    /// Combines multiple conditions using AND/OR logic.
    /// Implements IConditionEventDriven to support event-based evaluation.
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
        private Action _onConditionMet;
        private bool _isSubscribed;

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
        /// </summary>
        /// <param name="onConditionMet">Callback when the composite condition is met.</param>
        public void SubscribeToEvent(Action onConditionMet)
        {
            if (_isSubscribed) return;

            _onConditionMet = onConditionMet;
            InitializeConditionStates();

            foreach (Condition_SO condition in _conditions)
            {
                if (condition is IConditionEventDriven eventDriven)
                {
                    // Create a closure to track which condition triggered
                    var capturedCondition = condition;
                    eventDriven.SubscribeToEvent(() => OnChildConditionMet(capturedCondition));
                }
            }

            _isSubscribed = true;
        }

        /// <summary>
        /// Unsubscribes from all event-driven child conditions.
        /// </summary>
        public void UnsubscribeFromEvent()
        {
            if (!_isSubscribed) return;

            foreach (Condition_SO condition in _conditions)
            {
                if (condition is IConditionEventDriven eventDriven)
                {
                    eventDriven.UnsubscribeFromEvent();
                }
            }

            _isSubscribed = false;
            _onConditionMet = null;
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
            if (!_isSubscribed)
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
        /// Cleans up subscriptions and state.
        /// </summary>
        public void Cleanup()
        {
            UnsubscribeFromEvent();
            _conditionStates.Clear();
        }

        /// <summary>
        /// Called when entering play mode.
        /// </summary>
        protected override void OnScriptableObjectReset()
        {
            Cleanup();
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
