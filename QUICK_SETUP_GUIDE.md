# Quick Setup Guide - Level 0.1 Timed Challenge

## What Was Implemented

✅ **Task 1**: Level 0 now ends at exactly 2000 points
✅ **Task 2**: Level 0.1 timed challenge created (reach 2000 points in 30 seconds)
✅ **Task 3**: Points-to-money conversion for excess points above 2000

---

## Files Created

1. **[TimedLevelManager.cs](Assets/Scripts/TimedLevelManager.cs)** - Countdown timer system
2. **[PointsToMoneyConverter.cs](Assets/Scripts/PointsToMoneyConverter.cs)** - Points to currency converter
3. **[LEVEL_0_AND_0.1_IMPLEMENTATION_GUIDE.md](LEVEL_0_AND_0.1_IMPLEMENTATION_GUIDE.md)** - Full documentation

## Files Modified

1. **[GameManager.cs](Assets/Scripts/GameManager.cs)** - Added win condition check
2. **[LevelManager.cs](Assets/Scripts/LevelManager.cs)** - Added Level 0.1 configuration

---

## Quick Scene Setup for Level 0.1

### Step 1: Create the Scene

1. Duplicate your Level0_Core scene
2. Rename to: `Level0_1_TimedChallenge`
3. Save the scene

### Step 2: Add TimedLevelManager

1. Create Empty GameObject: "TimedLevelManager"
2. Add Component → TimedLevelManager
3. Set Time Limit to 30

### Step 3: Create Timer UI

1. In Canvas: Right-click → UI → Text - TextMeshPro
2. Name: "TimerText"
3. Position at top center of screen
4. Font Size: 48
5. Text Alignment: Center
6. Color: White

### Step 4: Link Components

1. Select TimedLevelManager GameObject
2. Drag TimerText into "Timer Text" field

### Step 5: Add PointsToMoneyConverter

1. Create Empty GameObject: "PointsToMoneyConverter"
2. Add Component → PointsToMoneyConverter
3. Set Points To Money Ratio: 100

### Step 6: Test

1. Press Play
2. Collect shards
3. Reach 2000 points before 30 seconds
4. Check Console for money conversion logs

---

## How It Works

### Level 0
- Player collects shards
- At 2000 points → Level immediately ends (WIN)

### Level 0.1
- Timer counts down from 30 seconds
- Player must reach 2000 points before timer expires
- Timer color changes: White → Yellow (10s) → Red (5s)
- **If time expires before 2000**: Game Over (LOSS)
- **If reach 2000 before time**: Win + Money conversion

### Money Conversion (Level 0.1 only)
- Base: Target score converts to money (2000 pts = 20 coins at 100:1 ratio)
- Bonus: Excess points convert to money (300 extra pts = 3 coins)
- Example: 2300 points = 23 coins total

---

## Testing Commands

### Test Level 0:
1. Load Level0_Core scene
2. Collect shards until 2000 points
3. Verify game ends with WIN

### Test Level 0.1:
1. Load Level0_1_TimedChallenge scene
2. Collect shards quickly
3. Try to reach 2000 before timer expires

---

## Configuration

### Change Timer Duration
Select TimedLevelManager → Change "Time Limit" value

### Change Conversion Rate
Select PointsToMoneyConverter → Change "Points To Money Ratio"
- 100 = 100 points per coin (default)
- 50 = 50 points per coin (more generous)
- 200 = 200 points per coin (less generous)

### Change Target Score
Edit [LevelManager.cs](Assets/Scripts/LevelManager.cs:75) → Change `targetScore` value

---

## Console Output to Expect

### Success (Level 0.1):
```
TimedLevelManager: Timer started - 30 seconds
Level Complete! Reached target score of 2000
TimedLevelManager: Level completed with 12.34 seconds remaining!
PointsToMoneyConverter: Conversion Complete!
  - Final Score: 2300
  - Total Awarded: 23 coins
```

### Failure (Level 0.1):
```
TimedLevelManager: Time's up!
Time's up! Score: 1800/2000 - Game Over
```

---

## Troubleshooting

**Timer not showing?**
→ Make sure TimerText is assigned in Inspector

**Money not converting?**
→ Check PointsToMoneyConverter exists in scene

**Level not ending at 2000?**
→ Verify LevelManager and GameManager exist in scene

---

For full documentation, see [LEVEL_0_AND_0.1_IMPLEMENTATION_GUIDE.md](LEVEL_0_AND_0.1_IMPLEMENTATION_GUIDE.md)
