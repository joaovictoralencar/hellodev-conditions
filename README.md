# HelloDev Conditions

A modular condition system for Unity games. Supports event-driven conditions, composite conditions, and self-contained world state flags for implementing prerequisites, triggers, and game logic.

## Features

- **ICondition** - Base interface with `Evaluate()` and inversion support
- **IConditionEventDriven** - Extended interface for event-driven conditions
- **Condition_SO** - Abstract ScriptableObject base implementing ICondition
- **ConditionEventDriven_SO\<T\>** - Generic base for typed event-driven conditions with cached evaluation
- **CompositeCondition_SO** - Combine multiple conditions with AND/OR logic (implements IConditionEventDriven)
- **Typed Conditions** - ConditionBool_SO, ConditionInt_SO, ConditionFloat_SO, ConditionString_SO
- **ComparisonType** - Equals, NotEquals, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual
- **HasBeenEvaluated** - Check if an event-driven condition has been triggered

### World State Flags (v1.2.0)

Self-contained ScriptableObject flags for tracking persistent game state:

- **WorldFlagBool_SO** - Boolean flags (met_king, chose_evil_path, dragon_defeated)
- **WorldFlagInt_SO** - Integer flags with min/max (reputation, kill_count, gold_donated)
- **WorldFlagModification** - Defines how to modify flags (Set, Add, Subtract)
- **ConditionWorldFlagBool_SO** - Check boolean flag values
- **ConditionWorldFlagInt_SO** - Check integer flags with comparisons (>=, <, ==, etc.)

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

### World State Flags

World flags are self-contained ScriptableObjects that store persistent game state. Each flag stores its own value and fires events when changed.

#### Creating World Flags

```
Assets > Create > HelloDev > World State > Bool Flag
Assets > Create > HelloDev > World State > Int Flag
```

#### Using World Flags in Code

```csharp
using HelloDev.Conditions.WorldFlags;

public class QuestGiver : MonoBehaviour
{
    [SerializeField] private WorldFlagBool_SO metKingFlag;
    [SerializeField] private WorldFlagInt_SO reputationFlag;

    public void OnMeetKing()
    {
        metKingFlag.SetTrue();
    }

    public void OnCompleteQuest()
    {
        reputationFlag.Increment(10); // Add 10 to reputation
    }

    void Start()
    {
        // Subscribe to flag changes
        metKingFlag.OnBecameTrue.AddListener(OnMetKing);
        reputationFlag.OnValueChanged.AddListener(OnReputationChanged);
    }
}
```

#### World Flag Conditions

Check world flag values in conditions:

```csharp
// ConditionWorldFlagBool_SO - checks if a boolean flag equals expected value
// ConditionWorldFlagInt_SO - checks integer flag with comparison (>=, <, ==, etc.)

// Use in quest start conditions, dialogue prerequisites, etc.
[SerializeField] private ConditionWorldFlagBool_SO metKingCondition;
[SerializeField] private ConditionWorldFlagInt_SO highReputationCondition;

if (metKingCondition.Evaluate() && highReputationCondition.Evaluate())
{
    // Player has met the king AND has high reputation
    UnlockSpecialQuest();
}
```

#### World Flag Modifications

Apply changes to world flags (used by quest system for branch consequences):

```csharp
using HelloDev.Conditions.WorldFlags;

// Create modification in code
var modification = WorldFlagModification.CreateBool(sparedMerchantFlag, true);
modification.Apply();

var repBoost = WorldFlagModification.CreateInt(reputationFlag, WorldFlagIntOperation.Add, 10);
repBoost.Apply();
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

### WorldFlagBool_SO
| Member | Description |
|--------|-------------|
| `Value` | Current boolean value |
| `DefaultValue` | Value when reset |
| `SetValue(bool)` | Set the flag value |
| `SetTrue()` | Set flag to true |
| `SetFalse()` | Set flag to false |
| `Toggle()` | Toggle the flag |
| `ResetToDefault()` | Reset to default value |
| `OnValueChanged` | Event fired when value changes |
| `OnBecameTrue` | Event fired when flag becomes true |
| `OnBecameFalse` | Event fired when flag becomes false |

### WorldFlagInt_SO
| Member | Description |
|--------|-------------|
| `Value` | Current integer value |
| `DefaultValue` | Value when reset |
| `MinValue` / `MaxValue` | Optional clamping bounds |
| `SetValue(int)` | Set the flag value |
| `Increment(int)` | Add to current value |
| `Decrement(int)` | Subtract from current value |
| `ResetToDefault()` | Reset to default value |
| `OnValueChanged` | Event fired when value changes |

### WorldFlagModification
| Member | Description |
|--------|-------------|
| `IsBoolFlag` | True if modifying a boolean flag |
| `BoolFlag` / `IntFlag` | Target flag reference |
| `IntOperation` | Set, Add, or Subtract |
| `Apply()` | Execute the modification |
| `CreateBool()` | Factory for boolean modification |
| `CreateInt()` | Factory for integer modification |

## Dependencies

- com.hellodev.utils (1.1.0+)
- com.hellodev.events (1.1.0+)

## Changelog

### v1.2.0 (2025-12-28)
**World State Flags:**
- Added `WorldFlagBase_SO` abstract base class for world flags
- Added `WorldFlagBool_SO` for boolean state tracking
- Added `WorldFlagInt_SO` for integer state tracking with min/max bounds
- Added `WorldFlagModification` for applying flag changes
- Added `ConditionWorldFlagBool_SO` for checking boolean flags
- Added `ConditionWorldFlagInt_SO` for checking integer flags with comparisons

**Design:**
- Self-contained ScriptableObject approach (not interface-based)
- Each flag stores its own value and fires events when changed
- Supports quest system integration via WorldFlagModification

### v1.1.0 (2025-12-21)
**Bug Fixes:**
- CompositeCondition_SO now properly initializes condition states before evaluation
- CompositeCondition_SO.Evaluate() works correctly even when not subscribed

**New Features:**
- CompositeCondition_SO now implements IConditionEventDriven
- ConditionEventDriven_SO caches last evaluation result
- Added `HasBeenEvaluated` property

### v1.0.0
- Initial release

## License

MIT License
