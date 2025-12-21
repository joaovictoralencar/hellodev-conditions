namespace HelloDev.Conditions
{
    /// <summary>
    /// Base interface for all conditions. Supports passive evaluation and inversion.
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// When true, the evaluation result is inverted (NOT logic).
        /// </summary>
        bool IsInverted { get; set; }

        /// <summary>
        /// Evaluates the condition and returns the result.
        /// For event-driven conditions, returns the last cached result.
        /// </summary>
        bool Evaluate();
    }

    /// <summary>
    /// Extended interface for event-driven conditions that react to game events.
    /// </summary>
    public interface IConditionEventDriven : ICondition
    {
        /// <summary>
        /// Subscribes to the underlying event. The callback is invoked when the condition is met.
        /// </summary>
        /// <param name="onConditionMet">Callback to invoke when condition evaluates to true.</param>
        void SubscribeToEvent(System.Action onConditionMet);

        /// <summary>
        /// Unsubscribes from the underlying event.
        /// </summary>
        void UnsubscribeFromEvent();

        /// <summary>
        /// Forces the condition to be fulfilled (for debugging/testing).
        /// </summary>
        void ForceFulfillCondition();
    }

    /// <summary>
    /// Comparison operators for numeric and comparable conditions.
    /// </summary>
    public enum ComparisonType
    {
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }

    /// <summary>
    /// Logical operators for combining multiple conditions.
    /// </summary>
    public enum CompositeOperator
    {
        And,
        Or
    }
}
