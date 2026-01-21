using System.Collections.Generic;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Interface for the world flag management system.
    /// Provides a minimal, type-agnostic API for flag registration and access.
    /// Type-specific operations (SetValue, Increment, etc.) are on the runtime classes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Following Tip #1 (Start with Interfaces): This interface enables:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Easier testing with mock implementations</description></item>
    /// <item><description>Decoupled consumers that don't depend on concrete types</description></item>
    /// <item><description>Swappable implementations for different contexts</description></item>
    /// </list>
    /// <para>
    /// Type-specific operations are accessed via typed getters:
    /// </para>
    /// <code>
    /// // Get typed runtime, then call type-specific methods
    /// manager.GetBoolFlag(flagData)?.SetValue(true);
    /// manager.GetIntFlag(flagData)?.Increment(10);
    /// </code>
    /// </remarks>
    public interface IWorldFlagManager
    {
        #region State

        /// <summary>
        /// Whether the manager has completed initialization.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the count of registered flags.
        /// </summary>
        int FlagCount { get; }

        /// <summary>
        /// Gets all registered runtime flags.
        /// </summary>
        IReadOnlyCollection<WorldFlagRuntime> AllFlags { get; }

        #endregion

        #region Generic Flag Access

        /// <summary>
        /// Gets a flag runtime by its data asset.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <returns>The runtime instance, or null if not registered.</returns>
        WorldFlagRuntime GetFlag(WorldFlagBase_SO flagData);

        /// <summary>
        /// Gets a flag runtime by its ID.
        /// </summary>
        /// <param name="flagId">The flag GUID.</param>
        /// <returns>The runtime instance, or null if not registered.</returns>
        WorldFlagRuntime GetFlagById(string flagId);

        /// <summary>
        /// Registers a flag data asset and creates its runtime instance.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <returns>The created runtime instance, or existing instance if already registered.</returns>
        WorldFlagRuntime RegisterFlag(WorldFlagBase_SO flagData);

        #endregion

        #region Typed Flag Access

        /// <summary>
        /// Gets a bool flag runtime by its data asset.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <returns>The runtime instance, or null if not registered or wrong type.</returns>
        WorldFlagBoolRuntime GetBoolFlag(WorldFlagBool_SO flagData);

        /// <summary>
        /// Gets an int flag runtime by its data asset.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <returns>The runtime instance, or null if not registered or wrong type.</returns>
        WorldFlagIntRuntime GetIntFlag(WorldFlagInt_SO flagData);

        #endregion

        #region Lifecycle

        /// <summary>
        /// Resets all flags to their default values.
        /// </summary>
        void ResetAllFlags();

        /// <summary>
        /// Resets a specific flag to its default value.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <returns>True if the flag was found and reset.</returns>
        bool ResetFlag(WorldFlagBase_SO flagData);

        #endregion
    }
}
