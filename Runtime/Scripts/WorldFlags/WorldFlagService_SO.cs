using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// ScriptableObject service locator for WorldFlagManager.
    /// Acts as a "channel" that any asset can reference to access world flags.
    /// The WorldFlagManager registers itself with this service on enable.
    ///
    /// Usage:
    /// 1. Create a single WorldFlagService_SO asset in your project
    /// 2. Assign it to WorldFlagManager's "Service" field
    /// 3. Reference the same asset in any SO that needs world flag access
    /// </summary>
    [CreateAssetMenu(fileName = "WorldFlagService", menuName = "HelloDev/Services/World Flag Service")]
    public class WorldFlagService_SO : ScriptableObject
    {
        #region Private Fields

        private WorldFlagManager _manager;

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if a WorldFlagManager is currently registered.
        /// </summary>
        public bool IsAvailable => _manager != null;

        /// <summary>
        /// Gets the count of registered flags.
        /// </summary>
        public int FlagCount => _manager?.FlagCount ?? 0;

        /// <summary>
        /// Gets all registered runtime flags.
        /// </summary>
        public IReadOnlyCollection<WorldFlagRuntime> AllFlags =>
            _manager?.AllFlags ?? (IReadOnlyCollection<WorldFlagRuntime>)System.Array.Empty<WorldFlagRuntime>();

        #endregion

        #region Events

        /// <summary>
        /// Fired when a manager registers with this service.
        /// </summary>
        [System.NonSerialized]
        public UnityEvent OnManagerRegistered = new();

        /// <summary>
        /// Fired when a manager unregisters from this service.
        /// </summary>
        [System.NonSerialized]
        public UnityEvent OnManagerUnregistered = new();

        #endregion

        #region Registration

        /// <summary>
        /// Registers a WorldFlagManager with this service.
        /// Called by WorldFlagManager.OnEnable().
        /// </summary>
        public void Register(WorldFlagManager manager)
        {
            if (manager == null) return;

            if (_manager != null && _manager != manager)
            {
                Debug.LogWarning($"[WorldFlagService] Replacing existing manager. Old: {_manager.name}, New: {manager.name}");
            }

            _manager = manager;
            OnManagerRegistered?.Invoke();
        }

        /// <summary>
        /// Unregisters a WorldFlagManager from this service.
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

        #region Flag Registration

        /// <summary>
        /// Registers a flag data asset and creates its runtime instance.
        /// </summary>
        public WorldFlagRuntime RegisterFlag(WorldFlagBase_SO flagData)
        {
            return _manager?.RegisterFlag(flagData);
        }

        /// <summary>
        /// Sets the flag registry and registers all contained flags.
        /// </summary>
        public void SetFlagRegistry(WorldFlagRegistry_SO registry)
        {
            _manager?.SetFlagRegistry(registry);
        }

        #endregion

        #region Flag Access

        /// <summary>
        /// Gets a flag runtime by its data asset.
        /// </summary>
        public WorldFlagRuntime GetFlag(WorldFlagBase_SO flagData)
        {
            return _manager?.GetFlag(flagData);
        }

        /// <summary>
        /// Gets a flag runtime by its ID.
        /// </summary>
        public WorldFlagRuntime GetFlagById(string flagId)
        {
            return _manager?.GetFlagById(flagId);
        }

        /// <summary>
        /// Gets a bool flag runtime by its data asset.
        /// </summary>
        public WorldFlagBoolRuntime GetBoolFlag(WorldFlagBool_SO flagData)
        {
            return _manager?.GetBoolFlag(flagData);
        }

        /// <summary>
        /// Gets an int flag runtime by its data asset.
        /// </summary>
        public WorldFlagIntRuntime GetIntFlag(WorldFlagInt_SO flagData)
        {
            return _manager?.GetIntFlag(flagData);
        }

        /// <summary>
        /// Tries to get a bool flag value.
        /// </summary>
        public bool TryGetBoolValue(WorldFlagBool_SO flagData, out bool value)
        {
            if (_manager != null)
            {
                return _manager.TryGetBoolValue(flagData, out value);
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Tries to get an int flag value.
        /// </summary>
        public bool TryGetIntValue(WorldFlagInt_SO flagData, out int value)
        {
            if (_manager != null)
            {
                return _manager.TryGetIntValue(flagData, out value);
            }
            value = default;
            return false;
        }

        #endregion

        #region Flag Modification

        /// <summary>
        /// Sets a bool flag value.
        /// </summary>
        public bool SetBoolValue(WorldFlagBool_SO flagData, bool value)
        {
            return _manager?.SetBoolValue(flagData, value) ?? false;
        }

        /// <summary>
        /// Sets an int flag value.
        /// </summary>
        public bool SetIntValue(WorldFlagInt_SO flagData, int value)
        {
            return _manager?.SetIntValue(flagData, value) ?? false;
        }

        /// <summary>
        /// Increments an int flag value.
        /// </summary>
        public bool IncrementIntValue(WorldFlagInt_SO flagData, int amount = 1)
        {
            return _manager?.IncrementIntValue(flagData, amount) ?? false;
        }

        /// <summary>
        /// Decrements an int flag value.
        /// </summary>
        public bool DecrementIntValue(WorldFlagInt_SO flagData, int amount = 1)
        {
            return _manager?.DecrementIntValue(flagData, amount) ?? false;
        }

        #endregion

        #region Reset

        /// <summary>
        /// Resets all flags to their default values.
        /// </summary>
        public void ResetAllFlags()
        {
            _manager?.ResetAllFlags();
        }

        /// <summary>
        /// Resets a specific flag to its default value.
        /// </summary>
        public bool ResetFlag(WorldFlagBase_SO flagData)
        {
            return _manager?.ResetFlag(flagData) ?? false;
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
        private int RegisteredFlagCount => FlagCount;
#endif

        #endregion
    }
}
