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
- **WorldFlagManager** - Centralized manager for world flag runtime instances
- **WorldFlagLocator_SO** - Locator ScriptableObject for decoupled access
- **WorldFlagRegistry_SO** - Registry for managing all flags in one place
- **WorldFlagModification** - Defines how to modify flags (Set, Add, Subtract)
- **ConditionWorldFlagBool_SO** - Check boolean flag values
- **ConditionWorldFlagInt_SO** - Check integer flags with comparisons (>=, <, ==, etc.)

## Getting Started

### 1. Install the Package

**Via Package Manager (Local):**
1. Open Unity Package Manager (Window > Package Manager)
2. Click "+" > "Add package from disk"
3. Navigate to this folder and select `package.json`

**Dependencies:** Ensure `com.hellodev.utils` and `com.hellodev.events` are installed.

### 2. Create Your First Condition

**Simple Boolean Condition:**
1. Create a GameEventBool_SO: **Create > HelloDev > Events > Bool Game Event**
2. Create a condition: **Create > HelloDev > Conditions > Bool Condition**
3. In the condition inspector, assign the event and set `targetValue` to `true`

**Use in Code:**
```csharp
using HelloDev.Conditions;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Condition_SO unlockCondition;

    public void TryOpen()
    {
        if (unlockCondition.Evaluate())
        {
            Open();
        }
        else
        {
            ShowLockedMessage();
        }
    }
}
```

### 3. Use World Flags for Persistent State

World flags are perfect for tracking game progress across scenes and play sessions.

**Create World Flags:**
1. **Create > HelloDev > World State > Bool Flag** (e.g., "HasMetKing")
2. **Create > HelloDev > World State > Int Flag** (e.g., "PlayerReputation")

**Use in Code:**
```csharp
using HelloDev.Conditions.WorldFlags;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private WorldFlagLocator_SO flagLocator;
    [SerializeField] private WorldFlagBool_SO hasMetKingData;
    [SerializeField] private WorldFlagInt_SO reputationData;

    public void OnMeetKing()
    {
        // Access runtime flags through the manager
        if (flagLocator.IsAvailable)
        {
            flagLocator.Manager.SetBoolValue(hasMetKingData, true);
            flagLocator.Manager.IncrementIntValue(reputationData, 10);
        }
    }

    public bool CanAccessRoyalQuest()
    {
        if (!flagLocator.IsAvailable) return false;

        flagLocator.Manager.TryGetBoolValue(hasMetKingData, out bool metKing);
        flagLocator.Manager.TryGetIntValue(reputationData, out int rep);
        return metKing && rep >= 50;
    }
}
```

### 4. Set Up the World Flag Manager (Optional)

For centralized flag management with events and runtime control:

1. Create a **WorldFlagLocator_SO**: **Create > HelloDev > Locators > World Flag Locator**
2. Create a **WorldFlagRegistry_SO**: **Create > HelloDev > World State > World Flag Registry**
3. Add your flags to the registry
4. Add a **WorldFlagManager** component to your scene
5. Assign the locator and registry references

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
    [SerializeField] private WorldFlagLocator_SO flagLocator;
    [SerializeField] private WorldFlagBool_SO metKingFlagData;
    [SerializeField] private WorldFlagInt_SO reputationFlagData;

    public void OnMeetKing()
    {
        // Modify flags through the manager
        flagLocator.Manager.SetBoolValue(metKingFlagData, true);
    }

    public void OnCompleteQuest()
    {
        flagLocator.Manager.IncrementIntValue(reputationFlagData, 10);
    }

    void Start()
    {
        // Subscribe to manager events for flag changes
        flagLocator.Manager.OnBoolFlagChanged.AddListener(OnBoolFlagChanged);
        flagLocator.Manager.OnIntFlagChanged.AddListener(OnIntFlagChanged);
    }

    void OnBoolFlagChanged(WorldFlagBool_SO flag, bool newValue)
    {
        if (flag == metKingFlagData && newValue)
            Debug.Log("Player met the king!");
    }

    void OnIntFlagChanged(WorldFlagInt_SO flag, int newValue)
    {
        if (flag == reputationFlagData)
            Debug.Log($"Reputation changed to: {newValue}");
    }
}
```

#### Using the World Flag Manager

For centralized flag management with bootstrap support:

```csharp
using HelloDev.Conditions.WorldFlags;

public class GameController : MonoBehaviour
{
    [SerializeField] private WorldFlagLocator_SO flagLocator;

