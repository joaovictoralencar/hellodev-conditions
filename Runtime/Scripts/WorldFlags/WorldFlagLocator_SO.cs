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
    /// The WorldFlagManager registers itself with this locator on enable.
    /// </summary>
    /// <remarks>
    /// Usage:
    /// 1. Create a single WorldFlagLocator_SO asset in your project
    /// 2. Assign it to WorldFlagManager's "Locator" field
    /// 3. Reference the same asset in any SO that needs world flag access
    /// 4. Access functionality via locator.Manager.MethodName()
    /// </remarks>
    [CreateAssetMenu(fileName = "WorldFlagLocator", menuName = "HelloDev/Locators/World Flag Locator")]
    public class WorldFlagLocator_SO : LocatorBase_SO
    {
        #region Private Fields

        private WorldFlagManager _manager;

        #endregion

        #region LocatorBase_SO Implementation

        /// <inheritdoc/>
        public override bool IsAvailable => _manager != null;

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
        /// Called by WorldFlagManager.OnEnable().
        /// </summary>
        public void Register(WorldFlagManager manager)
        {
            if (manager == null) return;

            if (_manager != null && _manager != manager)
            {
                Logger.LogWarning(LogSystems.WorldFlags, $"Replacing existing manager. Old: {_manager.name}, New: {manager.name}");
            }

            _manager = manager;
            OnManagerRegistered?.Invoke();
        }

        /// <summary>
        /// Unregisters a WorldFlagManager from this locator.
        /// Called by WorldFlagManager.OnDisable().
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
        private string ManagerName => _manager != null ? _manager.name : "(none)";

        [ShowInInspector, ReadOnly]
        [PropertyOrder(102)]
        private int RegisteredFlagCount => _manager?.FlagCount ?? 0;
#endif

        #endregion
    }
}
