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

### World State Flags (v1.5.0)

Self-contained ScriptableObject flags for tracking persistent game state:

- **WorldFlagBool_SO** - Boolean flags (met_king, chose_evil_path, dragon_defeated)
- **WorldFlagInt_SO** - Integer flags with min/max (reputation, kill_count, gold_donated)
- **WorldFlagManager** - Pure C# manager for world flag runtime instances
- **WorldFlagManagerBehaviour** - Thin MonoBehaviour wrapper for Unity lifecycle
- **WorldFlagLocator_SO** - Locator ScriptableObject for decoupled access
- **WorldFlagRegistry_SO** - Centralized registry for all flags with debug UI
- **WorldFlagModification** - Defines how to modify flags (Set, Add, Subtract)
- **ConditionWorldFlagBool_SO** - Check boolean flag values
- **ConditionWorldFlagInt_SO** - Check integer flags with comparisons (>=, <, ==, etc.)

### Event-Driven Flag Modifiers (v1.6.0)

Automatically modify world flags when game events fire:

- **WorldFlagEventModifierBase_SO** - Abstract base for event-driven modifiers
- **WorldFlagEventModifier_SO\<T\>** - Generic base with value filtering and comparison
- **WorldFlagEventModifierVoid_SO** - For parameterless events (OnGameStart, OnPause)
- **WorldFlagEventModifierID_SO** - For ID-based events (OnMonsterKilled, OnItemCollected)
- **WorldFlagEventModifierInt_SO** - For integer events with comparisons
- **WorldFlagEventModifierBool_SO** - For boolean events
- **WorldFlagEventModifierFloat_SO** - For float events with comparisons
- **WorldFlagEventModifierString_SO** - For string events
- **WorldFlagEventModifierManager** - MonoBehaviour to manage modifier lifecycles

## Assembly Structure

The package is organized into three assemblies for modularity:

| Assembly | Description | Dependencies |
|----------|-------------|--------------|
| `HelloDev.Conditions` | Core interfaces and base classes | Utils |
| `HelloDev.Conditions.Types` | Typed conditions (Bool, Int, Float, String, WorldFlag) | Conditions, WorldFlags, Events |
| `HelloDev.Conditions.WorldFlags` | World state flag system | Conditions, Events, IDs |

This allows you to reference only what you need. For example, if you only need the core condition interface without world flags, reference only `HelloDev.Conditions`.

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
        // Access runtime flags through the manager, then call methods on the runtime
        if (flagLocator.IsAvailable)
        {
            flagLocator.Manager.GetBoolFlag(hasMetKingData)?.SetValue(true);
            flagLocator.Manager.GetIntFlag(reputationData)?.Increment(10);
        }
    }

    public bool CanAccessRoyalQuest()
    {
        if (!flagLocator.IsAvailable) return false;

        var metKingFlag = flagLocator.Manager.GetBoolFlag(hasMetKingData);
        var repFlag = flagLocator.Manager.GetIntFlag(reputationData);

        bool metKing = metKingFlag?.Value ?? false;
        int rep = repFlag?.Value ?? 0;
        return metKing && rep >= 50;
    }
}
```

### 4. Set Up the World Flag Manager

For centralized flag management with events and runtime control:

1. Create a **WorldFlagLocator_SO**: **Create > HelloDev > Locators > World Flag Locator**
2. Create a **WorldFlagRegistry_SO**: **Create > HelloDev > Conditions > World Flag Registry**
3. Add your flags to the registry (or use "Find All WorldFlags in Project" button)
4. Add a **WorldFlagManagerBehaviour** component to your scene
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
        // Get the typed runtime, then call methods on it
        flagLocator.Manager.GetBoolFlag(metKingFlagData)?.SetValue(true);
    }

    public void OnCompleteQuest()
    {
        flagLocator.Manager.GetIntFlag(reputationFlagData)?.Increment(10);
    }

    void Start()
    {
        // Subscribe to manager events for flag changes
        flagLocator.Manager.OnBoolFlagChanged.AddListener(OnBoolFlagChanged);
        flagLocator.Manager.OnIntFlagChanged.AddListener(OnIntFlagChanged);
    }

    void OnBoolFlagChanged(WorldFlagBoolRuntime runtime, bool newValue)
    {
        if (runtime.Data == metKingFlagData && newValue)
            Debug.Log("Player met the king!");
    }

    void OnIntFlagChanged(WorldFlagIntRuntime runtime, int newValue, int oldValue)
    {
        if (runtime.Data == reputationFlagData)
            Debug.Log($"Reputation changed from {oldValue} to: {newValue}");
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
    [SerializeField] private WorldFlagBool_SO hasMetKingFlag;
    [SerializeField] private WorldFlagInt_SO reputationFlag;
    [SerializeField] private WorldFlagInt_SO killCountFlag;

    void Start()
    {
        // Access flags through the locator
        if (flagLocator.IsAvailable)
        {
            var manager = flagLocator.Manager;

            // Get typed runtimes
            var metKingRuntime = manager.GetBoolFlag(hasMetKingFlag);
            var reputationRuntime = manager.GetIntFlag(reputationFlag);
            var killCountRuntime = manager.GetIntFlag(killCountFlag);

            // Read values
            if (metKingRuntime != null)
            {
                Debug.Log($"Has met king: {metKingRuntime.Value}");
            }

            // Set flag values
            metKingRuntime?.SetValue(true);
            reputationRuntime?.SetValue(50);

            // Increment/decrement int flags
            killCountRuntime?.Increment(1);

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
var modification = WorldFlagModification.CreateBool(locator, sparedMerchantFlag, true);
modification.Apply();

var repBoost = WorldFlagModification.CreateInt(locator, reputationFlag, WorldFlagIntOperation.Add, 10);
repBoost.Apply();
```

