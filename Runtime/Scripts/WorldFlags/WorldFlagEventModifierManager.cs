using System.Collections.Generic;
using UnityEngine;

namespace HelloDev.Conditions.WorldFlags
{
    /// <summary>
    /// Manages subscription lifecycle for event modifiers.
    /// Add this component to a scene and assign modifiers to have them
    /// automatically subscribe/unsubscribe with the component's lifecycle.
    /// </summary>
    public class WorldFlagEventModifierManager : MonoBehaviour
    {
        [SerializeField] private List<WorldFlagEventModifierBase_SO> modifiers = new();

        void OnEnable()
        {
            foreach (var modifier in modifiers)
                modifier?.Subscribe();
        }

        void OnDisable()
        {
            foreach (var modifier in modifiers)
                modifier?.Unsubscribe();
        }
    }
}
