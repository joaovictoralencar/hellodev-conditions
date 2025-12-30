using System;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Abstract base class for world flag runtime instances.
    /// Runtime instances hold mutable state while the SO holds immutable config.
    /// </summary>
    public abstract class WorldFlagRuntime
    {
        /// <summary>
        /// Gets the data asset for this flag.
        /// </summary>
        public WorldFlagBase_SO Data { get; }

        /// <summary>
        /// Gets the unique identifier for this flag.
        /// </summary>
        public string FlagId => Data.FlagId;

        /// <summary>
        /// Gets the developer-friendly name for this flag.
        /// </summary>
        public string FlagName => Data.FlagName;

        protected WorldFlagRuntime(WorldFlagBase_SO data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Resets the flag to its default value.
        /// </summary>
        public abstract void ResetToDefault();

        /// <summary>
        /// Gets a string representation of the current value.
        /// </summary>
        public abstract string GetValueAsString();
    }
}
