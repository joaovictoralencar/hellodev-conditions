using System;
using UnityEngine.Events;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Runtime instance for a boolean world flag.
    /// Holds mutable state while WorldFlagBool_SO holds immutable config.
    /// </summary>
    public class WorldFlagBoolRuntime : WorldFlagRuntime
    {
        private bool _currentValue;

        /// <summary>
        /// Fired when the flag value changes. Parameter is the new value.
        /// </summary>
        public UnityEvent<bool> OnValueChanged { get; } = new();

        /// <summary>
        /// Fired when the flag becomes true.
        /// </summary>
        public UnityEvent OnBecameTrue { get; } = new();

        /// <summary>
        /// Fired when the flag becomes false.
        /// </summary>
        public UnityEvent OnBecameFalse { get; } = new();

        /// <summary>
        /// Gets the current value of the flag.
        /// </summary>
        public bool Value => _currentValue;

        /// <summary>
        /// Gets the typed data asset.
        /// </summary>
        public new WorldFlagBool_SO Data => (WorldFlagBool_SO)base.Data;

        public WorldFlagBoolRuntime(WorldFlagBool_SO data) : base(data)
        {
            _currentValue = data.DefaultValue;
        }

        /// <summary>
        /// Sets the flag value. Fires events if the value changed.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void SetValue(bool value)
        {
            if (_currentValue == value) return;

            bool previousValue = _currentValue;
            _currentValue = value;

            OnValueChanged?.Invoke(value);

            if (value && !previousValue)
            {
                OnBecameTrue?.Invoke();
            }
            else if (!value && previousValue)
            {
                OnBecameFalse?.Invoke();
            }
        }

        /// <summary>
        /// Sets the flag to true.
        /// </summary>
        public void SetTrue() => SetValue(true);

        /// <summary>
        /// Sets the flag to false.
        /// </summary>
        public void SetFalse() => SetValue(false);

        /// <summary>
        /// Toggles the flag value.
        /// </summary>
        public void Toggle() => SetValue(!_currentValue);

        /// <summary>
        /// Resets the flag to its default value.
        /// </summary>
        public override void ResetToDefault()
        {
            _currentValue = Data.DefaultValue;
            // Don't fire events on reset - this is initialization
        }

        /// <summary>
        /// Gets a string representation of the current value.
        /// </summary>
        public override string GetValueAsString() => _currentValue.ToString();
    }
}
