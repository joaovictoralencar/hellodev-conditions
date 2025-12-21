using UnityEngine;
using HelloDev.Conditions;
using HelloDev.Events;

namespace HelloDev.Conditions.Types
{
    [CreateAssetMenu(fileName = "Float Condition", menuName = "HelloDev/Conditions/Float Condition")]
    public class ConditionFloat_SO : ConditionEventDriven_SO<float>
    {
        [Header("Event Reference")]
        [SerializeField] private GameEventFloat_SO GameEventFloat;

        protected override void SubscribeToSpecificEvent()
        {
            if (GameEventFloat != null)
            {
                GameEventFloat.AddListener(OnEventTriggered);
            }
        }

        protected override void UnsubscribeFromSpecificEvent()
        {
            if (GameEventFloat != null)
            {
                GameEventFloat.RemoveListener(OnEventTriggered);
            }
        }

        protected override bool CompareValues(float eventValue, float targetValue, ComparisonType comparisonType)
        {
            return comparisonType switch
            {
                ComparisonType.Equals => Mathf.Approximately(eventValue, targetValue),
                ComparisonType.NotEquals => !Mathf.Approximately(eventValue, targetValue),
                ComparisonType.GreaterThan => eventValue > targetValue,
                ComparisonType.GreaterThanOrEqual => eventValue >= targetValue,
                ComparisonType.LessThan => eventValue < targetValue,
                ComparisonType.LessThanOrEqual => eventValue <= targetValue,
                _ => false
            };
        }

        protected override void DebugForceFulfillCondition()
        {
            if (GameEventFloat != null)
                GameEventFloat.Raise(targetValue);
        }
    }
}