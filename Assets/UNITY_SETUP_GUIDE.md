# Unity Setup Instructions for Cobra Chase Game

## Project Setup

### 1. Create New Unity Project
- Open Unity Hub
- Create a new **2D Project**
- Name it "CobraChase" or whatever you prefer
- Unity version: 2021.3 LTS or newer recommended

### 2. Import Scripts
Copy all the C# scripts from the `Unity_Scripts` folder into your Unity project's `Assets/Scripts` folder:
- `PlayerController.cs`
- `CobraAI.cs`
- `ShardController.cs`
- `GameManager.cs`
- `UIManager.cs`

---

## Scene Setup

### 3. Camera Setup
- Select Main Camera
- Set **Projection** to **Orthographic**
- Set **Size** to **3** (or adjust based on your preference)
- Position: (0, 0, -10)
- Background: Dark gray color (RGB: 40, 40, 45)

### 4. Create Player GameObject
- Create **2D Object → Sprites → Circle** (or use a sprite)
- Rename to "Player"
- Tag: "Player"
- Position: (-1, 0, 0)
- Scale: (0.4, 0.4, 1) to make it roughly 20 pixels
- **Color**: Blue (RGB: 50, 120, 200)
- Add Component: **Circle Collider 2D**
  - Radius: 0.2
  - Is Trigger: ✓ (checked)
- Add Component: **Rigidbody 2D**
  - Body Type: Kinematic
  - Gravity Scale: 0
- Add Component: **PlayerController** script

### 5. Create Cobra Prefabs (Make 3 Different Cobras)

#### Red Cobra (Chase AI)
- Create **2D Object → Sprites → Circle**
- Rename to "Cobra_Chase"
- Tag: "Cobra"
- Scale: (0.36, 0.36, 1)
- **Color**: Red (RGB: 200, 50, 50)
- Add Component: **Circle Collider 2D**
  - Radius: 0.18
  - Is Trigger: ✓ (checked)
- Add Component: **Rigidbody 2D**
  - Body Type: Kinematic
  - Gravity Scale: 0
- Add Component: **CobraAI** script
  - AI Type: **Chase**
  - Speed: 2.6
- Drag to **Assets/Prefabs** folder to create prefab
- **Optional**: Add 2 small white circles as eyes (children objects)

#### Orange Cobra (Attack AI - The Smart One!)
- Create **2D Object → Sprites → Circle**
- Rename to "Cobra_Attack"
- Tag: "Cobra"
- Scale: (0.36, 0.36, 1)
- **Color**: Orange (RGB: 255, 140, 0)
- Add Component: **Circle Collider 2D**
  - Radius: 0.18
  - Is Trigger: ✓ (checked)
- Add Component: **Rigidbody 2D**
  - Body Type: Kinematic
  - Gravity Scale: 0
- Add Component: **CobraAI** script
  - AI Type: **Attack**
  - Speed: 2.6
  - Prediction Multiplier: 12
  - Close Range Distance: 0.8
  - Boost Multiplier: 0.8
- Drag to **Assets/Prefabs** folder

#### Purple Cobra (Random AI)
- Create **2D Object → Sprites → Circle**
- Rename to "Cobra_Random"
- Tag: "Cobra"
- Scale: (0.36, 0.36, 1)
- **Color**: Purple (RGB: 150, 50, 200)
- Add Component: **Circle Collider 2D**
  - Radius: 0.18
  - Is Trigger: ✓ (checked)
- Add Component: **Rigidbody 2D**
  - Body Type: Kinematic
  - Gravity Scale: 0
- Add Component: **CobraAI** script
  - AI Type: **Random**
  - Speed: 2.6
  - Random Target Change Interval: 1
- Drag to **Assets/Prefabs** folder

### 6. Create Shard Prefab
- Create **2D Object → Sprites → Circle**
- Rename to "Shard"
- Tag: "Shard"
- Position: (0, 0, 0)
- Scale: (0.16, 0.16, 1)
- **Color**: Yellow (RGB: 255, 215, 0)
- Add Component: **Circle Collider 2D**
  - Radius: 0.08
  - Is Trigger: ✓ (checked)
- Add Component: **ShardController** script
  - Pulse Speed: 0.1
  - Pulse Amount: 0.02
  - Shard Value: 100
- Drag to **Assets/Prefabs** folder

### 7. Create Walls (Plus-Shaped Map)

#### Horizontal Wall (Center)
- Create **2D Object → Sprites → Square**
- Rename to "Wall_Horizontal"
- Tag: "Wall"
- Position: (0, 0, 0)
- Scale: (1.8, 0.25, 1) - Adjust to create horizontal bar
- **Color**: Black or dark gray
- Add Component: **Box Collider 2D**
  - Is Trigger: No

#### Vertical Wall (Center)
- Create **2D Object → Sprites → Square**
- Rename to "Wall_Vertical"
- Tag: "Wall"
- Position: (0, 0, 0)
- Scale: (0.25, 1.8, 1) - Adjust to create vertical bar
- **Color**: Black or dark gray
- Add Component: **Box Collider 2D**
  - Is Trigger: No

