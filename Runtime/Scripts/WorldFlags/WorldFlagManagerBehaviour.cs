using System.Threading.Tasks;
using HelloDev.Logging;
using HelloDev.Utils;
using UnityEngine;
using Logger = HelloDev.Logging.Logger;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Thin MonoBehaviour wrapper for WorldFlagManager.
    /// Handles Unity lifecycle and owns the pure C# WorldFlagManager instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Following Tip #2 (Separate Logic from MonoBehaviors):
    /// </para>
    /// <list type="bullet">
    /// <item><description>This behaviour only handles Unity lifecycle (Awake, OnEnable, OnDisable)</description></item>
    /// <item><description>All actual logic lives in the pure C# WorldFlagManager class</description></item>
    /// <item><description>The manager becomes testable without Play mode</description></item>
    /// </list>
    /// <para>
    /// Supports two initialization modes:
    /// </para>
    /// <list type="bullet">
    /// <item><term>Standalone</term><description>Self-initializes in OnEnable (default)</description></item>
    /// <item><term>Bootstrap</term><description>Waits for GameBootstrap to call InitializeAsync</description></item>
    /// </list>
    /// Set <see cref="selfInitialize"/> to false when using with GameBootstrap.
    /// </remarks>
    public class WorldFlagManagerBehaviour : MonoBehaviour, IBootstrapInitializable
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
        [Required]
        [InfoBox("Use a WorldFlagRegistry_SO for centralized flag management and auto-discovery.")]
#endif
        [SerializeField]
        [Tooltip("The registry containing all WorldFlag assets to register on startup.")]
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

        private WorldFlagManager _manager;
        private GameContext _context;

        #endregion

        #region IBootstrapInitializable

        /// <inheritdoc />
        public bool SelfInitialize
        {
            get => selfInitialize;
            set => selfInitialize = value;
        }

        /// <inheritdoc />
        public int InitializationPriority => 100; // Data layer

        /// <inheritdoc />
        public bool IsInitialized => _manager?.IsInitialized ?? false;

        /// <summary>
        /// Receives the game context from GameBootstrap.
        /// </summary>
        /// <param name="context">The game context for service registration.</param>
        public void ReceiveContext(GameContext context)
        {
            _context = context;
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (persistent)
            {
                DontDestroyOnLoad(gameObject);
            }

            // Create the manager instance
            _manager = new WorldFlagManager();
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
            if (_manager == null)
            {
                _manager = new WorldFlagManager();
            }

            if (_manager.IsInitialized)
                return Task.CompletedTask;

            Logger.Log(LogSystems.WorldFlags, "Starting initialization...");

            // Initialize the manager with flags from the registry
            _manager.Initialize(flagRegistry);

            // Register with locator (for SO access)
            if (locator != null)
            {
                locator.Register(_manager);
            }
            else
            {
                Logger.LogWarning(LogSystems.WorldFlags, $"No locator assigned on {name}. Flags will not be accessible via locator.");
            }

            // Self-register to context
            _context?.Register<IWorldFlagManager>(_manager);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            if (_manager == null || !_manager.IsInitialized)
                return;

            // Unregister from locator
            if (locator != null)
            {
                locator.Unregister(_manager);
            }

            // Unregister from context
            _context?.Unregister<IWorldFlagManager>();

            _manager.Shutdown();
        }

        #endregion

        #region Debug

#if ODIN_INSPECTOR && UNITY_EDITOR
        [Title("Debug")]
        [ShowInInspector, ReadOnly]
        [PropertyOrder(100)]
        private int RuntimeFlagCount => _manager?.FlagCount ?? 0;

        [Button("Log All Flag Values")]
        [PropertyOrder(101)]
        private void DebugLogAllValues()
        {
            if (_manager == null)
            {
                Logger.LogWarning(LogSystems.WorldFlags, "Manager not initialized.");
                return;
            }

            Logger.Log(LogSystems.WorldFlags, "Current flag values:");
            foreach (var runtime in _manager.AllFlags)
            {
                Logger.Log(LogSystems.WorldFlags, $"  [{runtime.GetType().Name}] {runtime.FlagName}: {runtime.GetValueAsString()}");
            }
        }

        [Button("Reset All Flags")]
        [PropertyOrder(102)]
        private void DebugResetAll()
        {
            _manager?.ResetAllFlags();
        }
#endif

        #endregion
    }
}