### Event-Driven Flag Modifiers

Set world flags automatically when game events fire, independent of quest stage transitions.

#### Creating Event Modifiers

```
Assets > Create > HelloDev > Conditions > World Flags > Event Modifier (Void)
Assets > Create > HelloDev > Conditions > World Flags > Event Modifier (ID)
Assets > Create > HelloDev > Conditions > World Flags > Event Modifier (Int)
Assets > Create > HelloDev > Conditions > World Flags > Event Modifier (Bool)
Assets > Create > HelloDev > Conditions > World Flags > Event Modifier (Float)
Assets > Create > HelloDev > Conditions > World Flags > Event Modifier (String)
```

#### Use Cases

- **ID events:** `OnMonsterKilled(GoblinChiefID)` → Set `boss_defeated` flag
- **Void events:** `OnGameStart` → Reset flags to defaults
- **Int events:** `OnScoreChanged(100)` → Set milestone flag when score >= 100
- **Bool events:** `OnDoorOpened(true)` → Track door state

#### Setting Up Event Modifiers

1. Create a modifier: **Create > HelloDev > Conditions > World Flags > Event Modifier (ID)**
2. Configure the modifier:
   - Assign the **GameEvent** to listen to
   - Set **Filter By Value** and **Target Value** (optional filtering)
   - Set **Comparison Type** (Equals, GreaterThan, etc.)
   - Configure the **Modification** (flag to set, value, operation)
3. Add a **WorldFlagEventModifierManager** component to your scene
4. Add the modifier(s) to the manager's list

#### Example: Track Boss Kills

```csharp
// ScriptableObject assets needed:
// - GameEventID_SO: OnMonsterKilled
// - ID_SO: GoblinChiefID
// - WorldFlagBool_SO: GoblinChiefDefeated
// - WorldFlagEventModifierID_SO: configured with above references

// In your Monster script:
[SerializeField] private ID_SO monsterId;
[SerializeField] private GameEvent_SO<ID_SO> onMonsterKilled;

void OnDeath()
{
    onMonsterKilled.Raise(monsterId);
    // The modifier automatically sets GoblinChiefDefeated = true
    // when monsterId matches GoblinChiefID
}
```

#### Example: Score Milestone Tracking

```csharp
// WorldFlagEventModifierInt_SO configured with:
// - GameEvent: OnScoreChanged
// - Filter By Value: true
// - Target Value: 1000
// - Comparison: GreaterThanOrEqual
// - Modification: Set ReachedHighScore = true

// When OnScoreChanged fires with value >= 1000,
// the ReachedHighScore flag is automatically set
```

#### Managing Modifier Lifecycles

The `WorldFlagEventModifierManager` handles subscription/unsubscription automatically:

