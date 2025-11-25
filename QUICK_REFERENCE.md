# 🎮 Quick Reference Guide

## 📂 File Structure

```
Assets/
├── Scenes/
│   ├── MenuScene.unity
│   ├── Level0_Core.unity
│   ├── Level1_ContactZone.unity
│   ├── Level2_ToxicGrounds.unity
│   └── Level3_DivorcePapers.unity
├── Scripts/
│   ├── LevelManager.cs
│   ├── MenuManager.cs
│   ├── GameManager.cs
│   ├── UIManager.cs
│   ├── PlayerController.cs
│   ├── HealthSystem.cs
│   ├── AbilitySystem.cs
│   ├── CobraAI.cs
│   ├── ShardController.cs
│   ├── DifficultyManager.cs
│   ├── Projectile.cs
│   ├── Mine.cs
│   └── Painkiller.cs
└── Prefabs/
    ├── FireProjectile.prefab
    ├── PoisonProjectile.prefab
    ├── Mine.prefab
    └── Painkiller.prefab
```

---

## ⚙️ Component Cheat Sheet

### Player Components by Level

| Component | Level 0 | Level 1 | Level 2 | Level 3 |
|-----------|---------|---------|---------|---------|
| PlayerController | ✓ (smooth!) | ✓ (smooth!) | ✓ (smooth!) | ✓ (smooth!) |
| Rigidbody2D | ✓ | ✓ | ✓ | ✓ |
| Collider2D | ✓ | ✓ | ✓ | ✓ |
| HealthSystem | ✗ | ✓ | ✓ | ✓ |
| AbilitySystem | ✗ | Shield | Slow Motion | Both |

**Note:** All levels now have smooth momentum-based movement!

### Cobra Components

Required on ALL cobras:
- Sprite Renderer
- Rigidbody2D (Kinematic)
- Circle Collider 2D (Is Trigger ✓)
- CobraAI script
- Tag: "Cobra"

### Projectile Components

Required on ALL projectiles:
- Sprite Renderer
- Rigidbody2D (Kinematic, Gravity 0)
- Circle Collider 2D (Is Trigger ✓)
- Projectile script

---

## 🎯 Level Configuration Quick Copy

### Level 0 - Core
```
Player: NO HealthSystem, NO AbilitySystem
Cobras: 3 (no projectiles)
Win Score: 2000
Death: 1-hit
```

### Level 1 - Contact Zone
```
Player:
- HealthSystem (100 HP, 100 Focus)
- AbilitySystem (Shield only)

Cobras: 3
- Shoot FireProjectile
- Fire Rate: 0.5-0.75

Win Score: 2000 OR 90s survival
```

### Level 2 - Toxic Grounds
```
Player:
- HealthSystem (100 HP, 100 Focus, Poison settings)
- AbilitySystem (Slow Motion only)

Cobras: 3
- Shoot PoisonProjectile
- Fire Rate: 0.5-1.0

Pickups: 3 Painkillers
Win Score: 2500 OR 120s survival
```

### Level 3 - Divorce Papers
```
Player:
- HealthSystem (full settings)
- AbilitySystem (Shield + Slow Motion)

Cobras: 3+
- Mix Fire + Poison projectiles
- Higher speeds (3.0-3.5)

Hazards: 8-12 Mines
Pickups: 3-4 Painkillers
Win Score: 3000
```

---

## 🔧 Settings Quick Reference

### PlayerController Values 🌊 NEW!
```
Speed: 4
Acceleration: 20  ← NEW - Smooth movement!
Deceleration: 25  ← NEW - Smooth stopping!
```
**Creates smooth, flowing movement like ice skating!**

### HealthSystem Values
```
Max Health: 100
Max Focus: 100
Invulnerability Duration: 0.5
Poison Damage/Sec: 3
Poison Duration: 8
Poison Speed Reduction: 0.4
```

### AbilitySystem - Shield
```
Duration: 3 seconds
Cooldown: 15 seconds
Focus Cost: 30
Key: Q
```

### AbilitySystem - Slow Motion
```
Time Scale: 0.3 (30% speed)
Duration: 4 seconds
Cooldown: 20 seconds
Focus Cost: 40
Key: E
```

### Projectile - Fire
```
Type: Fire
Speed: 5
Damage: 15
Lifetime: 5
Applies Poison: NO
Color: Orange (255, 100, 0)
```

### Projectile - Poison
```
Type: Poison
Speed: 5
Damage: 15
Lifetime: 5
Applies Poison: YES
Color: Purple (150, 0, 255)
```

### Mine
```
Damage: 35
Knockback: 2.5
Stun Duration: 0.5
Warning Distance: 1.5
Pulse Speed: 5
Color: Red (255, 50, 50)
```

### Painkiller
```
Heal Amount: 15
Pulse Speed: 2
Pulse Amount: 0.1
Color: Green (0, 255, 100)
```

---

## 🎨 Color Palette

