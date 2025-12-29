using HelloDev.Conditions.WorldFlags;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace HelloDev.Conditions.Types
{
    /// <summary>
    /// Condition that checks a WorldFlagBool_SO against an expected value.
    /// Subscribes to the flag's change events for reactive evaluation.
    ///
    /// Example usage:
    /// - Check if player chose the evil path: WorldFlag = "chose_evil_path", ExpectedValue = true
    /// - Check if player has NOT met the king: WorldFlag = "met_king", ExpectedValue = false
    /// </summary>
    [CreateAssetMenu(fileName = "ConditionWorldFlagBool", menuName = "HelloDev/Conditions/World Flag Bool")]
    public class ConditionWorldFlagBool_SO : Condition_SO, IConditionEventDriven
    {
        #region Serialized Fields

#if ODIN_INSPECTOR
        [BoxGroup("World Flag")]
        [PropertyOrder(0)]
        [Required]
#endif
        [SerializeField]
        [Tooltip("The world flag to check.")]
        private WorldFlagBool_SO worldFlag;

#if ODIN_INSPECTOR
        [BoxGroup("World Flag")]
        [PropertyOrder(1)]
#endif
        [SerializeField]
        [Tooltip("The expected value for this condition to be true.")]
        private bool expectedValue = true;

        #endregion

        #region Private Fields

        private System.Action _onConditionMet;
        private bool _isSubscribed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the world flag being checked.
        /// </summary>
        public WorldFlagBool_SO WorldFlag => worldFlag;

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        public bool ExpectedValue => expectedValue;

        #endregion

        #region Condition Implementation

        /// <summary>
        /// Evaluates the condition by checking the world flag's current value.
        /// </summary>
        public override bool Evaluate()
        {
            if (worldFlag == null) return false;

            bool result = worldFlag.Value == expectedValue;
            return IsInverted ? !result : result;
        }

        #endregion

        #region IConditionEventDriven Implementation

        /// <summary>
        /// Subscribes to the world flag's value changed event.
        /// </summary>
        public void SubscribeToEvent(System.Action onConditionMet)
        {
            if (_isSubscribed) return;
            if (worldFlag == null) return;

            _onConditionMet = onConditionMet;
            worldFlag.OnValueChanged.AddListener(HandleValueChanged);
            _isSubscribed = true;
        }

        /// <summary>
        /// Unsubscribes from the world flag's value changed event.
        /// </summary>
        public void UnsubscribeFromEvent()
        {
            if (!_isSubscribed) return;
            if (worldFlag == null) return;

            worldFlag.OnValueChanged.RemoveListener(HandleValueChanged);
            _isSubscribed = false;
            _onConditionMet = null;
        }

        /// <summary>
        /// Forces the condition to be fulfilled by setting the world flag.
        /// </summary>
        public void ForceFulfillCondition()
        {
            if (worldFlag != null)
            {
                worldFlag.SetValue(expectedValue);
            }
        }

        #endregion

        #region Private Methods

        private void HandleValueChanged(bool newValue)
        {
            if (Evaluate())
            {
                _onConditionMet?.Invoke();
            }
        }

        #endregion

        #region Lifecycle

        protected override void OnScriptableObjectReset()
        {
            UnsubscribeFromEvent();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvent();
        }

        #endregion

#if ODIN_INSPECTOR && UNITY_EDITOR
        #region Debug

        [BoxGroup("Debug")]
        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(100)]
        private bool CurrentFlagValue => worldFlag != null ? worldFlag.Value : false;

        [BoxGroup("Debug")]
        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(101)]
        private bool EvaluationResult => Evaluate();

        [BoxGroup("Debug")]
        [Button("Evaluate Now")]
        [PropertyOrder(102)]
        private void DebugEvaluate()
        {
            Debug.Log($"[{name}] Evaluation result: {Evaluate()}");
        }

        [BoxGroup("Debug")]
        [Button("Force Fulfill")]
        [PropertyOrder(103)]
        private void DebugForceFulfill() => ForceFulfillCondition();

        #endregion
#endif
    }
}
