using UnityEngine;
using HelloDev.Conditions;
using HelloDev.Events;

namespace HelloDev.Conditions.Types
{
    [CreateAssetMenu(fileName = "String Condition", menuName = "HelloDev/Conditions/String Condition")]
    public class ConditionString_SO : ConditionEventDriven_SO<string>
    {
        [Header("Event Reference")] [SerializeField]
        private GameEventString_SO GameEventString;

        protected override void SubscribeToSpecificEvent()
        {
            if (GameEventString != null)
            {
                GameEventString.AddListener(OnEventTriggered);
            }
        }

        protected override void UnsubscribeFromSpecificEvent()
        {
            if (GameEventString != null)
            {
                GameEventString.RemoveListener(OnEventTriggered);
            }
        }

        protected override bool CompareValues(string eventValue, string targetValue, ComparisonType comparisonType)
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
            if (GameEventString != null)
                GameEventString.Raise(targetValue);
        }
    }
}