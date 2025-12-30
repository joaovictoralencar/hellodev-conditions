using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Registry of all WorldFlags in the game. Use this for save/load auto-discovery.
    /// Create one instance and populate it with all your WorldFlag assets.
    ///
    /// Usage with QuestSaveManager:
    /// <code>
    /// [SerializeField] private WorldFlagRegistry_SO worldFlagRegistry;
    ///
    /// void Start()
    /// {
    ///     QuestSaveManager.Instance.SetWorldFlagRegistry(worldFlagRegistry);
    /// }
    /// </code>
    /// </summary>
    [CreateAssetMenu(fileName = "WorldFlagRegistry", menuName = "HelloDev/Conditions/World Flag Registry")]
    public class WorldFlagRegistry_SO : ScriptableObject
    {
        #region Serialized Fields

#if ODIN_INSPECTOR
        [Title("World Flags")]
        [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
        [InfoBox("Add all WorldFlag assets here for save/load auto-discovery.")]
#endif
        [SerializeField]
        [Tooltip("All WorldFlag assets in the game.")]
        private List<WorldFlagBase_SO> worldFlags = new();

        #endregion

        #region Properties

        /// <summary>
        /// Gets all registered world flags.
        /// </summary>
        public IReadOnlyList<WorldFlagBase_SO> AllFlags => worldFlags;

        /// <summary>
        /// Gets the count of registered flags.
        /// </summary>
        public int Count => worldFlags.Count;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a world flag by its FlagId.
        /// </summary>
        /// <param name="flagId">The GUID of the flag.</param>
        /// <returns>The flag, or null if not found.</returns>
        public WorldFlagBase_SO GetByFlagId(string flagId)
        {
            if (string.IsNullOrEmpty(flagId)) return null;

            foreach (var flag in worldFlags)
            {
                if (flag != null && flag.FlagId == flagId)
                    return flag;
            }
            return null;
        }

        /// <summary>
        /// Gets a world flag by its name.
        /// </summary>
        /// <param name="flagName">The name of the flag.</param>
        /// <returns>The flag, or null if not found.</returns>
        public WorldFlagBase_SO GetByName(string flagName)
        {
            if (string.IsNullOrEmpty(flagName)) return null;

            foreach (var flag in worldFlags)
            {
                if (flag != null && flag.FlagName == flagName)
                    return flag;
            }
            return null;
        }

        /// <summary>
        /// Checks if a flag is registered.
        /// </summary>
        /// <param name="flag">The flag to check.</param>
        /// <returns>True if registered.</returns>
        public bool Contains(WorldFlagBase_SO flag)
        {
            return flag != null && worldFlags.Contains(flag);
        }

        /// <summary>
        /// Resets all flags to their default values via the provided service.
        /// </summary>
        /// <param name="flagService">The flag service to use.</param>
        public void ResetAllFlags(WorldFlagService_SO flagService)
        {
            if (flagService != null && flagService.IsAvailable)
            {
                flagService.ResetAllFlags();
            }
            else
            {
                Debug.LogWarning("[WorldFlagRegistry] Cannot reset flags - flagService not available.");
            }
        }

        #endregion

        #region Editor Helpers

#if ODIN_INSPECTOR && UNITY_EDITOR
        [Title("Quick Actions")]
        [Button("Find All WorldFlags in Project", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 0.4f)]
        private void FindAllWorldFlagsInProject()
        {
            worldFlags.Clear();

            var guids = UnityEditor.AssetDatabase.FindAssets("t:WorldFlagBase_SO");
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var flag = UnityEditor.AssetDatabase.LoadAssetAtPath<WorldFlagBase_SO>(path);
                if (flag != null && !worldFlags.Contains(flag))
                {
                    worldFlags.Add(flag);
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[WorldFlagRegistry] Found {worldFlags.Count} WorldFlag assets.");
        }

        [Button("Clear All")]
        [GUIColor(0.8f, 0.4f, 0.4f)]
        private void ClearAll()
        {
            worldFlags.Clear();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [Title("Validation")]
        [Button("Check for Duplicates")]
        private void CheckForDuplicates()
        {
            var seen = new HashSet<string>();
            var duplicates = new List<string>();

            foreach (var flag in worldFlags)
            {
                if (flag == null) continue;

                if (!seen.Add(flag.FlagId))
                {
                    duplicates.Add(flag.FlagName);
                }
            }

            if (duplicates.Count > 0)
            {
                Debug.LogWarning($"[WorldFlagRegistry] Duplicate flags found: {string.Join(", ", duplicates)}");
            }
            else
            {
                Debug.Log("[WorldFlagRegistry] No duplicates found.");
            }
        }

        [Title("Debug Service")]
        [InfoBox("Assign a WorldFlagService_SO to use runtime debug features.")]
        [SerializeField]
        private WorldFlagService_SO debugService;

        [Button("Log All Flag Values (Runtime)")]
        [EnableIf("@UnityEngine.Application.isPlaying && debugService != null")]
        private void LogAllFlagValues()
        {
            if (debugService == null || !debugService.IsAvailable)
            {
                Debug.LogWarning("[WorldFlagRegistry] WorldFlagService not available.");
                return;
            }

            Debug.Log("[WorldFlagRegistry] Current flag values:");
            foreach (var flag in worldFlags)
            {
                if (flag == null) continue;

                var runtime = debugService.GetFlag(flag);
                string value = runtime?.GetValueAsString() ?? "(not registered)";
                Debug.Log($"  [{flag.GetType().Name}] {flag.FlagName}: {value}");
            }
        }
#endif

        #endregion
    }
}