#### Corner Decorative Walls (Optional L-shapes)
Create small L-shaped walls in each corner:
- Top-left, Top-right, Bottom-left, Bottom-right
- Each L-shape is 2 small rectangles
- Scale: (0.4, 0.08, 1) and (0.08, 0.4, 1)
- Position in corners: approximately (±3.8, ±2.8, 0)
- Tag: "Wall"
- Add **Box Collider 2D**

### 8. Create UI Canvas

#### Main Canvas
- Create **UI → Canvas**
- Canvas Scaler:
  - UI Scale Mode: Scale with Screen Size
  - Reference Resolution: 800x600

#### Score Text
- Create **UI → Text - TextMeshPro** (child of Canvas)
- Rename to "ScoreText"
- Position: Top-left corner
- Rect Transform: Anchor to top-left
  - Pos X: 10, Pos Y: -10
  - Width: 300, Height: 60
- Font Size: 36
- Color: White
- Text: "Score: 0"

#### Win Screen Panel
- Create **UI → Panel** (child of Canvas)
- Rename to "WinScreen"
- Color: Black with alpha ~200
- **Deactivate this object** (uncheck in inspector)
- Add child **UI → Text - TextMeshPro**:
  - Name: "WinTitle"
  - Text: "LEVEL CLEARED!"
  - Font Size: 60
  - Color: Green (RGB: 50, 200, 50)
  - Alignment: Center
- Add child **UI → Text - TextMeshPro**:
  - Name: "WinScoreText"
  - Text: "Total Score: 0"
  - Font Size: 40
  - Color: Yellow
  - Alignment: Center
  - Position below title
- Add child **UI → Text - TextMeshPro**:
  - Name: "RestartText"
  - Text: "Press R to Restart"
  - Font Size: 36
  - Color: White
  - Alignment: Center
  - Position below score

#### Lose Screen Panel
- Create **UI → Panel** (child of Canvas)
- Rename to "LoseScreen"
- Color: Black with alpha ~200
- **Deactivate this object** (uncheck in inspector)
- Add child **UI → Text - TextMeshPro**:
  - Name: "LoseTitle"
  - Text: "CAUGHT!"
  - Font Size: 60
  - Color: Red (RGB: 200, 50, 50)
  - Alignment: Center
- Add child **UI → Text - TextMeshPro**:
  - Name: "LoseScoreText"
  - Text: "Total Score: 0"
  - Font Size: 40
  - Color: Yellow
  - Alignment: Center
- Add child **UI → Text - TextMeshPro**:
  - Name: "RestartText"
  - Text: "Press R to Restart"
  - Font Size: 36
  - Color: White
  - Alignment: Center

### 9. Create GameManager Object
- Create **Empty GameObject**
- Rename to "GameManager"
- Add Component: **GameManager** script
- Set Win Score: 2000
- Set Total Shards: 10
- Drag UI Canvas's UIManager reference (see step 10)
- Drag prefabs into slots:
  - Player Prefab: Player prefab
  - Cobra Chase Prefab: Cobra_Chase prefab
  - Cobra Attack Prefab: Cobra_Attack prefab
  - Cobra Random Prefab: Cobra_Random prefab
  - Shard Prefab: Shard prefab
- Set spawn positions (adjust as needed)

### 10. Create UIManager Object
- Select the **Canvas**
- Add Component: **UIManager** script
- Drag UI elements into slots:
  - Score Text: ScoreText
  - Win Screen: WinScreen panel
  - Lose Screen: LoseScreen panel
  - Win Score Text: WinScoreText
  - Lose Score Text: LoseScoreText
- Go back to GameManager and drag Canvas into UI Manager slot

---

## Tags Setup

Make sure these tags exist (Edit → Project Settings → Tags):
- Player
- Cobra
- Shard
- Wall

---

## Testing

1. Press Play
2. Use **WASD** or **Arrow Keys** to move
3. Avoid the cobras (especially the orange one with advanced AI!)
4. Collect yellow shards (100 points each)
5. Reach 2000 points to win
6. Press **R** to restart after game over
7. Press **ESC** to quit

---

## Customization Tips

### Adjust Difficulty
- **Cobra Speed**: Change speed values in CobraAI components
- **Player Speed**: Change in PlayerController
- **Win Score**: Change in GameManager
- **Orange Cobra Aggression**: Adjust prediction_multiplier and boost_multiplier

### Visual Improvements
- Replace circles with custom sprites
- Add particle effects on shard collection
- Add sound effects
- Add animations for cobras
- Create better wall visuals

### Additional Features
- Add multiple levels
- Add power-ups (speed boost, invincibility)
- Add a timer for speedrun mode
- Add difficulty selection menu

---

## Troubleshooting

**Cobras not moving**: Make sure Player has "Player" tag and CobraAI can find it

**Collisions not working**: Ensure all colliders are set to "Is Trigger" and Rigidbody2D Body Type is "Kinematic"

**UI not showing**: Check Canvas Scaler settings and camera setup

**Shards spawning in walls**: Adjust spawn bounds in ShardController and GameManager

---

Enjoy your Unity version of Cobra Chase!