    void Start()
    {
        // Access flags through the locator
        if (flagLocator.IsAvailable)
        {
            var manager = flagLocator.Manager;

            // Get flag values
            if (manager.TryGetBoolValue(hasMetKingFlag, out bool value))
            {
                Debug.Log($"Has met king: {value}");
            }

            // Set flag values
            manager.SetBoolValue(hasMetKingFlag, true);
            manager.SetIntValue(reputationFlag, 50);

            // Increment/decrement int flags
            manager.IncrementIntValue(killCountFlag, 1);

            // Subscribe to manager events
            manager.OnBoolFlagChanged.AddListener(HandleBoolFlagChanged);
            manager.OnIntFlagChanged.AddListener(HandleIntFlagChanged);
        }
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

### WorldFlagBool_SO (Data)
| Member | Description |
|--------|-------------|
| `DefaultValue` | Value when reset |
| `FlagId` | Unique identifier |

### WorldFlagBoolRuntime (accessed via Manager)
| Member | Description |
|--------|-------------|
| `Value` | Current boolean value |
| `SetValue(bool)` | Set the flag value |
| `SetTrue()` | Set flag to true |
| `SetFalse()` | Set flag to false |
| `Toggle()` | Toggle the flag |
| `ResetToDefault()` | Reset to default value |
| `OnValueChanged` | Event fired when value changes |
| `OnBecameTrue` | Event fired when flag becomes true |
| `OnBecameFalse` | Event fired when flag becomes false |

### WorldFlagInt_SO (Data)
| Member | Description |
|--------|-------------|
| `DefaultValue` | Value when reset |
| `MinValue` / `MaxValue` | Optional clamping bounds |
| `FlagId` | Unique identifier |

### WorldFlagIntRuntime (accessed via Manager)
| Member | Description |
|--------|-------------|
| `Value` | Current integer value |
| `SetValue(int)` | Set the flag value |
| `Increment(int)` | Add to current value |
| `Decrement(int)` | Subtract from current value |
| `ResetToDefault()` | Reset to default value |
| `OnValueChanged` | Event fired when value changes |

### WorldFlagManager
| Member | Description |
|--------|-------------|
| `SelfInitialize` | If true, self-initializes in Unity lifecycle. Set false for bootstrap mode. |
| `InitializationPriority` | Bootstrap priority (100 - Data Layer) |
| `Locator` | The locator this manager is registered with |
| `FlagCount` | Number of registered flags |
| `AllFlags` | All registered runtime flags |
| `GetBoolFlag(flagData)` | Get bool flag runtime instance |
| `GetIntFlag(flagData)` | Get int flag runtime instance |
| `TryGetBoolValue(flagData, out value)` | Try get bool flag value |
| `TryGetIntValue(flagData, out value)` | Try get int flag value |
| `SetBoolValue(flagData, value)` | Set bool flag value |
| `SetIntValue(flagData, value)` | Set int flag value |
| `IncrementIntValue(flagData, amount)` | Increment int flag |
| `DecrementIntValue(flagData, amount)` | Decrement int flag |
| `ResetAllFlags()` | Reset all flags to defaults |
| `OnBoolFlagChanged` | Event for bool flag changes |
| `OnIntFlagChanged` | Event for int flag changes |
| `OnFlagRegistered` | Event when flag is registered |

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

### v1.2.1 (2025-12-31)
**Bootstrap Support:**
- Added `SelfInitialize` property to `WorldFlagManager` for bootstrap mode support
- When `SelfInitialize = false`, manager waits for `GameBootstrap` to call `InitializeAsync`

### v1.2.0 (2025-12-28)
**World State Flags:**
- Added `WorldFlagBase_SO` abstract base class for world flags
- Added `WorldFlagBool_SO` for boolean state tracking
- Added `WorldFlagInt_SO` for integer state tracking with min/max bounds
- Added `WorldFlagManager` for centralized flag management
- Added `WorldFlagLocator_SO` for decoupled locator access
- Added `WorldFlagRegistry_SO` for flag auto-discovery
- Added `WorldFlagModification` for applying flag changes
- Added `ConditionWorldFlagBool_SO` for checking boolean flags
- Added `ConditionWorldFlagInt_SO` for checking integer flags with comparisons

**Design:**
- Self-contained ScriptableObject approach (not interface-based)
- Each flag stores its own value and fires events when changed
- Supports quest system integration via WorldFlagModification
- Bootstrap-compatible with `IBootstrapInitializable` on WorldFlagManager

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