### UI Colors
```
Health Bar: Red (255, 50, 50)
Focus Bar: Cyan (0, 200, 255)
Shield Icon: Cyan (0, 200, 255)
Slow Motion Icon: Yellow (255, 255, 0)
Poison Indicator: Purple (200, 0, 255)
Stun Indicator: Yellow (255, 255, 0)
```

### Entity Colors
```
Player: Blue (0, 150, 255)
Shard: Yellow (255, 255, 0)
Cobra Attack: Orange (255, 150, 0)
Cobra Ambusher: Purple (200, 0, 255)
Cobra Chase: Green (0, 255, 100)
Fire Projectile: Orange (255, 100, 0)
Poison Projectile: Purple (150, 0, 255)
Mine: Red (255, 50, 50)
Painkiller: Green (0, 255, 100)
```

---

## 📍 Typical Object Positions

### Arena Bounds
```
Camera Orthographic Size: 3
Play Area: -4 to 4 (X), -3 to 3 (Y)
```

### Spawn Positions Example
```
Player: (-1, 0, 0)
Cobra 1: (2.5, 1, 0)
Cobra 2: (-1.5, 1, 0)
Cobra 3: (3, -1, 0)
```

### Scale Values
```
Player: 0.4, 0.4, 1
Cobra: 0.36, 0.36, 1
Shard: 0.25, 0.25, 1
Projectile: 0.3, 0.3, 1
Mine: 0.4, 0.4, 1
Painkiller: 0.3, 0.3, 1
```

---

## 🎮 Controls

### Player Movement
```
W or ↑ : Move Up
S or ↓ : Move Down
A or ← : Move Left
D or → : Move Right
```

### Abilities
```
Q : Shield (Levels 1, 3)
E : Slow Motion (Levels 2, 3)
```

### System
```
R : Restart (on Win/Lose screen)
ESC : Quit Game
```

---

## 🐛 Common Error Fixes

### "NullReferenceException: LevelManager.Instance"
- Add LevelManager GameObject to MenuScene
- Make sure it has LevelManager script

### "Scene could not be loaded"
- File > Build Settings
- Add all scenes (drag from Project window)
- MenuScene must be index 0

### "Player not responding to input"
- Check PlayerController script is attached
- Check Input System isn't blocking (no InputSystem package conflicts)

### "UI not updating"
- Check UIManager has all references assigned
- Check Canvas has UIManager component
- Check GameManager has UI Manager reference

### "Abilities don't work"
- Player needs HealthSystem component
- Player needs AbilitySystem component
- Check hasShield/hasSlowMotion checkboxes

### "Projectiles pass through player"
- Check Player tag is "Player"
- Check Collider2D "Is Trigger" is checked
- Check Rigidbody2D exists on projectile

### "Cobras don't shoot"
- Check "Can Shoot Projectiles" is checked
- Check Projectile Prefab is assigned
- Check Fire Rate > 0

### "Back to Menu doesn't work"
- Check LevelManager exists in MenuScene
- Check button has OnClick event
- Check event calls UIManager.OnBackToMenuClicked()

---

## ✅ Quick Testing Checklist

### Menu Scene
- [ ] 4 buttons visible
- [ ] Each button loads correct level
- [ ] Quit button works

### All Levels
- [ ] Player moves
- [ ] Cobras move
- [ ] Shards collectable
- [ ] Score updates
- [ ] Win condition works
- [ ] Lose condition works
- [ ] Back to menu button works

### Level-Specific
- [ ] Level 0: 1-hit death
- [ ] Level 1: Health bar, Shield works
- [ ] Level 2: Poison works, Slow motion works, Painkillers work
- [ ] Level 3: Mines explode, Both abilities work

---

## 📊 Performance Tips

### If game lags:
1. Reduce number of cobras
2. Lower projectile fire rate
3. Reduce number of mines
4. Check for infinite loops in custom code
5. Disable vsync (Edit > Project Settings > Quality)

### If collision detection fails:
1. Physics2D.queriesHitTriggers must be true
2. Collision Matrix (Edit > Project Settings > Physics 2D)
3. Layer collision settings
4. Collider sizes (too small = miss collisions)

---

## 🚀 Build Settings

### Recommended Settings
```
Platform: PC, Mac & Linux Standalone
Architecture: x86_64
Compression: Default
Development Build: Unchecked (for release)
```

### Before Building
1. Test all scenes
2. Check Build Settings scene order
3. Save all scenes
4. Close Unity Editor windows
5. Build to empty folder

---

## 📝 Tag Reference

Required Tags:
- `Player`
- `Cobra`
- `Shard`
- `Wall` (if using walls)

Create if missing:
- Tags & Layers > Tags > + button

---

## 🎯 Quick Stats Summary

| Level | Win Score | Time Option | Cobras | Mechanics |
|-------|-----------|-------------|--------|-----------|
| 0 | 2000 | None | 3 | 1-hit death |
| 1 | 2000 | 90s | 3 | HP, Shield, Fire |
| 2 | 2500 | 120s | 3 | Poison, Slow Motion |
| 3 | 3000 | None | 3+ | All + Mines |

---

Good luck! 🎮✨
