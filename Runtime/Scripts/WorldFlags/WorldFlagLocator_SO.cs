using HelloDev.Logging;
using HelloDev.Utils;
using UnityEngine;
using UnityEngine.Events;
using Logger = HelloDev.Logging.Logger;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// ScriptableObject locator for WorldFlagManager.
    /// Acts as a decoupled access point that any asset can reference.
    /// The WorldFlagManagerBehaviour registers its manager with this locator on enable.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Usage:
    /// </para>
    /// <list type="number">
    /// <item><description>Create a single WorldFlagLocator_SO asset in your project</description></item>
    /// <item><description>Assign it to WorldFlagManagerBehaviour's "Locator" field</description></item>
    /// <item><description>Reference the same asset in any SO that needs world flag access</description></item>
    /// <item><description>Access functionality via locator.Manager.MethodName()</description></item>
    /// </list>
    /// <para>
    /// To modify flag values, use the typed getters and call methods on the runtime:
    /// </para>
    /// <code>
    /// locator.Manager.GetBoolFlag(flagData)?.SetValue(true);
    /// locator.Manager.GetIntFlag(flagData)?.Increment(10);
    /// </code>
    /// </remarks>
    [CreateAssetMenu(fileName = "WorldFlagLocator", menuName = "HelloDev/Locators/World Flag Locator")]
    public class WorldFlagLocator_SO : LocatorBase_SO
    {
        #region Private Fields

        private WorldFlagManager _manager;

        #endregion

        #region LocatorBase_SO Implementation

        /// <inheritdoc/>
        public override bool IsAvailable => _manager != null && _manager.IsInitialized;

        /// <inheritdoc/>
        public override void PrepareForBootstrap()
        {
            _manager = null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the registered manager instance.
        /// </summary>
        public WorldFlagManager Manager => _manager;

        #endregion

        #region Events

        /// <summary>
        /// Fired when a manager registers with this locator.
        /// </summary>
        [System.NonSerialized]
        public UnityEvent OnManagerRegistered = new();

        /// <summary>
        /// Fired when a manager unregisters from this locator.
        /// </summary>
        [System.NonSerialized]
        public UnityEvent OnManagerUnregistered = new();

        #endregion

        #region Registration

        /// <summary>
        /// Registers a WorldFlagManager with this locator.
        /// Called by WorldFlagManagerBehaviour.InitializeAsync().
        /// </summary>
        public void Register(WorldFlagManager manager)
        {
            if (manager == null) return;

            if (_manager != null && _manager != manager)
            {
                Logger.LogWarning(LogSystems.WorldFlags, "Replacing existing manager.");
            }

            _manager = manager;
            OnManagerRegistered?.Invoke();
        }

        /// <summary>
        /// Unregisters a WorldFlagManager from this locator.
        /// Called by WorldFlagManagerBehaviour.Shutdown().
        /// </summary>
        public void Unregister(WorldFlagManager manager)
        {
            if (_manager == manager)
            {
                _manager = null;
                OnManagerUnregistered?.Invoke();
            }
        }

        #endregion

        #region Debug

#if ODIN_INSPECTOR && UNITY_EDITOR
        [Title("Debug")]
        [ShowInInspector, ReadOnly]
        [PropertyOrder(100)]
        private bool ManagerRegistered => IsAvailable;

        [ShowInInspector, ReadOnly]
        [PropertyOrder(101)]
        private int RegisteredFlagCount => _manager?.FlagCount ?? 0;
#endif

        #endregion
    }
}
