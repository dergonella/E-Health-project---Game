# Level 0 & Level 0.1 Implementation Guide

## Overview

This guide documents the implementation of three new features:
1. **Level 0**: Win condition at exactly 2000 points
2. **Level 0.1**: Timed challenge - reach 2000 points in 30 seconds
3. **Points-to-Money Conversion**: Excess points above 2000 convert to currency

---

## Task 1: Level 0 - Win at 2000 Points

### Implementation

The win condition is now automatically checked every time the score updates.

**File Modified**: [GameManager.cs](Assets/Scripts/GameManager.cs:59-73)

**Key Code**:
```csharp
private void CheckWinCondition()
{
    if (gameOver) return;

    // Get target score from current level
    if (LevelManager.Instance != null)
    {
        LevelData currentLevel = LevelManager.Instance.GetCurrentLevelData();
        if (currentLevel != null && currentScore >= currentLevel.targetScore)
        {
            Debug.Log($"Level Complete! Reached target score of {currentLevel.targetScore}");
            GameOver(true); // Win!
        }
    }
}
```

**How It Works**:
- Every time `UpdateScore()` is called, it checks if the player has reached the target score
- For Level 0, target score is 2000 points
- Once the player reaches 2000 points, the level immediately ends with a win

**Testing**:
1. Load Level 0 scene (Level0_Core)
2. Collect shards until you reach 2000 points
3. Game should immediately trigger win condition and show win screen

---

## Task 2: Level 0.1 - Timed Challenge

### Overview

Level 0.1 is a timed challenge where players must:
- Reach 2000 points within 30 seconds
- A countdown timer displays on screen (MM:SS.MS format)
- Timer changes color based on remaining time:
  - **White**: More than 10 seconds remaining
  - **Yellow**: 10-5 seconds remaining
  - **Red**: Less than 5 seconds remaining

### New Components

#### 1. TimedLevelManager.cs

**Location**: [Assets/Scripts/TimedLevelManager.cs](Assets/Scripts/TimedLevelManager.cs)

**Purpose**: Manages countdown timer, displays time, checks win/loss conditions

**Inspector Settings**:
```
Time Limit: 30 seconds
Enable Timer: ✓ (checked)
Timer Text: [Assign TextMeshProUGUI component from Canvas]
Normal Color: White
Warning Color: Yellow (10s threshold)
Critical Color: Red (5s threshold)
```

**Key Methods**:
- `StartTimer()`: Initializes timer when level begins
- `UpdateTimerDisplay()`: Updates UI text and color based on remaining time
- `TimeUp()`: Called when timer expires - triggers game over if target not reached
- `OnLevelComplete()`: Called when player reaches 2000 points - stops timer and converts points to money

#### 2. LevelManager Configuration

**File Modified**: [LevelManager.cs](Assets/Scripts/LevelManager.cs:70-87)

**New Level Data** (Level 0.1):
```csharp
levels[1] = new LevelData
{
    levelName = "Timed Challenge",
    sceneName = "Level0_1_TimedChallenge",
    description = "Reach 2000 points in 30 seconds! Excess points = Money!",
    targetScore = 2000,
    survivalTime = 0f,
    hasHealthSystem = false,
    hasShield = false,
    hasSlowMotion = false,
    hasFire = false,
    hasPoison = false,
    hasMines = false,
    cobraInstantKill = true,
    hasTimedChallenge = true,           // NEW
    timeLimitSeconds = 30f,             // NEW
    convertExcessPointsToMoney = true   // NEW
};
```

### Scene Setup for Level 0.1

To create the Level 0.1 scene:

1. **Duplicate Level 0 Scene**:
   - In Unity, right-click Level0_Core scene
   - Duplicate
   - Rename to: `Level0_1_TimedChallenge`

2. **Add TimedLevelManager**:
   - Create empty GameObject: "TimedLevelManager"
   - Add Component: TimedLevelManager script
   - Configure settings:
     - Time Limit: 30
     - Enable Timer: ✓

3. **Create Timer UI**:
   - In Canvas, create: UI → Text - TextMeshPro
   - Name it: "TimerText"
   - Configure:
     - Font Size: 48
     - Position: Top center of screen
     - Anchor: Top Center
     - Text: "00:30.00" (placeholder)

4. **Link Timer Text**:
   - Select TimedLevelManager GameObject
   - Drag TimerText to "Timer Text" field in Inspector

