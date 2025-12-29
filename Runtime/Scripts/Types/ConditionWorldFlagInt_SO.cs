using HelloDev.Conditions.WorldFlags;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.Types
{
    /// <summary>
    /// Condition that checks a WorldFlagInt_SO against a target value with comparison.
    /// Subscribes to the flag's change events for reactive evaluation.
    ///
    /// Example usage:
    /// - Check if reputation >= 50: WorldFlag = "merchant_reputation", Target = 50, Comparison = GreaterThanOrEqual
    /// - Check if kills < 10: WorldFlag = "bandits_killed", Target = 10, Comparison = LessThan
    /// </summary>
    [CreateAssetMenu(fileName = "ConditionWorldFlagInt", menuName = "HelloDev/Conditions/World Flag Int")]
    public class ConditionWorldFlagInt_SO : Condition_SO, IConditionEventDriven
    {
        #region Serialized Fields

#if ODIN_INSPECTOR
        [BoxGroup("World Flag")]
        [PropertyOrder(0)]
        [Required]
#endif
        [SerializeField]
        [Tooltip("The world flag to check.")]
        private WorldFlagInt_SO worldFlag;

#if ODIN_INSPECTOR
        [BoxGroup("World Flag")]
        [PropertyOrder(1)]
#endif
        [SerializeField]
        [Tooltip("The target value to compare against.")]
        private int targetValue;

#if ODIN_INSPECTOR
        [BoxGroup("World Flag")]
        [PropertyOrder(2)]
#endif
        [SerializeField]
        [Tooltip("How to compare the flag value to the target.")]
        private ComparisonType comparisonType = ComparisonType.GreaterThanOrEqual;

        #endregion

        #region Private Fields

        private System.Action _onConditionMet;
        private bool _isSubscribed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the world flag being checked.
        /// </summary>
        public WorldFlagInt_SO WorldFlag => worldFlag;

        /// <summary>
        /// Gets the target value.
        /// </summary>
        public int TargetValue => targetValue;

        /// <summary>
        /// Gets the comparison type.
        /// </summary>
        public ComparisonType ComparisonType => comparisonType;

        #endregion

        #region Condition Implementation

        /// <summary>
        /// Evaluates the condition by comparing the world flag's current value.
        /// </summary>
        public override bool Evaluate()
        {
            if (worldFlag == null) return false;

            bool result = worldFlag.Compare(targetValue, comparisonType);
            return IsInverted ? !result : result;
        }

        #endregion

        #region IConditionEventDriven Implementation

        /// <summary>
        /// Subscribes to the world flag's value changed event.
        /// </summary>
        public void SubscribeToEvent(System.Action onConditionMet)
        {
            if (_isSubscribed) return;
            if (worldFlag == null) return;

            _onConditionMet = onConditionMet;
            worldFlag.OnValueChanged.AddListener(HandleValueChanged);
            _isSubscribed = true;
        }

        /// <summary>
        /// Unsubscribes from the world flag's value changed event.
        /// </summary>
        public void UnsubscribeFromEvent()
        {
            if (!_isSubscribed) return;
            if (worldFlag == null) return;

            worldFlag.OnValueChanged.RemoveListener(HandleValueChanged);
            _isSubscribed = false;
            _onConditionMet = null;
        }

        /// <summary>
        /// Forces the condition to be fulfilled by setting the world flag.
        /// Sets the flag to the target value (for == conditions) or an appropriate value for other comparisons.
        /// </summary>
        public void ForceFulfillCondition()
        {
            if (worldFlag == null) return;

            switch (comparisonType)
            {
                case ComparisonType.Equals:
                case ComparisonType.LessThanOrEqual:
                case ComparisonType.GreaterThanOrEqual:
                    worldFlag.SetValue(targetValue);
                    break;
                case ComparisonType.NotEquals:
                    worldFlag.SetValue(targetValue + 1);
                    break;
                case ComparisonType.LessThan:
                    worldFlag.SetValue(targetValue - 1);
                    break;
                case ComparisonType.GreaterThan:
                    worldFlag.SetValue(targetValue + 1);
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void HandleValueChanged(int newValue, int previousValue)
        {
            if (Evaluate())
            {
                _onConditionMet?.Invoke();
            }
        }

        #endregion

        #region Lifecycle

        protected override void OnScriptableObjectReset()
        {
            UnsubscribeFromEvent();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvent();
        }

        #endregion

#if ODIN_INSPECTOR && UNITY_EDITOR
        #region Debug

        [BoxGroup("Debug")]
        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(100)]
        private int CurrentFlagValue => worldFlag != null ? worldFlag.Value : 0;

        [BoxGroup("Debug")]
        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(101)]
        private string ComparisonDescription => worldFlag != null
            ? $"{worldFlag.Value} {GetComparisonSymbol()} {targetValue}"
            : "No flag assigned";

        [BoxGroup("Debug")]
        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(102)]
        private bool EvaluationResult => Evaluate();

        private string GetComparisonSymbol()
        {
            return comparisonType switch
            {
                ComparisonType.Equals => "==",
                ComparisonType.NotEquals => "!=",
                ComparisonType.LessThan => "<",
                ComparisonType.LessThanOrEqual => "<=",
                ComparisonType.GreaterThan => ">",
                ComparisonType.GreaterThanOrEqual => ">=",
                _ => "?"
            };
        }

        [BoxGroup("Debug")]
        [Button("Evaluate Now")]
        [PropertyOrder(103)]
        private void DebugEvaluate()
        {
            Debug.Log($"[{name}] {ComparisonDescription} = {Evaluate()}");
        }

        [BoxGroup("Debug")]
        [Button("Force Fulfill")]
        [PropertyOrder(104)]
        private void DebugForceFulfill() => ForceFulfillCondition();

        #endregion
#endif
    }
}
