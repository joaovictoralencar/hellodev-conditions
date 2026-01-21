using System.Collections.Generic;
using HelloDev.Logging;
using UnityEngine;
using Logger = HelloDev.Logging.Logger;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Registry of all WorldFlags in the game. Use this for save/load auto-discovery.
    /// Create one instance and populate it with all your WorldFlag assets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Usage: Assign this registry to WorldFlagManagerBehaviour's "Registry" field in the inspector.
    /// The manager will automatically register all flags from this registry.
    /// </para>
    /// <para>
    /// Debug operations for all flags are centralized here to avoid redundant
    /// locator references on each individual flag asset.
    /// </para>
    /// </remarks>
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
        /// Resets all flags to their default values via the provided locator.
        /// </summary>
        /// <param name="flagLocator">The flag locator to use.</param>
        public void ResetAllFlags(WorldFlagLocator_SO flagLocator)
        {
            if (flagLocator != null && flagLocator.IsAvailable)
            {
                flagLocator.Manager.ResetAllFlags();
            }
            else
            {
                Logger.LogWarning(LogSystems.WorldFlags, "Cannot reset flags - flagLocator not available.");
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
            Logger.Log(LogSystems.WorldFlags, $"Found {worldFlags.Count} WorldFlag assets.");
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
                Logger.LogWarning(LogSystems.WorldFlags, $"Duplicate flags found: {string.Join(", ", duplicates)}");
            }
            else
            {
                Logger.Log(LogSystems.WorldFlags, "No duplicates found.");
            }
        }

        [Title("Runtime Debug")]
        [InfoBox("Assign a WorldFlagLocator_SO to use runtime debug features.")]
        [SerializeField]
        private WorldFlagLocator_SO debugLocator;

        [ShowInInspector, ReadOnly]
        [ShowIf("@debugLocator != null")]
        [PropertyOrder(50)]
        private bool ManagerAvailable => debugLocator != null && debugLocator.IsAvailable;

        [Button("Log All Flag Values")]
        [EnableIf("@UnityEngine.Application.isPlaying && debugLocator != null && debugLocator.IsAvailable")]
        private void LogAllFlagValues()
        {
            if (debugLocator == null || !debugLocator.IsAvailable)
            {
                Logger.LogWarning(LogSystems.WorldFlags, "WorldFlagLocator not available.");
                return;
            }

            Logger.Log(LogSystems.WorldFlags, "Current flag values:");
            foreach (var flag in worldFlags)
            {
                if (flag == null) continue;

                var runtime = debugLocator.Manager.GetFlag(flag);
                string value = runtime?.GetValueAsString() ?? "(not registered)";
                Logger.Log(LogSystems.WorldFlags, $"  [{flag.GetType().Name}] {flag.FlagName}: {value}");
            }
        }

        [Button("Reset All Flags to Defaults")]
        [EnableIf("@UnityEngine.Application.isPlaying && debugLocator != null && debugLocator.IsAvailable")]
        [GUIColor(0.8f, 0.6f, 0.4f)]
        private void ResetAllFlagsToDefaults()
        {
            if (debugLocator == null || !debugLocator.IsAvailable)
            {
                Logger.LogWarning(LogSystems.WorldFlags, "WorldFlagLocator not available.");
                return;
            }

            debugLocator.Manager.ResetAllFlags();
            Logger.Log(LogSystems.WorldFlags, "All flags reset to defaults.");
        }

        [Title("Individual Flag Debug")]
        [InfoBox("Select a flag to modify its value at runtime.")]
        [SerializeField]
        [ValueDropdown(nameof(GetFlagDropdown))]
        [ShowIf("@UnityEngine.Application.isPlaying && debugLocator != null && debugLocator.IsAvailable")]
        private WorldFlagBase_SO selectedFlag;

        private IEnumerable<ValueDropdownItem<WorldFlagBase_SO>> GetFlagDropdown()
        {
            yield return new ValueDropdownItem<WorldFlagBase_SO>("(None)", null);
            foreach (var flag in worldFlags)
            {
                if (flag != null)
                {
                    yield return new ValueDropdownItem<WorldFlagBase_SO>(flag.FlagName, flag);
                }
            }
        }

        [ShowInInspector, ReadOnly]
        [ShowIf("@UnityEngine.Application.isPlaying && selectedFlag != null && debugLocator != null && debugLocator.IsAvailable")]
        [PropertyOrder(60)]
        private string SelectedFlagValue
        {
            get
            {
                if (selectedFlag == null || debugLocator == null || !debugLocator.IsAvailable)
                    return "(none)";
                var runtime = debugLocator.Manager.GetFlag(selectedFlag);
                return runtime?.GetValueAsString() ?? "(not registered)";
            }
        }

        [HorizontalGroup("BoolButtons")]
        [Button("Set True")]
        [ShowIf("@UnityEngine.Application.isPlaying && selectedFlag is WorldFlagBool_SO && debugLocator != null && debugLocator.IsAvailable")]
        private void SetSelectedBoolTrue()
        {
            if (selectedFlag is WorldFlagBool_SO boolFlag && debugLocator?.IsAvailable == true)
            {
                debugLocator.Manager.GetBoolFlag(boolFlag)?.SetValue(true);
            }
        }

        [HorizontalGroup("BoolButtons")]
        [Button("Set False")]
        [ShowIf("@UnityEngine.Application.isPlaying && selectedFlag is WorldFlagBool_SO && debugLocator != null && debugLocator.IsAvailable")]
        private void SetSelectedBoolFalse()
        {
            if (selectedFlag is WorldFlagBool_SO boolFlag && debugLocator?.IsAvailable == true)
            {
                debugLocator.Manager.GetBoolFlag(boolFlag)?.SetValue(false);
            }
        }

        [HorizontalGroup("IntButtons")]
        [Button("Increment")]
        [ShowIf("@UnityEngine.Application.isPlaying && selectedFlag is WorldFlagInt_SO && debugLocator != null && debugLocator.IsAvailable")]
        private void IncrementSelectedInt()
        {
            if (selectedFlag is WorldFlagInt_SO intFlag && debugLocator?.IsAvailable == true)
            {
                debugLocator.Manager.GetIntFlag(intFlag)?.Increment();
            }
        }

        [HorizontalGroup("IntButtons")]
        [Button("Decrement")]
        [ShowIf("@UnityEngine.Application.isPlaying && selectedFlag is WorldFlagInt_SO && debugLocator != null && debugLocator.IsAvailable")]
        private void DecrementSelectedInt()
        {
            if (selectedFlag is WorldFlagInt_SO intFlag && debugLocator?.IsAvailable == true)
            {
                debugLocator.Manager.GetIntFlag(intFlag)?.Decrement();
            }
        }

        [HorizontalGroup("IntButtons")]
        [Button("Set to Max")]
        [ShowIf("@UnityEngine.Application.isPlaying && selectedFlag is WorldFlagInt_SO && debugLocator != null && debugLocator.IsAvailable")]
        private void SetSelectedIntToMax()
        {
            if (selectedFlag is WorldFlagInt_SO intFlag && debugLocator?.IsAvailable == true)
            {
                debugLocator.Manager.GetIntFlag(intFlag)?.SetValue(intFlag.MaxValue);
            }
        }

        [HorizontalGroup("IntButtons")]
        [Button("Set to Min")]
        [ShowIf("@UnityEngine.Application.isPlaying && selectedFlag is WorldFlagInt_SO && debugLocator != null && debugLocator.IsAvailable")]
        private void SetSelectedIntToMin()
        {
            if (selectedFlag is WorldFlagInt_SO intFlag && debugLocator?.IsAvailable == true)
            {
                debugLocator.Manager.GetIntFlag(intFlag)?.SetValue(intFlag.MinValue);
            }
        }

        [Button("Reset Selected Flag")]
        [ShowIf("@UnityEngine.Application.isPlaying && selectedFlag != null && debugLocator != null && debugLocator.IsAvailable")]
        private void ResetSelectedFlag()
        {
            if (selectedFlag != null && debugLocator?.IsAvailable == true)
            {
                debugLocator.Manager.ResetFlag(selectedFlag);
            }
        }
#endif

        #endregion
    }
}