```csharp
// Add to scene - modifiers subscribe on OnEnable, unsubscribe on OnDisable
public class WorldFlagEventModifierManager : MonoBehaviour
{
    [SerializeField] private List<WorldFlagEventModifierBase_SO> modifiers;
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

### WorldFlagBool_SO (Data)
| Member | Description |
|--------|-------------|
| `DefaultValue` | Value when reset |
| `FlagId` | Unique identifier |

### WorldFlagBoolRuntime (accessed via Manager.GetBoolFlag)
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

### WorldFlagIntRuntime (accessed via Manager.GetIntFlag)
| Member | Description |
|--------|-------------|
| `Value` | Current integer value |
| `SetValue(int)` | Set the flag value |
| `Increment(int)` | Add to current value |
| `Decrement(int)` | Subtract from current value |
| `ResetToDefault()` | Reset to default value |
| `OnValueChanged` | Event fired when value changes |

### IWorldFlagManager (Interface)
| Member | Description |
|--------|-------------|
| `IsInitialized` | Whether the manager is initialized |
| `FlagCount` | Number of registered flags |
| `AllFlags` | All registered runtime flags |
| `GetFlag(flagData)` | Get any flag runtime by data |
| `GetFlagById(flagId)` | Get any flag runtime by ID |
| `GetBoolFlag(flagData)` | Get typed bool flag runtime |
| `GetIntFlag(flagData)` | Get typed int flag runtime |
| `RegisterFlag(flagData)` | Register a flag manually |
| `ResetAllFlags()` | Reset all flags to defaults |
| `ResetFlag(flagData)` | Reset specific flag to default |

### WorldFlagManager (Pure C# Implementation)
| Member | Description |
|--------|-------------|
| All `IWorldFlagManager` members | See interface above |
| `TryGetBoolValue(flagData, out value)` | Try get bool flag value |
| `TryGetIntValue(flagData, out value)` | Try get int flag value |
| `OnBoolFlagChanged` | Event for bool flag changes |
| `OnIntFlagChanged` | Event for int flag changes |
| `OnFlagRegistered` | Event when flag is registered |

### WorldFlagManagerBehaviour (MonoBehaviour Wrapper)
| Member | Description |
|--------|-------------|
| `SelfInitialize` | If true, self-initializes in Unity lifecycle. Set false for bootstrap mode. |
| `InitializationPriority` | Bootstrap priority (100 - Data Layer) |
| `ReceiveContext(GameContext)` | Called by bootstrap to provide service container for registration |
| `Manager` | The underlying WorldFlagManager instance |
| `Locator` | The locator this manager is registered with |

### WorldFlagModification
| Member | Description |
|--------|-------------|
| `IsBoolFlag` | True if modifying a boolean flag |
| `BoolFlag` / `IntFlag` | Target flag reference |
| `IntOperation` | Set, Add, or Subtract |
| `Apply()` | Execute the modification |
| `CreateBool()` | Factory for boolean modification |
| `CreateInt()` | Factory for integer modification |

### WorldFlagEventModifierBase_SO (Abstract)
| Member | Description |
|--------|-------------|
| `IsSubscribed` | Whether currently subscribed to event |
| `Modification` | The WorldFlagModification to apply |
| `IsValid` | Whether configuration is valid |
| `Subscribe()` | Subscribe to the game event |
| `Unsubscribe()` | Unsubscribe from the game event |

### WorldFlagEventModifier_SO\<T\> (Generic Typed Base)
| Member | Description |
|--------|-------------|
| `filterByValue` | If true, only trigger when value matches filter |
| `targetValue` | Value to compare against |
| `comparisonType` | How to compare (Equals, GreaterThan, etc.) |

### WorldFlagEventModifierVoid_SO
| Member | Description |
|--------|-------------|
| `gameEvent` | GameEventVoid_SO to listen to |

### WorldFlagEventModifierID_SO
| Member | Description |
|--------|-------------|
| `gameEvent` | GameEvent_SO\<ID_SO\> to listen to |

### WorldFlagEventModifierInt_SO
| Member | Description |
|--------|-------------|
| `gameEvent` | GameEventInt_SO to listen to |

### WorldFlagEventModifierBool_SO
| Member | Description |
|--------|-------------|
| `gameEvent` | GameEventBool_SO to listen to |

### WorldFlagEventModifierFloat_SO
| Member | Description |
|--------|-------------|
| `gameEvent` | GameEventFloat_SO to listen to |

### WorldFlagEventModifierString_SO
| Member | Description |
|--------|-------------|
| `gameEvent` | GameEventString_SO to listen to |

### WorldFlagEventModifierManager
| Member | Description |
|--------|-------------|
| `modifiers` | List of modifiers to manage |
| OnEnable | Subscribes all modifiers |
| OnDisable | Unsubscribes all modifiers |

## Dependencies

- com.hellodev.utils (1.5.0+) - Includes GameContext for bootstrap integration
- com.hellodev.events (1.1.0+)
- com.hellodev.ids (1.1.0+) - Required for WorldFlagEventModifierID_SO

## License

MIT License
