using HelloDev.Conditions.WorldFlags;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.Types
{
    /// <summary>
    /// Condition that checks a WorldFlagInt_SO against a target value with comparison.
    /// Subscribes to the flag's runtime change events for reactive evaluation.
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
        [BoxGroup("Service")]
        [PropertyOrder(-1)]
        [Required]
#endif
        [SerializeField]
        [Tooltip("The WorldFlagService that provides access to flag runtime values.")]
        private WorldFlagService_SO flagService;

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
        private WorldFlagIntRuntime _cachedRuntime;

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

        /// <summary>
        /// Gets the runtime instance from the flag service.
        /// </summary>
        private WorldFlagIntRuntime Runtime
        {
            get
            {
                if (_cachedRuntime == null && worldFlag != null && flagService != null && flagService.IsAvailable)
                {
                    _cachedRuntime = flagService.GetIntFlag(worldFlag);
                }
                return _cachedRuntime;
            }
        }

        #endregion

        #region Condition Implementation

        /// <summary>
        /// Evaluates the condition by comparing the world flag's current value.
        /// </summary>
        public override bool Evaluate()
        {
            if (worldFlag == null) return false;

            var runtime = Runtime;
            if (runtime == null)
            {
                // Fallback to default value if runtime not available
                bool defaultResult = Compare(worldFlag.DefaultValue, targetValue, comparisonType);
                return IsInverted ? !defaultResult : defaultResult;
            }

            bool result = runtime.Compare(targetValue, comparisonType);
            return IsInverted ? !result : result;
        }

        private static bool Compare(int value, int target, ComparisonType comparison)
        {
            return comparison switch
            {
                ComparisonType.Equals => value == target,
                ComparisonType.NotEquals => value != target,
                ComparisonType.LessThan => value < target,
                ComparisonType.LessThanOrEqual => value <= target,
                ComparisonType.GreaterThan => value > target,
                ComparisonType.GreaterThanOrEqual => value >= target,
                _ => false
            };
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

            var runtime = Runtime;
            if (runtime == null)
            {
                Debug.LogWarning($"[{name}] Cannot subscribe - flagService not available or flag not registered.");
                return;
            }

            _onConditionMet = onConditionMet;
            runtime.OnValueChanged.AddListener(HandleValueChanged);
            _isSubscribed = true;
        }

        /// <summary>
        /// Unsubscribes from the world flag's value changed event.
        /// </summary>
        public void UnsubscribeFromEvent()
        {
            if (!_isSubscribed) return;

            var runtime = Runtime;
            if (runtime != null)
            {
                runtime.OnValueChanged.RemoveListener(HandleValueChanged);
            }

            _isSubscribed = false;
            _onConditionMet = null;
            _cachedRuntime = null;
        }

        /// <summary>
        /// Forces the condition to be fulfilled by setting the world flag.
        /// Sets the flag to the target value (for == conditions) or an appropriate value for other comparisons.
        /// </summary>
        public void ForceFulfillCondition()
        {
            if (worldFlag == null) return;
            if (flagService == null || !flagService.IsAvailable) return;

            int valueToSet = comparisonType switch
            {
                ComparisonType.Equals => targetValue,
                ComparisonType.LessThanOrEqual => targetValue,
                ComparisonType.GreaterThanOrEqual => targetValue,
                ComparisonType.NotEquals => targetValue + 1,
                ComparisonType.LessThan => targetValue - 1,
                ComparisonType.GreaterThan => targetValue + 1,
                _ => targetValue
            };

            flagService.SetIntValue(worldFlag, valueToSet);
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
        private string CurrentFlagValue
        {
            get
            {
                if (worldFlag == null) return "(No flag)";
                if (!Application.isPlaying || flagService == null || !flagService.IsAvailable)
                    return $"(Default: {worldFlag.DefaultValue})";

                var runtime = flagService.GetIntFlag(worldFlag);
                return runtime != null ? runtime.Value.ToString() : "(Not registered)";
            }
        }

        [BoxGroup("Debug")]
        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(101)]
        private string ComparisonDescription
        {
            get
            {
                if (worldFlag == null) return "No flag assigned";
                string currentVal = CurrentFlagValue;
                return $"{currentVal} {GetComparisonSymbol()} {targetValue}";
            }
        }

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
        [EnableIf("@UnityEngine.Application.isPlaying")]
        private void DebugForceFulfill() => ForceFulfillCondition();

        #endregion
#endif
    }
}
