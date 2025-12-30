using System;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Abstract base class for world flag configuration assets.
    /// WorldFlags are immutable data containers; use WorldFlagManager for runtime state.
    /// </summary>
    public abstract class WorldFlagBase_SO : ScriptableObject
    {
        #region Serialized Fields

        [SerializeField, HideInInspector]
        private string flagGuid;

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
        /// Gets a unique identifier for this flag (stable GUID).
        /// </summary>
        public string FlagId
        {
            get
            {
                if (string.IsNullOrEmpty(flagGuid))
                {
                    flagGuid = Guid.NewGuid().ToString();
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                }
                return flagGuid;
            }
        }

        #endregion
    }
}
