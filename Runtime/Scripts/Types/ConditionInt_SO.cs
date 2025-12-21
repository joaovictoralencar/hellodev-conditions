using UnityEngine;
using HelloDev.Conditions;
using HelloDev.Events;

namespace HelloDev.Conditions.Types
{
    [CreateAssetMenu(fileName = "Int Condition", menuName = "HelloDev/Conditions/Int Condition")]
    public class ConditionInt_SO : ConditionEventDriven_SO<int>
    {
        [Header("Event Reference")]
        [SerializeField] private GameEventInt_SO GameEventInt;

        protected override void SubscribeToSpecificEvent()
        {
            if (GameEventInt != null)
            {
                GameEventInt.AddListener(OnEventTriggered);
            }
        }

        protected override void UnsubscribeFromSpecificEvent()
        {
            if (GameEventInt != null)
            {
                GameEventInt.RemoveListener(OnEventTriggered);
            }
        }

        protected override bool CompareValues(int eventValue, int targetValue, ComparisonType comparisonType)
        {
            return comparisonType switch
            {
                ComparisonType.Equals => eventValue == targetValue,
                ComparisonType.NotEquals => eventValue != targetValue,
                ComparisonType.GreaterThan => eventValue > targetValue,
                ComparisonType.GreaterThanOrEqual => eventValue >= targetValue,
                ComparisonType.LessThan => eventValue < targetValue,
                ComparisonType.LessThanOrEqual => eventValue <= targetValue,
                _ => false
            };
        }
        protected override void DebugForceFulfillCondition()
        {
            if (GameEventInt != null)
                GameEventInt.Raise(targetValue);
        }
    }
}