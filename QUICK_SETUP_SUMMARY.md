# ⚡ Quick Setup Summary - Level 0 New Features

**2 New Features Ready to Use!**

---

## 🎯 Feature 1: 10 Shards Always Spawning

### What to Do:
1. Open `Level0_Core.unity` scene
2. Create Empty GameObject → name it "ShardSpawner"
3. Add Component → "Shard Spawner" script
4. In Inspector:
   - **Shard Prefab:** Drag `Assets/Prefabs/Shard.prefab`
   - **Shard Count:** `10`
   - **Bound X:** `4`
   - **Bound Y:** `3`
   - **Wall Buffer:** `0.5`
5. Delete any old Shard instances in Hierarchy (not the prefab!)
6. Click Play → 10 shards spawn randomly!

**Done!** Shards will now always spawn 10 at a time at random locations.

---

## 🏃 Feature 2: Player Can Move After Death

### What to Do:
**Nothing!** It's already working in the updated scripts.

When a snake catches you, you can now move for:
- **1.5 seconds** OR
- **2 meters**
- Whichever comes first!

### Optional Tuning:
Select Player GameObject in Level0_Core scene, in Inspector find:
- **Death Movement Distance:** 2m (change to adjust distance)
- **Death Movement Delay:** 1.5s (change to adjust time)

---

## 📂 What Changed?

### New Files:
- ✅ `Assets/Scripts/ShardSpawner.cs` (NEW)

### Modified Files:
- ✅ `Assets/Scripts/ShardController.cs` (updated to work with spawner)
- ✅ `Assets/Scripts/PlayerController.cs` (added death movement)
- ✅ `Assets/Scripts/CobraAI.cs` (calls Die() instead of instant game over)

---

## ✅ Quick Test

1. Play Level 0
2. **Test Shards:** Count 10 shards, collect one, still 10 visible ✓
3. **Test Death:** Get caught, try to move, game over after ~1.5 seconds ✓

---

**That's it! Both features are ready.** 🎮

For detailed guide: See [LEVEL0_NEW_FEATURES_GUIDE.md](LEVEL0_NEW_FEATURES_GUIDE.md)
