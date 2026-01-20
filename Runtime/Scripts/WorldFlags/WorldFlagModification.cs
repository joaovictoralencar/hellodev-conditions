using System;
using HelloDev.Logging;
using UnityEngine;
using Logger = HelloDev.Logging.Logger;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Specifies how to modify an integer world flag.
    /// </summary>
    public enum WorldFlagIntOperation
    {
        /// <summary>Set the flag to an exact value.</summary>
        Set,
        /// <summary>Add to the current value.</summary>
        Add,
        /// <summary>Subtract from the current value.</summary>
        Subtract
    }

    /// <summary>
    /// Defines a modification to apply to a world flag.
    /// Used by quest transitions to update world state when choices are made.
    ///
    /// Examples:
    /// - Set "spared_merchant" to true when player chooses mercy
    /// - Add 10 to "bandit_reputation" when player helps bandits
    /// - Set "story_path" to 2 when player chooses the dark path
    /// </summary>
    [Serializable]
    public class WorldFlagModification
    {
        #region Serialized Fields

#if ODIN_INSPECTOR
        [BoxGroup("Locator")]
        [PropertyOrder(-1)]
        [Tooltip("The WorldFlagLocator that provides access to flag runtime values.")]
#endif
        [SerializeField]
        private WorldFlagLocator_SO flagLocator;

#if ODIN_INSPECTOR
        [BoxGroup("Flag Type")]
        [PropertyOrder(0)]
        [Tooltip("If true, this modification affects a boolean flag. If false, it affects an integer flag.")]
#endif
        [SerializeField]
        private bool isBoolFlag = true;

#if ODIN_INSPECTOR
        [BoxGroup("Boolean Flag")]
        [PropertyOrder(10)]
        [ShowIf(nameof(isBoolFlag))]
        [Tooltip("The boolean flag to modify.")]
#endif
        [SerializeField]
        private WorldFlagBool_SO boolFlag;

#if ODIN_INSPECTOR
        [BoxGroup("Boolean Flag")]
        [PropertyOrder(11)]
        [ShowIf(nameof(isBoolFlag))]
        [Tooltip("The value to set the flag to.")]
#endif
        [SerializeField]
        private bool boolValue = true;

#if ODIN_INSPECTOR
        [BoxGroup("Integer Flag")]
        [PropertyOrder(20)]
        [HideIf(nameof(isBoolFlag))]
        [Tooltip("The integer flag to modify.")]
#endif
        [SerializeField]
        private WorldFlagInt_SO intFlag;

#if ODIN_INSPECTOR
        [BoxGroup("Integer Flag")]
        [PropertyOrder(21)]
        [HideIf(nameof(isBoolFlag))]
        [Tooltip("How to modify the integer flag.")]
#endif
        [SerializeField]
        private WorldFlagIntOperation intOperation = WorldFlagIntOperation.Set;

#if ODIN_INSPECTOR
        [BoxGroup("Integer Flag")]
        [PropertyOrder(22)]
        [HideIf(nameof(isBoolFlag))]
        [Tooltip("The value to use for the operation.")]
#endif
        [SerializeField]
        private int intValue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether this modification affects a boolean flag.
        /// </summary>
        public bool IsBoolFlag => isBoolFlag;

        /// <summary>
        /// Gets the boolean flag to modify (null if integer flag).
        /// </summary>
        public WorldFlagBool_SO BoolFlag => boolFlag;

        /// <summary>
        /// Gets the value to set the boolean flag to.
        /// </summary>
        public bool BoolValue => boolValue;

        /// <summary>
        /// Gets the integer flag to modify (null if boolean flag).
        /// </summary>
        public WorldFlagInt_SO IntFlag => intFlag;

        /// <summary>
        /// Gets the operation to perform on the integer flag.
        /// </summary>
        public WorldFlagIntOperation IntOperation => intOperation;

        /// <summary>
        /// Gets the value to use for the integer operation.
        /// </summary>
        public int IntValue => intValue;

        /// <summary>
        /// Gets whether this modification has a valid target flag.
        /// </summary>
        public bool IsValid => (isBoolFlag && boolFlag != null) || (!isBoolFlag && intFlag != null);

        /// <summary>
        /// Gets whether this modification has a valid locator reference.
        /// </summary>
        public bool HasLocator => flagLocator != null;

        /// <summary>
        /// Gets a description of this modification for debugging.
        /// </summary>
        public string Description
        {
            get
            {
                if (!IsValid) return "(invalid)";

                if (isBoolFlag)
                    return $"{boolFlag.FlagName} = {boolValue}";

                return intOperation switch
                {
                    WorldFlagIntOperation.Set => $"{intFlag.FlagName} = {intValue}",
                    WorldFlagIntOperation.Add => $"{intFlag.FlagName} += {intValue}",
                    WorldFlagIntOperation.Subtract => $"{intFlag.FlagName} -= {intValue}",
                    _ => "(unknown operation)"
                };
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Applies this modification to the target world flag via the configured locator.
        /// </summary>
        public void Apply()
        {
            if (!IsValid)
            {
                Logger.LogWarning(LogSystems.WorldFlags, "Cannot apply modification - no valid flag assigned.");
                return;
            }

            if (flagLocator == null || !flagLocator.IsAvailable)
            {
                Logger.LogWarning(LogSystems.WorldFlags, "Cannot apply modification - flagLocator not available.");
                return;
            }

            var manager = flagLocator.Manager;
            if (isBoolFlag)
            {
                manager.SetBoolValue(boolFlag, boolValue);
                Logger.LogVerbose(LogSystems.WorldFlags, $"Set {boolFlag.FlagName} = {boolValue}");
            }
            else
            {
                switch (intOperation)
                {
                    case WorldFlagIntOperation.Set:
                        manager.SetIntValue(intFlag, intValue);
                        Logger.LogVerbose(LogSystems.WorldFlags, $"Set {intFlag.FlagName} = {intValue}");
                        break;
                    case WorldFlagIntOperation.Add:
                        manager.IncrementIntValue(intFlag, intValue);
                        var addRuntime = manager.GetIntFlag(intFlag);
                        Logger.LogVerbose(LogSystems.WorldFlags, $"{intFlag.FlagName} += {intValue} (now {addRuntime?.Value})");
                        break;
                    case WorldFlagIntOperation.Subtract:
                        manager.DecrementIntValue(intFlag, intValue);
                        var subRuntime = manager.GetIntFlag(intFlag);
                        Logger.LogVerbose(LogSystems.WorldFlags, $"{intFlag.FlagName} -= {intValue} (now {subRuntime?.Value})");
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a boolean flag modification.
        /// </summary>
        public static WorldFlagModification CreateBool(WorldFlagLocator_SO locator, WorldFlagBool_SO flag, bool value)
        {
            return new WorldFlagModification
            {
                flagLocator = locator,
                isBoolFlag = true,
                boolFlag = flag,
                boolValue = value
            };
        }

        /// <summary>
        /// Creates an integer flag modification.
        /// </summary>
        public static WorldFlagModification CreateInt(WorldFlagLocator_SO locator, WorldFlagInt_SO flag, WorldFlagIntOperation operation, int value)
        {
            return new WorldFlagModification
            {
                flagLocator = locator,
                isBoolFlag = false,
                intFlag = flag,
                intOperation = operation,
                intValue = value
            };
        }

        #endregion
    }
}
