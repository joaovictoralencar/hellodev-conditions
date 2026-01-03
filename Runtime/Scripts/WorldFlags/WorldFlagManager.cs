using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    /// Manager for world flag runtime instances.
    /// Manages the lifecycle of WorldFlagRuntime objects and provides access to flag values.
    /// Registers itself with a WorldFlagLocator_SO for decoupled access.
    /// </summary>
    /// <remarks>
    /// Supports two initialization modes:
    /// <list type="bullet">
    /// <item><term>Standalone</term><description>Self-initializes in OnEnable (default)</description></item>
    /// <item><term>Bootstrap</term><description>Waits for GameBootstrap to call InitializeAsync</description></item>
    /// </list>
    /// Set <see cref="selfInitialize"/> to false when using with GameBootstrap.
    /// </remarks>
    public class WorldFlagManager : MonoBehaviour, IBootstrapInitializable
    {
        #region Serialized Fields

#if ODIN_INSPECTOR
        [Title("Locator")]
        [Required]
        [InfoBox("Reference the WorldFlagLocator_SO asset. This manager will register itself with the locator on enable.")]
#endif
        [SerializeField]
        [Tooltip("The locator SO that provides decoupled access to this manager.")]
        private WorldFlagLocator_SO locator;

#if ODIN_INSPECTOR
        [Title("Configuration")]
        [InfoBox("Drag WorldFlag_SO assets here or use a WorldFlagRegistry_SO for auto-discovery.")]
#endif
        [SerializeField]
        [Tooltip("WorldFlag data assets to register on startup.")]
        private List<WorldFlagBase_SO> flagAssets = new();

#if ODIN_INSPECTOR
        [PropertySpace(10)]
#endif
        [SerializeField]
        [Tooltip("Optional: Use a registry for easier management of all flags.")]
        private WorldFlagRegistry_SO flagRegistry;

        [SerializeField]
        [Tooltip("If true, this manager persists across scene loads.")]
        private bool persistent = true;

#if ODIN_INSPECTOR
        [Title("Initialization")]
        [ToggleLeft]
#else
        [Header("Initialization")]
#endif
        [SerializeField]
        [Tooltip("If true, initializes in OnEnable. If false, waits for external initialization (e.g., GameBootstrap).")]
        private bool selfInitialize = true;

        #endregion

        #region Runtime State

        private readonly Dictionary<string, WorldFlagRuntime> _runtimeFlags = new();
        private readonly Dictionary<WorldFlagBase_SO, WorldFlagRuntime> _dataToRuntime = new();
        private bool _isInitialized;

        #endregion


        #region IBootstrapInitializable

        /// <inheritdoc />
        public bool SelfInitialize => selfInitialize;

        /// <inheritdoc />
        public int InitializationPriority => 100; // Data layer

        /// <inheritdoc />
        public bool IsInitialized => _isInitialized;

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

        #region Properties

        /// <summary>
        /// Gets the locator this manager is registered with.
        /// </summary>
        public WorldFlagLocator_SO Locator => locator;

        /// <summary>
        /// Gets the count of registered flags.
        /// </summary>
        public int FlagCount => _runtimeFlags.Count;

        /// <summary>
        /// Gets all registered runtime flags.
        /// </summary>
        public IReadOnlyCollection<WorldFlagRuntime> AllFlags => _runtimeFlags.Values;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (persistent)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnEnable()
        {
            // Only self-initialize if in standalone mode
            if (selfInitialize)
            {
                _ = InitializeAsync();
            }
        }

        private void OnDisable()
        {
            Shutdown();
        }

        /// <inheritdoc />
        public Task InitializeAsync()
        {
            if (_isInitialized)
                return Task.CompletedTask;

            Logger.Log(LogSystems.WorldFlags, "Starting initialization...");

            // Register with locator
            if (locator != null)
            {
                locator.Register(this);
            }
            else
            {
                Logger.LogWarning(LogSystems.WorldFlags, $"No locator assigned on {name}. Flags will not be accessible via locator.");
            }

            // Register initial flags
            RegisterInitialFlags();

            _isInitialized = true;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            if (!_isInitialized)
                return;

            // Unregister from locator
            if (locator != null)
            {
                locator.Unregister(this);
            }

            _isInitialized = false;
        }

        private void RegisterInitialFlags()
        {
            // Register flags from the serialized list
            foreach (var flagData in flagAssets)
            {
                if (flagData != null)
                    RegisterFlag(flagData);
            }

            // Register flags from the registry
            if (flagRegistry != null)
            {
                foreach (var flagData in flagRegistry.AllFlags)
                {
                    if (flagData != null)
                        RegisterFlag(flagData);
                }
            }

            Logger.Log(LogSystems.WorldFlags, $"Initialized with {_runtimeFlags.Count} flags.");
        }

        #endregion

        #region Registration

        /// <summary>
        /// Registers a flag data asset and creates its runtime instance.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <returns>The created runtime instance, or existing instance if already registered.</returns>
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

        /// <summary>
        /// Sets the flag registry and registers all contained flags.
        /// </summary>
        /// <param name="registry">The registry to use.</param>
        public void SetFlagRegistry(WorldFlagRegistry_SO registry)
        {
            flagRegistry = registry;

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

        /// <summary>
        /// Gets a flag runtime by its data asset.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <returns>The runtime instance, or null if not registered.</returns>
        public WorldFlagRuntime GetFlag(WorldFlagBase_SO flagData)
        {
            if (flagData == null) return null;
            return _dataToRuntime.TryGetValue(flagData, out var runtime) ? runtime : null;
        }

        /// <summary>
        /// Gets a flag runtime by its ID.
        /// </summary>
        /// <param name="flagId">The flag GUID.</param>
        /// <returns>The runtime instance, or null if not registered.</returns>
        public WorldFlagRuntime GetFlagById(string flagId)
        {
            if (string.IsNullOrEmpty(flagId)) return null;
            return _runtimeFlags.TryGetValue(flagId, out var runtime) ? runtime : null;
        }

        /// <summary>
        /// Gets a bool flag runtime by its data asset.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <returns>The runtime instance, or null if not registered or wrong type.</returns>
        public WorldFlagBoolRuntime GetBoolFlag(WorldFlagBool_SO flagData)
        {
            return GetFlag(flagData) as WorldFlagBoolRuntime;
        }

        /// <summary>
        /// Gets an int flag runtime by its data asset.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <returns>The runtime instance, or null if not registered or wrong type.</returns>
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

        #region Flag Modification

        /// <summary>
        /// Sets a bool flag value.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <param name="value">The new value.</param>
        /// <returns>True if the flag was found and set.</returns>
        public bool SetBoolValue(WorldFlagBool_SO flagData, bool value)
        {
            var runtime = GetBoolFlag(flagData);
            if (runtime != null)
            {
                runtime.SetValue(value);
                return true;
            }
            Logger.LogWarning(LogSystems.WorldFlags, $"Bool flag '{flagData?.FlagName}' not registered.");
            return false;
        }

        /// <summary>
        /// Sets an int flag value.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <param name="value">The new value.</param>
        /// <returns>True if the flag was found and set.</returns>
        public bool SetIntValue(WorldFlagInt_SO flagData, int value)
        {
            var runtime = GetIntFlag(flagData);
            if (runtime != null)
            {
                runtime.SetValue(value);
                return true;
            }
            Logger.LogWarning(LogSystems.WorldFlags, $"Int flag '{flagData?.FlagName}' not registered.");
            return false;
        }

        /// <summary>
        /// Increments an int flag value.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <param name="amount">Amount to add.</param>
        /// <returns>True if the flag was found and incremented.</returns>
        public bool IncrementIntValue(WorldFlagInt_SO flagData, int amount = 1)
        {
            var runtime = GetIntFlag(flagData);
            if (runtime != null)
            {
                runtime.Increment(amount);
                return true;
            }
            Logger.LogWarning(LogSystems.WorldFlags, $"Int flag '{flagData?.FlagName}' not registered.");
            return false;
        }

        /// <summary>
        /// Decrements an int flag value.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <param name="amount">Amount to subtract.</param>
        /// <returns>True if the flag was found and decremented.</returns>
        public bool DecrementIntValue(WorldFlagInt_SO flagData, int amount = 1)
        {
            var runtime = GetIntFlag(flagData);
            if (runtime != null)
            {
                runtime.Decrement(amount);
                return true;
            }
            Logger.LogWarning(LogSystems.WorldFlags, $"Int flag '{flagData?.FlagName}' not registered.");
            return false;
        }

        #endregion

        #region Reset

        /// <summary>
        /// Resets all flags to their default values.
        /// </summary>
        public void ResetAllFlags()
        {
            foreach (var runtime in _runtimeFlags.Values)
            {
                runtime.ResetToDefault();
            }
            Logger.Log(LogSystems.WorldFlags, $"Reset {_runtimeFlags.Count} flags to defaults.");
        }

        /// <summary>
        /// Resets a specific flag to its default value.
        /// </summary>
        /// <param name="flagData">The flag data asset.</param>
        /// <returns>True if the flag was found and reset.</returns>
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

        #region Debug

#if ODIN_INSPECTOR && UNITY_EDITOR
        [Title("Debug")]
        [ShowInInspector, ReadOnly]
        [PropertyOrder(100)]
        private int RuntimeFlagCount => _runtimeFlags.Count;

        [Button("Log All Flag Values")]
        [PropertyOrder(101)]
        private void DebugLogAllValues()
        {
            Logger.Log(LogSystems.WorldFlags, "Current flag values:");
            foreach (var runtime in _runtimeFlags.Values)
            {
                Logger.Log(LogSystems.WorldFlags, $"  [{runtime.GetType().Name}] {runtime.FlagName}: {runtime.GetValueAsString()}");
            }
        }

        [Button("Reset All Flags")]
        [PropertyOrder(102)]
        private void DebugResetAll()
        {
            ResetAllFlags();
        }
#endif

        #endregion
    }
}
