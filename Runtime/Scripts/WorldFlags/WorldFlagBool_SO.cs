using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Immutable configuration for a boolean world flag.
    /// Use WorldFlagManager (via WorldFlagService_SO) to get the runtime instance for mutable state.
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

#if ODIN_INSPECTOR && UNITY_EDITOR
        [BoxGroup("Debug Service")]
        [PropertyOrder(99)]
        [Tooltip("Reference for debug buttons only. Not required for normal operation.")]
#endif
        [SerializeField]
        private WorldFlagService_SO debugService;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default value of the flag.
        /// </summary>
        public bool DefaultValue => defaultValue;

        #endregion

        #region Factory Method

        /// <summary>
        /// Creates a new runtime instance for this flag.
        /// Prefer using WorldFlagService_SO.GetBoolFlag() instead of calling this directly.
        /// </summary>
        /// <returns>A new runtime instance.</returns>
        public WorldFlagBoolRuntime CreateRuntime()
        {
            return new WorldFlagBoolRuntime(this);
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
                if (!Application.isPlaying || debugService == null || !debugService.IsAvailable)
                    return "(Not in play mode)";

                var runtime = debugService.GetBoolFlag(this);
                return runtime != null ? runtime.Value.ToString() : "(Not registered)";
            }
        }

        [BoxGroup("Debug")]
        [Button("Set True (Runtime)")]
        [PropertyOrder(101)]
        [EnableIf("@UnityEngine.Application.isPlaying && debugService != null")]
        private void DebugSetTrue()
        {
            if (debugService != null && debugService.IsAvailable)
                debugService.SetBoolValue(this, true);
        }

        [BoxGroup("Debug")]
        [Button("Set False (Runtime)")]
        [PropertyOrder(102)]
        [EnableIf("@UnityEngine.Application.isPlaying && debugService != null")]
        private void DebugSetFalse()
        {
            if (debugService != null && debugService.IsAvailable)
                debugService.SetBoolValue(this, false);
        }

        [BoxGroup("Debug")]
        [Button("Reset to Default (Runtime)")]
        [PropertyOrder(103)]
        [EnableIf("@UnityEngine.Application.isPlaying && debugService != null")]
        private void DebugReset()
        {
            if (debugService != null && debugService.IsAvailable)
                debugService.ResetFlag(this);
        }

        #endregion
#endif
    }
}
