# HelloDev Conditions

A modular condition system for Unity games. Supports event-driven and composite conditions for implementing prerequisites, triggers, and game logic.

## Features

- **ICondition** - Base interface with `Evaluate()` and inversion support
- **IConditionEventDriven** - Extended interface for event-driven conditions
- **Condition_SO** - Abstract ScriptableObject base implementing ICondition
- **ConditionEventDriven_SO\<T\>** - Generic base for typed event-driven conditions with cached evaluation
- **CompositeCondition_SO** - Combine multiple conditions with AND/OR logic (implements IConditionEventDriven)
- **Typed Conditions** - ConditionBool_SO, ConditionInt_SO, ConditionFloat_SO, ConditionString_SO
- **ComparisonType** - Equals, NotEquals, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual
- **HasBeenEvaluated** - Check if an event-driven condition has been triggered

## Installation

### Via Package Manager (Local)
1. Open Unity Package Manager
2. Click "+" > "Add package from disk"
3. Navigate to this folder and select `package.json`

## Usage

### Creating a Condition

```
Assets > Create > HelloDev > Conditions > [Type] Condition
```

### Using Conditions in Code

```csharp
using HelloDev.Conditions;

public class Door : MonoBehaviour
{
    [SerializeField] private Condition_SO unlockCondition;

    public void TryOpen()
    {
        if (unlockCondition.Evaluate())
        {
            Open();
        }
    }
}
```

### Event-Driven Conditions

```csharp
using HelloDev.Conditions;

[SerializeField] private Condition_SO triggerCondition;

void Start()
{
    if (triggerCondition is IConditionEventDriven eventCondition)
    {
        eventCondition.SubscribeToEvent(OnConditionMet);
    }
}

void OnDestroy()
{
    if (triggerCondition is IConditionEventDriven eventCondition)
    {
        eventCondition.UnsubscribeFromEvent();
    }
}

void OnConditionMet()
{
    Debug.Log("Condition was fulfilled!");
}
```

### Checking Cached Evaluation State

```csharp
// For event-driven conditions, Evaluate() returns the cached result
if (triggerCondition is ConditionEventDriven_SO<int> intCondition)
{
    // Check if condition has ever been evaluated
    if (intCondition.HasBeenEvaluated)
    {
        bool lastResult = intCondition.Evaluate();
    }
}
```

### Composite Conditions

Combine multiple conditions using AND/OR logic:
1. Create > HelloDev > Conditions > Composite Condition
2. Add child conditions to the list
3. Select operator (And/Or)

```csharp
// CompositeCondition_SO also implements IConditionEventDriven
[SerializeField] private CompositeCondition_SO allRequirements;

void Start()
{
    // Subscribe to be notified when ALL (or ANY, depending on operator) conditions are met
    allRequirements.SubscribeToEvent(OnAllRequirementsMet);
}
```

### Creating Custom Condition Types

```csharp
using HelloDev.Conditions;
using HelloDev.Events;
using UnityEngine;

[CreateAssetMenu(menuName = "HelloDev/Conditions/Custom Condition")]
public class ConditionCustom_SO : ConditionEventDriven_SO<float>
{
    [SerializeField] private GameEventFloat_SO gameEvent;

    protected override void SubscribeToSpecificEvent()
    {
        if (gameEvent != null)
            gameEvent.AddListener(OnEventTriggered);
    }

    protected override void UnsubscribeFromSpecificEvent()
    {
        if (gameEvent != null)
            gameEvent.RemoveListener(OnEventTriggered);
    }

    protected override bool CompareValues(float eventValue, float targetValue, ComparisonType comparisonType)
    {
        return comparisonType switch
        {
            ComparisonType.Equals => Mathf.Approximately(eventValue, targetValue),
            ComparisonType.GreaterThan => eventValue > targetValue,
            // ... other comparisons
            _ => false
        };
    }

    protected override void DebugForceFulfillCondition()
    {
        if (gameEvent != null)
            gameEvent.Raise(targetValue);
    }
}
```

## API Reference

### ICondition
| Member | Description |
|--------|-------------|
| `IsInverted` | When true, result is inverted (NOT logic) |
| `Evaluate()` | Returns condition result |

### IConditionEventDriven
| Member | Description |
|--------|-------------|
| `SubscribeToEvent(callback)` | Subscribe to condition fulfillment |
| `UnsubscribeFromEvent()` | Unsubscribe from condition |
| `ForceFulfillCondition()` | Force condition to be met (debug) |

### ConditionEventDriven_SO\<T\>
| Member | Description |
|--------|-------------|
| `Evaluate()` | Returns cached result from last event |
| `HasBeenEvaluated` | True if condition has been triggered |

### CompositeCondition_SO
| Member | Description |
|--------|-------------|
| `Conditions` | Read-only list of child conditions |
| `Operator` | And/Or logic for combining |
| `Cleanup()` | Unsubscribe and clear state |

## Dependencies

- com.hellodev.utils (1.1.0+)
- com.hellodev.events (1.1.0+)

## Changelog

### v1.1.0 (2025-12-21)
**Bug Fixes:**
- CompositeCondition_SO now properly initializes condition states before evaluation
- CompositeCondition_SO.Evaluate() works correctly even when not subscribed

**New Features:**
- CompositeCondition_SO now implements IConditionEventDriven
- ConditionEventDriven_SO caches last evaluation result (Evaluate() returns cached value)
- Added `HasBeenEvaluated` property to check if condition was triggered

**Robustness:**
- Added null checks in all DebugForceFulfillCondition methods
- Proper state reset in OnScriptableObjectReset

**Documentation:**
- Added XML documentation to all interfaces, classes, and methods

**Package:**
- Updated Unity version to 6000.3
- Updated dependencies to 1.1.0

### v1.0.0
- Initial release

## License

MIT License
