using System;
using System.Collections.Generic;
using HelloDev.Logging;
using UnityEngine.Events;
using Logger = HelloDev.Logging.Logger;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Pure C# manager for world flag runtime instances.
    /// Manages the lifecycle of WorldFlagRuntime objects and provides access to flag values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Following Tip #2 (Separate Logic from MonoBehaviors):
    /// </para>
    /// <list type="bullet">
    /// <item><description>This is a pure C# class with no Unity dependencies</description></item>
    /// <item><description>Can be unit tested without Play mode</description></item>
    /// <item><description>WorldFlagManagerBehaviour owns this and handles Unity lifecycle</description></item>
    /// </list>
    /// <para>
    /// Following Tip #1 (Start with Interfaces):
    /// </para>
    /// <list type="bullet">
    /// <item><description>Implements IWorldFlagManager for testability and decoupling</description></item>
    /// <item><description>Type-specific operations are on runtime classes, not the interface</description></item>
    /// </list>
    /// </remarks>
    public class WorldFlagManager : IWorldFlagManager
    {
        #region Runtime State

        private readonly Dictionary<string, WorldFlagRuntime> _runtimeFlags = new();
        private readonly Dictionary<WorldFlagBase_SO, WorldFlagRuntime> _dataToRuntime = new();
        private bool _isInitialized;
        private WorldFlagRegistry_SO _flagRegistry;

        #endregion

        #region Events

        /// <summary>
        /// Fired when any bool flag changes value.
        /// </summary>
        public UnityEvent<WorldFlagBoolRuntime, bool> OnBoolFlagChanged { get; } = new();

        /// <summary>
        /// Fired when any int flag changes value.
        /// </summary>
        public UnityEvent<WorldFlagIntRuntime, int, int> OnIntFlagChanged { get; } = new();

        /// <summary>
        /// Fired when a flag is registered.
        /// </summary>
        public UnityEvent<WorldFlagRuntime> OnFlagRegistered { get; } = new();

        #endregion

        #region IWorldFlagManager Properties

        /// <inheritdoc />
        public bool IsInitialized => _isInitialized;

        /// <inheritdoc />
        public int FlagCount => _runtimeFlags.Count;

        /// <inheritdoc />
        public IReadOnlyCollection<WorldFlagRuntime> AllFlags => _runtimeFlags.Values;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the manager with flags from the provided registry.
        /// </summary>
        /// <param name="flagRegistry">The registry containing all flags to register.</param>
        public void Initialize(WorldFlagRegistry_SO flagRegistry)
        {
            if (_isInitialized)
                return;

            _flagRegistry = flagRegistry;

            // Register flags from the registry
            if (flagRegistry != null)
            {
                foreach (var flagData in flagRegistry.AllFlags)
                {
                    if (flagData != null)
                        RegisterFlag(flagData);
                }
            }

            _isInitialized = true;
            Logger.Log(LogSystems.WorldFlags, $"Initialized with {_runtimeFlags.Count} flags.");
        }

        /// <summary>
        /// Shuts down the manager and clears all state.
        /// </summary>
        public void Shutdown()
        {
            if (!_isInitialized)
                return;

            _runtimeFlags.Clear();
            _dataToRuntime.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// Sets the flag registry and registers all contained flags.
        /// </summary>
        /// <param name="registry">The registry to use.</param>
        public void SetFlagRegistry(WorldFlagRegistry_SO registry)
        {
            _flagRegistry = registry;

            if (registry != null)
            {
                foreach (var flagData in registry.AllFlags)
                {
                    if (flagData != null)
                        RegisterFlag(flagData);
                }
                Logger.Log(LogSystems.WorldFlags, $"Registry set with {registry.Count} flags. Total: {_runtimeFlags.Count}");
            }
        }

        #endregion

        #region Registration

        /// <inheritdoc />
        public WorldFlagRuntime RegisterFlag(WorldFlagBase_SO flagData)
        {
            if (flagData == null)
            {
                Logger.LogWarning(LogSystems.WorldFlags, "Cannot register null flag data.");
                return null;
            }

            // Check if already registered
            if (_dataToRuntime.TryGetValue(flagData, out var existing))
                return existing;

            // Create runtime instance based on type
            WorldFlagRuntime runtime = flagData switch
            {
                WorldFlagBool_SO boolData => CreateBoolRuntime(boolData),
                WorldFlagInt_SO intData => CreateIntRuntime(intData),
                _ => throw new NotSupportedException($"Unknown flag type: {flagData.GetType().Name}")
            };

            _runtimeFlags[flagData.FlagId] = runtime;
            _dataToRuntime[flagData] = runtime;

            OnFlagRegistered?.Invoke(runtime);
            return runtime;
        }

        private WorldFlagBoolRuntime CreateBoolRuntime(WorldFlagBool_SO data)
        {
            var runtime = new WorldFlagBoolRuntime(data);

            // Subscribe to forward events
            runtime.OnValueChanged.AddListener(value =>
            {
                OnBoolFlagChanged?.Invoke(runtime, value);
            });

            return runtime;
        }

        private WorldFlagIntRuntime CreateIntRuntime(WorldFlagInt_SO data)
        {
            var runtime = new WorldFlagIntRuntime(data);

            // Subscribe to forward events
            runtime.OnValueChanged.AddListener((newVal, oldVal) =>
            {
                OnIntFlagChanged?.Invoke(runtime, newVal, oldVal);
            });

            return runtime;
        }

        #endregion

        #region Flag Access

        /// <inheritdoc />
        public WorldFlagRuntime GetFlag(WorldFlagBase_SO flagData)
        {
            if (flagData == null) return null;
            return _dataToRuntime.TryGetValue(flagData, out var runtime) ? runtime : null;
        }

        /// <inheritdoc />
        public WorldFlagRuntime GetFlagById(string flagId)
        {
            if (string.IsNullOrEmpty(flagId)) return null;
            return _runtimeFlags.TryGetValue(flagId, out var runtime) ? runtime : null;
        }

        /// <inheritdoc />
        public WorldFlagBoolRuntime GetBoolFlag(WorldFlagBool_SO flagData)
        {
            return GetFlag(flagData) as WorldFlagBoolRuntime;
        }

        /// <inheritdoc />
        public WorldFlagIntRuntime GetIntFlag(WorldFlagInt_SO flagData)
        {
            return GetFlag(flagData) as WorldFlagIntRuntime;
        }

        /// <summary>
        /// Tries to get a bool flag value.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <param name="value">The current value if found.</param>
        /// <returns>True if the flag was found.</returns>
        public bool TryGetBoolValue(WorldFlagBool_SO flagData, out bool value)
        {
            var runtime = GetBoolFlag(flagData);
            if (runtime != null)
            {
                value = runtime.Value;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Tries to get an int flag value.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <param name="value">The current value if found.</param>
        /// <returns>True if the flag was found.</returns>
        public bool TryGetIntValue(WorldFlagInt_SO flagData, out int value)
        {
            var runtime = GetIntFlag(flagData);
            if (runtime != null)
            {
                value = runtime.Value;
                return true;
            }
            value = default;
            return false;
        }

        #endregion

        #region Reset

        /// <inheritdoc />
        public void ResetAllFlags()
        {
            foreach (var runtime in _runtimeFlags.Values)
            {
                runtime.ResetToDefault();
            }
            Logger.Log(LogSystems.WorldFlags, $"Reset {_runtimeFlags.Count} flags to defaults.");
        }

        /// <inheritdoc />
        public bool ResetFlag(WorldFlagBase_SO flagData)
        {
            var runtime = GetFlag(flagData);
            if (runtime != null)
            {
                runtime.ResetToDefault();
                return true;
            }
            return false;
        }

        #endregion
    }
}
