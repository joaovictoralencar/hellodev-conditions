using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Immutable configuration for a boolean world flag.
    /// Use WorldFlagManager (via WorldFlagLocator_SO) to get the runtime instance for mutable state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Example uses:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Quest branch decisions ("spared_the_merchant", "joined_thieves_guild")</description></item>
    /// <item><description>Story milestones ("dragon_defeated", "met_king")</description></item>
    /// <item><description>Player choices ("chose_evil_path", "accepted_quest")</description></item>
    /// </list>
    /// <para>
    /// Debug operations for flags are centralized in WorldFlagRegistry_SO to avoid
    /// redundant locator references on each individual flag asset.
    /// </para>
    /// </remarks>
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
        /// Prefer using WorldFlagLocator_SO.Manager.GetBoolFlag() instead of calling this directly.
        /// </summary>
        /// <returns>A new runtime instance.</returns>
        public WorldFlagBoolRuntime CreateRuntime()
        {
            return new WorldFlagBoolRuntime(this);
        }

        #endregion
    }
}
