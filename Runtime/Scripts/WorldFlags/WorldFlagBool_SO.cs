using System;
using UnityEngine;
using UnityEngine.Events;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// A self-contained boolean world state flag.
    /// Stores a true/false value and fires events when changed.
    ///
    /// Example uses:
    /// - Quest branch decisions ("spared_the_merchant", "joined_thieves_guild")
    /// - Story milestones ("dragon_defeated", "met_king")
    /// - Player choices ("chose_evil_path", "accepted_quest")
    /// </summary>
    [CreateAssetMenu(fileName = "WorldFlagBool", menuName = "HelloDev/World State/Bool Flag")]
    public class WorldFlagBool_SO : WorldFlagBase_SO
    {
        #region Serialized Fields

#if ODIN_INSPECTOR
        [BoxGroup("Value")]
        [PropertyOrder(10)]
#endif
        [SerializeField]
        [Tooltip("The default value when the flag is reset.")]
        private bool defaultValue = false;

#if ODIN_INSPECTOR
        [BoxGroup("Value")]
        [PropertyOrder(11)]
        [ReadOnly]
        [ShowInInspector]
#endif
        private bool _currentValue;

        #endregion

        #region Events

        /// <summary>
        /// Fired when the flag value changes. Parameter is the new value.
        /// </summary>
        [NonSerialized]
        public UnityEvent<bool> OnValueChanged = new();

        /// <summary>
        /// Fired when the flag becomes true.
        /// </summary>
        [NonSerialized]
        public UnityEvent OnBecameTrue = new();

        /// <summary>
        /// Fired when the flag becomes false.
        /// </summary>
        [NonSerialized]
        public UnityEvent OnBecameFalse = new();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current value of the flag.
        /// </summary>
        public bool Value => _currentValue;

        /// <summary>
        /// Gets the default value of the flag.
        /// </summary>
        public bool DefaultValue => defaultValue;

        #endregion

        #region Public Methods

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
            _currentValue = defaultValue;
            // Don't fire events on reset - this is initialization
        }

        /// <summary>
        /// Gets a string representation of the current value.
        /// </summary>
        public override string GetValueAsString() => _currentValue.ToString();

        #endregion

#if ODIN_INSPECTOR && UNITY_EDITOR
        #region Debug Buttons

        [BoxGroup("Debug")]
        [Button("Set True")]
        [PropertyOrder(100)]
        private void DebugSetTrue() => SetTrue();

        [BoxGroup("Debug")]
        [Button("Set False")]
        [PropertyOrder(101)]
        private void DebugSetFalse() => SetFalse();

        [BoxGroup("Debug")]
        [Button("Toggle")]
        [PropertyOrder(102)]
        private void DebugToggle() => Toggle();

        [BoxGroup("Debug")]
        [Button("Reset to Default")]
        [PropertyOrder(103)]
        private void DebugReset() => ResetToDefault();

        #endregion
#endif
    }
}
