using HelloDev.Events;
using UnityEngine;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Event modifier for parameterless void events.
    /// Use for events like OnGameStart, OnPause, OnLevelComplete, etc.
    /// </summary>
    [CreateAssetMenu(menuName = "HelloDev/Conditions/World Flags/Event Modifier (Void)")]
    public class WorldFlagEventModifierVoid_SO : WorldFlagEventModifierBase_SO
    {
        [SerializeField] private GameEventVoid_SO gameEvent;

        /// <summary>
        /// Whether this modifier has valid configuration.
        /// </summary>
        public override bool IsValid => gameEvent != null && modification.IsValid;

        /// <summary>
        /// Subscribes to the void event.
        /// </summary>
        public override void Subscribe()
        {
            if (gameEvent == null || IsSubscribed) return;
            gameEvent.AddListener(OnEventRaised);
            IsSubscribed = true;
        }

        /// <summary>
        /// Unsubscribes from the void event.
        /// </summary>
        public override void Unsubscribe()
        {
            if (gameEvent == null || !IsSubscribed) return;
            gameEvent.RemoveListener(OnEventRaised);
            IsSubscribed = false;
        }

        private void OnEventRaised() => ApplyModification();
    }
}
