using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Immutable configuration for an integer world flag.
    /// Use WorldFlagManager (via WorldFlagLocator_SO) to get the runtime instance for mutable state.
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

#if ODIN_INSPECTOR && UNITY_EDITOR
        [BoxGroup("Debug Locator")]
        [PropertyOrder(99)]
        [Tooltip("Reference for debug buttons only. Not required for normal operation.")]
#endif
        [SerializeField]
        private WorldFlagLocator_SO debugLocator;

        #endregion

        #region Properties

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

        #endregion

        #region Factory Method

        /// <summary>
        /// Creates a new runtime instance for this flag.
        /// Prefer using WorldFlagLocator_SO.GetIntFlag() instead of calling this directly.
        /// </summary>
        /// <returns>A new runtime instance.</returns>
        public WorldFlagIntRuntime CreateRuntime()
        {
            return new WorldFlagIntRuntime(this);
        }

        #endregion

#if ODIN_INSPECTOR && UNITY_EDITOR
        #region Debug

        [BoxGroup("Debug")]
        [ShowInInspector, ReadOnly]
        [PropertyOrder(100)]
        private string RuntimeValue
        {
            get
            {
                if (!Application.isPlaying || debugLocator == null || !debugLocator.IsAvailable)
                    return "(Not in play mode)";

                var runtime = debugLocator.GetIntFlag(this);
                return runtime != null ? runtime.Value.ToString() : "(Not registered)";
            }
        }

        [BoxGroup("Debug")]
        [Button("Increment (Runtime)")]
        [PropertyOrder(101)]
        [EnableIf("@UnityEngine.Application.isPlaying && debugLocator != null")]
        private void DebugIncrement()
        {
            if (debugLocator != null && debugLocator.IsAvailable)
                debugLocator.IncrementIntValue(this);
        }

        [BoxGroup("Debug")]
        [Button("Decrement (Runtime)")]
        [PropertyOrder(102)]
        [EnableIf("@UnityEngine.Application.isPlaying && debugLocator != null")]
        private void DebugDecrement()
        {
            if (debugLocator != null && debugLocator.IsAvailable)
                debugLocator.DecrementIntValue(this);
        }

        [BoxGroup("Debug")]
        [Button("Reset to Default (Runtime)")]
        [PropertyOrder(103)]
        [EnableIf("@UnityEngine.Application.isPlaying && debugLocator != null")]
        private void DebugReset()
        {
            if (debugLocator != null && debugLocator.IsAvailable)
                debugLocator.ResetFlag(this);
        }

        [BoxGroup("Debug")]
        [Button("Set to Max (Runtime)")]
        [PropertyOrder(104)]
        [EnableIf("@UnityEngine.Application.isPlaying && debugLocator != null")]
        private void DebugSetMax()
        {
            if (debugLocator != null && debugLocator.IsAvailable)
                debugLocator.SetIntValue(this, maxValue);
        }

        [BoxGroup("Debug")]
        [Button("Set to Min (Runtime)")]
        [PropertyOrder(105)]
        [EnableIf("@UnityEngine.Application.isPlaying && debugLocator != null")]
        private void DebugSetMin()
        {
            if (debugLocator != null && debugLocator.IsAvailable)
                debugLocator.SetIntValue(this, minValue);
        }

        #endregion
#endif
    }
}
