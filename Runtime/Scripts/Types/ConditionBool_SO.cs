using UnityEngine;
using HelloDev.Conditions;
using HelloDev.Events;

namespace HelloDev.Conditions.Types
{
    [CreateAssetMenu(fileName = "Bool Condition", menuName = "HelloDev/Conditions/Bool Condition")]
    public class ConditionBool_SO : ConditionEventDriven_SO<bool>
    {
        [Header("Event Reference")]
        [SerializeField] private GameEventBool_SO GameEventBool;

        protected override void SubscribeToSpecificEvent()
        {
            if (GameEventBool != null)
            {
                GameEventBool.AddListener(OnEventTriggered);
            }
        }

        protected override void UnsubscribeFromSpecificEvent()
        {
            if (GameEventBool != null)
            {
                GameEventBool.RemoveListener(OnEventTriggered);
            }
        }

        protected override bool CompareValues(bool eventValue, bool targetValue, ComparisonType comparisonType)
        {
            return comparisonType switch
            {
                ComparisonType.Equals => eventValue == targetValue,
                ComparisonType.NotEquals => eventValue != targetValue,
                _ => false
            };
        }
        protected override void DebugForceFulfillCondition()
        {
            if (GameEventBool != null)
                GameEventBool.Raise(targetValue);
        }
    }
}