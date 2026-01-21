using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Immutable configuration for an integer world flag.
    /// Use WorldFlagManager (via WorldFlagLocator_SO) to get the runtime instance for mutable state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Example uses:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Reputation values ("merchant_reputation", "thieves_guild_standing")</description></item>
    /// <item><description>Kill counts ("dragons_slain", "bandits_defeated")</description></item>
    /// <item><description>Resource tracking ("village_population", "gold_donated")</description></item>
    /// <item><description>Quest counters ("quests_completed", "artifacts_found")</description></item>
    /// </list>
    /// <para>
    /// Debug operations for flags are centralized in WorldFlagRegistry_SO to avoid
    /// redundant locator references on each individual flag asset.
    /// </para>
    /// </remarks>
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
        /// Prefer using WorldFlagLocator_SO.Manager.GetIntFlag() instead of calling this directly.
        /// </summary>
        /// <returns>A new runtime instance.</returns>
        public WorldFlagIntRuntime CreateRuntime()
        {
            return new WorldFlagIntRuntime(this);
        }

        #endregion
    }
}
