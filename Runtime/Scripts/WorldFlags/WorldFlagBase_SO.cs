using System;
using HelloDev.Utils;
using UnityEngine;
using UnityEngine.Events;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Abstract base class for self-contained world state flags.
    /// WorldFlags store their own values and fire events when changed.
    /// Use these to track persistent game state (quest branches, story decisions, etc.).
    /// </summary>
    public abstract class WorldFlagBase_SO : RuntimeScriptableObject
    {
        #region Serialized Fields

#if ODIN_INSPECTOR
        [BoxGroup("Flag Identity")]
        [PropertyOrder(0)]
#endif
        [SerializeField]
        [Tooltip("Developer-friendly name for this flag.")]
        private string flagName;

#if ODIN_INSPECTOR
        [BoxGroup("Flag Identity")]
        [PropertyOrder(1)]
        [TextArea(2, 4)]
#endif
        [SerializeField]
        [Tooltip("Description of what this flag represents.")]
        private string description;

#if ODIN_INSPECTOR
        [BoxGroup("Flag Identity")]
        [PropertyOrder(2)]
#endif
        [SerializeField]
        [Tooltip("Category for organization (e.g., 'MainQuest', 'SideQuest', 'WorldState').")]
        private string category;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the developer-friendly name for this flag.
        /// </summary>
        public string FlagName => string.IsNullOrEmpty(flagName) ? name : flagName;

        /// <summary>
        /// Gets the description of this flag.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Gets the category of this flag for organization.
        /// </summary>
        public string Category => category;

        /// <summary>
        /// Gets a unique identifier for this flag (uses asset name).
        /// </summary>
        public string FlagId => name;

        #endregion

        #region Abstract Members

        /// <summary>
        /// Resets the flag to its default value.
        /// </summary>
        public abstract void ResetToDefault();

        /// <summary>
        /// Gets a string representation of the current value.
        /// </summary>
        public abstract string GetValueAsString();

        #endregion

        /// <summary>
        /// Called when entering play mode. Resets to default value.
        /// </summary>
        protected override void OnScriptableObjectReset()
        {
            ResetToDefault();
        }
    }
}