5. **Add PointsToMoneyConverter**:
   - Create empty GameObject: "PointsToMoneyConverter"
   - Add Component: PointsToMoneyConverter script
   - Configure:
     - Points To Money Ratio: 100 (100 points = 1 coin)
     - Excess Points Multiplier: 1.0
     - Show Debug Logs: ✓

6. **Test the Scene**:
   - Press Play
   - Timer should start counting down from 30 seconds
   - Collect shards to reach 2000 points
   - If time runs out before 2000: Game Over (loss)
   - If you reach 2000 before time: Win + Money conversion

---

## Task 3: Points-to-Money Conversion

### Overview

When players complete Level 0.1 with more than 2000 points, excess points are converted to in-game currency.

### New Component: PointsToMoneyConverter.cs

**Location**: [Assets/Scripts/PointsToMoneyConverter.cs](Assets/Scripts/PointsToMoneyConverter.cs)

**Purpose**: Converts score above target into money using CurrencyManager

**Conversion Formula**:
```
Base Money = Target Score / Conversion Ratio
Bonus Money = (Excess Points / Conversion Ratio) * Excess Multiplier
Total Money = Base Money + Bonus Money
```

**Example**:
- Player score: 2500 points
- Target score: 2000 points
- Excess points: 500 points
- Conversion ratio: 100 (100 points = 1 coin)
- Excess multiplier: 1.0

**Calculation**:
```
Base Money = 2000 / 100 = 20 coins
Bonus Money = (500 / 100) * 1.0 = 5 coins
Total Money = 20 + 5 = 25 coins awarded
```

**Key Methods**:
- `ConvertAndAwardMoney(finalScore, targetScore, levelWon)`: Converts score to money and awards it
- `CalculatePotentialMoney(currentScore, targetScore)`: Preview how much money would be earned (for UI)

### Integration with CurrencyManager

The system integrates with existing [CurrencyManager.cs](Assets/Scripts/CurrencyManager.cs):
- Money is added using `CurrencyManager.Instance.AddMoney(amount)`
- Money persists across scenes using PlayerPrefs
- Players can spend money in the shop/upgrade system

### When Conversion Happens

