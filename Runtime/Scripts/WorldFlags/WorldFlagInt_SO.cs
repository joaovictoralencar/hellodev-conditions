using System;
using UnityEngine;
using UnityEngine.Events;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// A self-contained integer world state flag.
    /// Stores an integer value and fires events when changed.
    ///
    /// Example uses:
    /// - Reputation values ("merchant_reputation", "thieves_guild_standing")
    /// - Kill counts ("dragons_slain", "bandits_defeated")
    /// - Resource tracking ("village_population", "gold_donated")
    /// - Quest counters ("quests_completed", "artifacts_found")
    /// </summary>
    [CreateAssetMenu(fileName = "WorldFlagInt", menuName = "HelloDev/World State/Int Flag")]
    public class WorldFlagInt_SO : WorldFlagBase_SO
    {
        #region Serialized Fields

#if ODIN_INSPECTOR
        [BoxGroup("Value")]
        [PropertyOrder(10)]
#endif
        [SerializeField]
        [Tooltip("The default value when the flag is reset.")]
        private int defaultValue = 0;

#if ODIN_INSPECTOR
        [BoxGroup("Value")]
        [PropertyOrder(11)]
#endif
        [SerializeField]
        [Tooltip("Minimum allowed value (inclusive). Set to int.MinValue for no minimum.")]
        private int minValue = int.MinValue;

#if ODIN_INSPECTOR
        [BoxGroup("Value")]
        [PropertyOrder(12)]
#endif
        [SerializeField]
        [Tooltip("Maximum allowed value (inclusive). Set to int.MaxValue for no maximum.")]
        private int maxValue = int.MaxValue;

#if ODIN_INSPECTOR
        [BoxGroup("Value")]
        [PropertyOrder(13)]
        [ReadOnly]
        [ShowInInspector]
#endif
        private int _currentValue;

        #endregion

        #region Events

        /// <summary>
        /// Fired when the flag value changes. Parameters are (newValue, previousValue).
        /// </summary>
        [NonSerialized]
        public UnityEvent<int, int> OnValueChanged = new();

        /// <summary>
        /// Fired when the value is incremented.
        /// </summary>
        [NonSerialized]
        public UnityEvent<int> OnIncremented = new();

        /// <summary>
        /// Fired when the value is decremented.
        /// </summary>
        [NonSerialized]
        public UnityEvent<int> OnDecremented = new();

        /// <summary>
        /// Fired when the value reaches or exceeds a threshold (via SetValueWithThreshold).
        /// </summary>
        [NonSerialized]
        public UnityEvent<int> OnThresholdReached = new();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current value of the flag.
        /// </summary>
        public int Value => _currentValue;

        /// <summary>
        /// Gets the default value of the flag.
        /// </summary>
        public int DefaultValue => defaultValue;

        /// <summary>
        /// Gets the minimum allowed value.
        /// </summary>
        public int MinValue => minValue;

        /// <summary>
        /// Gets the maximum allowed value.
        /// </summary>
        public int MaxValue => maxValue;

        /// <summary>
        /// Gets whether the current value is at the minimum.
        /// </summary>
        public bool IsAtMin => _currentValue <= minValue;

        /// <summary>
        /// Gets whether the current value is at the maximum.
        /// </summary>
        public bool IsAtMax => _currentValue >= maxValue;

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the flag value, clamped to min/max bounds. Fires events if the value changed.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetValue(int value)
        {
            int clampedValue = Mathf.Clamp(value, minValue, maxValue);
            if (_currentValue == clampedValue) return;

            int previousValue = _currentValue;
            _currentValue = clampedValue;

            OnValueChanged?.Invoke(_currentValue, previousValue);
        }

        /// <summary>
        /// Increments the value by the specified amount.
        /// </summary>
        /// <param name="amount">Amount to add (default 1).</param>
        public void Increment(int amount = 1)
        {
            int previousValue = _currentValue;
            SetValue(_currentValue + amount);

            if (_currentValue != previousValue)
            {
                OnIncremented?.Invoke(_currentValue);
            }
        }

        /// <summary>
        /// Decrements the value by the specified amount.
        /// </summary>
        /// <param name="amount">Amount to subtract (default 1).</param>
        public void Decrement(int amount = 1)
        {
            int previousValue = _currentValue;
            SetValue(_currentValue - amount);

            if (_currentValue != previousValue)
            {
                OnDecremented?.Invoke(_currentValue);
            }
        }

        /// <summary>
        /// Sets the value and fires OnThresholdReached if it meets or exceeds the threshold.
        /// Useful for tracking progress toward goals.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="threshold">The threshold to check against.</param>
        public void SetValueWithThreshold(int value, int threshold)
        {
            bool wasBelowThreshold = _currentValue < threshold;
            SetValue(value);

            if (wasBelowThreshold && _currentValue >= threshold)
            {
                OnThresholdReached?.Invoke(_currentValue);
            }
        }

        /// <summary>
        /// Increments the value and fires OnThresholdReached if it meets or exceeds the threshold.
        /// </summary>
        /// <param name="threshold">The threshold to check against.</param>
        /// <param name="amount">Amount to add (default 1).</param>
        public void IncrementWithThreshold(int threshold, int amount = 1)
        {
            SetValueWithThreshold(_currentValue + amount, threshold);
        }

        /// <summary>
        /// Resets the flag to its default value.
        /// </summary>
        public override void ResetToDefault()
        {
            _currentValue = Mathf.Clamp(defaultValue, minValue, maxValue);
            // Don't fire events on reset - this is initialization
        }

        /// <summary>
        /// Gets a string representation of the current value.
        /// </summary>
        public override string GetValueAsString() => _currentValue.ToString();

        /// <summary>
        /// Checks if the current value meets a comparison against a target.
        /// </summary>
        /// <param name="target">The target value to compare against.</param>
        /// <param name="comparison">The comparison type.</param>
        public bool Compare(int target, ComparisonType comparison)
        {
            return comparison switch
            {
                ComparisonType.Equals => _currentValue == target,
                ComparisonType.NotEquals => _currentValue != target,
                ComparisonType.LessThan => _currentValue < target,
                ComparisonType.LessThanOrEqual => _currentValue <= target,
                ComparisonType.GreaterThan => _currentValue > target,
                ComparisonType.GreaterThanOrEqual => _currentValue >= target,
                _ => false
            };
        }

        #endregion

#if ODIN_INSPECTOR && UNITY_EDITOR
        #region Debug Buttons

        [BoxGroup("Debug")]
        [Button("Increment")]
        [PropertyOrder(100)]
        private void DebugIncrement() => Increment();

        [BoxGroup("Debug")]
        [Button("Decrement")]
        [PropertyOrder(101)]
        private void DebugDecrement() => Decrement();

        [BoxGroup("Debug")]
        [Button("Reset to Default")]
        [PropertyOrder(102)]
        private void DebugReset() => ResetToDefault();

        [BoxGroup("Debug")]
        [Button("Set to Max")]
        [PropertyOrder(103)]
        private void DebugSetMax() => SetValue(maxValue);

        [BoxGroup("Debug")]
        [Button("Set to Min")]
        [PropertyOrder(104)]
        private void DebugSetMin() => SetValue(minValue);

        #endregion
#endif
    }
}
