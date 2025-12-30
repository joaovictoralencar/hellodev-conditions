using System;
using UnityEngine;
using UnityEngine.Events;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Runtime instance for an integer world flag.
    /// Holds mutable state while WorldFlagInt_SO holds immutable config.
    /// </summary>
    public class WorldFlagIntRuntime : WorldFlagRuntime
    {
        private int _currentValue;

        /// <summary>
        /// Fired when the flag value changes. Parameters are (newValue, previousValue).
        /// </summary>
        public UnityEvent<int, int> OnValueChanged { get; } = new();

        /// <summary>
        /// Fired when the value is incremented.
        /// </summary>
        public UnityEvent<int> OnIncremented { get; } = new();

        /// <summary>
        /// Fired when the value is decremented.
        /// </summary>
        public UnityEvent<int> OnDecremented { get; } = new();

        /// <summary>
        /// Fired when the value reaches or exceeds a threshold (via SetValueWithThreshold).
        /// </summary>
        public UnityEvent<int> OnThresholdReached { get; } = new();

        /// <summary>
        /// Gets the current value of the flag.
        /// </summary>
        public int Value => _currentValue;

        /// <summary>
        /// Gets whether the current value is at the minimum.
        /// </summary>
        public bool IsAtMin => _currentValue <= Data.MinValue;

        /// <summary>
        /// Gets whether the current value is at the maximum.
        /// </summary>
        public bool IsAtMax => _currentValue >= Data.MaxValue;

        /// <summary>
        /// Gets the typed data asset.
        /// </summary>
        public new WorldFlagInt_SO Data => (WorldFlagInt_SO)base.Data;

        public WorldFlagIntRuntime(WorldFlagInt_SO data) : base(data)
        {
            _currentValue = Mathf.Clamp(data.DefaultValue, data.MinValue, data.MaxValue);
        }

        /// <summary>
        /// Sets the flag value, clamped to min/max bounds. Fires events if the value changed.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetValue(int value)
        {
            int clampedValue = Mathf.Clamp(value, Data.MinValue, Data.MaxValue);
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
            _currentValue = Mathf.Clamp(Data.DefaultValue, Data.MinValue, Data.MaxValue);
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
    }
}