**Automatic Trigger**:
- When player reaches 2000 points in Level 0.1
- Timer must still be running (haven't run out of time)
- `TimedLevelManager.OnLevelComplete()` calls `ConvertPointsToMoney()`
- `PointsToMoneyConverter.ConvertAndAwardMoney()` handles the conversion

**Manual Trigger** (for testing):
```csharp
PointsToMoneyConverter converter = FindObjectOfType<PointsToMoneyConverter>();
converter.ConvertAndAwardMoney(2500, 2000, true);
```

---

## Summary of Files Created/Modified

### Files Created:
1. **[TimedLevelManager.cs](Assets/Scripts/TimedLevelManager.cs)** - NEW
   - Countdown timer system for Level 0.1
   - Displays timer on screen with color changes
   - Triggers win/loss based on time and score

2. **[PointsToMoneyConverter.cs](Assets/Scripts/PointsToMoneyConverter.cs)** - NEW
   - Converts excess points to currency
   - Integrates with CurrencyManager
   - Configurable conversion rates

### Files Modified:
1. **[GameManager.cs](Assets/Scripts/GameManager.cs:59-73)** - MODIFIED
   - Added `CheckWinCondition()` method
   - Automatically checks win condition on score update
   - Level 0 now ends at 2000 points

2. **[LevelManager.cs](Assets/Scripts/LevelManager.cs)** - MODIFIED
   - Added 3 new fields to LevelData:
     - `hasTimedChallenge` (bool)
     - `timeLimitSeconds` (float)
     - `convertExcessPointsToMoney` (bool)
   - Expanded levels array from 5 to 6 entries
   - Added Level 0.1 configuration (index 1)
   - Shifted other levels to indices 2-5

---

## Testing Checklist

### Level 0 Testing:
- [ ] Load Level 0 scene
- [ ] Collect shards to reach 2000 points
- [ ] Verify game immediately ends with win screen
- [ ] Check console for: "Level Complete! Reached target score of 2000"

### Level 0.1 Testing:
- [ ] Load Level 0.1 scene (Level0_1_TimedChallenge)
- [ ] Verify timer starts at 30:00.00 and counts down
- [ ] Verify timer color changes:
  - [ ] White (30-10 seconds)
  - [ ] Yellow (10-5 seconds)
  - [ ] Red (5-0 seconds)
- [ ] Test win scenario (reach 2000 before time expires):
  - [ ] Level ends with win
  - [ ] Points convert to money
  - [ ] Check console for money conversion log
- [ ] Test loss scenario (time expires before 2000):
  - [ ] Timer shows "TIME'S UP!"
  - [ ] Game Over triggered
  - [ ] Return to menu
- [ ] Test money conversion:
  - [ ] Score 2500 points before time expires
  - [ ] Check console for: "Total Awarded: X coins"
  - [ ] Verify CurrencyManager balance increased

### Points-to-Money Testing:
- [ ] Complete Level 0.1 with 2500 points
- [ ] Check console logs:
  - [ ] Final Score: 2500
  - [ ] Target Score: 2000
  - [ ] Excess Points: 500
  - [ ] Base Money: 20 coins
  - [ ] Bonus Money: 5 coins
  - [ ] Total Awarded: 25 coins
- [ ] Verify money persists after returning to menu

---

## Debug Console Output

### Expected Logs for Level 0.1 Success:

```
TimedLevelManager: Timer started - 30 seconds
ShardSpawner: Spawned 10 shards
Player collision detected with: Shard_0, Tag: Shard
ShardController found! Collecting shard...
Level Complete! Reached target score of 2000
TimedLevelManager: Level completed with 12.34 seconds remaining!
PointsToMoneyConverter: Conversion Complete!
  - Final Score: 2300
  - Target Score: 2000
  - Excess Points: 300
  - Base Money: 20 coins (from target score)
  - Bonus Money: 3 coins (from excess points)
  - Total Awarded: 23 coins
  - Player Total Money: 23 coins
```

### Expected Logs for Level 0.1 Failure:

```
TimedLevelManager: Timer started - 30 seconds
ShardSpawner: Spawned 10 shards
TimedLevelManager: Time's up!
Time's up! Score: 1500/2000 - Game Over
```

---

## Configuration Options

### TimedLevelManager Settings:
- **Time Limit**: Adjust duration (default: 30s)
- **Warning Threshold**: When timer turns yellow (default: 10s)
- **Critical Threshold**: When timer turns red (default: 5s)
- **Colors**: Customize normal/warning/critical colors

### PointsToMoneyConverter Settings:
- **Points To Money Ratio**: How many points = 1 coin (default: 100)
- **Excess Points Multiplier**: Bonus multiplier for excess points (default: 1.0)
  - Set to 1.5 for 50% bonus on excess points
  - Set to 2.0 for double bonus on excess points
- **Show Debug Logs**: Enable detailed conversion logs

---

## Troubleshooting

### Timer Not Starting:
- Verify TimedLevelManager GameObject exists in scene
- Check "Enable Timer" is checked in Inspector
- Verify scene name matches LevelData.sceneName ("Level0_1_TimedChallenge")

### Timer Not Displaying:
- Check Timer Text is assigned in TimedLevelManager Inspector
- Verify Canvas exists and is active
- Check TimerText GameObject is active

### Money Not Converting:
- Verify PointsToMoneyConverter GameObject exists in scene
- Check CurrencyManager exists in scene (should persist from menu)
- Verify Level 0.1 has `convertExcessPointsToMoney = true` in LevelManager
- Check console for error messages

### Level 0 Not Ending at 2000:
- Verify GameManager.CheckWinCondition() is being called
- Check LevelManager.Instance exists
- Verify Level 0 has `targetScore = 2000` in LevelData

---

## Future Enhancements

Possible improvements for Level 0.1:

1. **UI Improvements**:
   - Show potential money earnings in real-time
   - Display "Bonus: +X coins" when reaching target
   - Add timer pulse effect when critical

2. **Gameplay Variations**:
   - Level 0.2: 3000 points in 45 seconds
   - Level 0.3: Increasing difficulty with repositioning shards
   - Hard mode: 2000 points in 20 seconds (2x money multiplier)

3. **Progression System**:
   - Unlock Level 0.1 after completing Level 0
   - Track best time for Level 0.1
   - Leaderboard for fastest 2000 points

4. **Money System Integration**:
   - Shop to spend money on upgrades
   - Player speed boost (costs 50 coins)
   - Shield ability (costs 100 coins)
   - Slow motion ability (costs 150 coins)

---

## Conclusion

All three tasks have been successfully implemented:

✅ **Task 1**: Level 0 ends at 2000 points
✅ **Task 2**: Level 0.1 timed challenge (30 seconds)
✅ **Task 3**: Points-to-money conversion system

The system is modular, configurable, and ready for expansion. All code includes detailed documentation and debug logging for easy testing and troubleshooting.
