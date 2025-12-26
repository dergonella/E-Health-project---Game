# Addiction Recovery Game – Mini GDD (Game Design Document)

**Concept summary:**  
A chase-based experience structured across multiple levels, culminating in a final boss that symbolizes addiction.  
Players confront three snake bosses, each representing a stage of alcohol addiction.  
The objective is to overcome these challenges, with each victory progressively weakening the final embodiment of addiction.  
By keeping the game “in your pocket,” individuals struggling with alcoholism can engage with it during moments of vulnerability.

---

## Core Gameplay Loop (1–3 min per run)

### 1. Prepare (5–10s)
- Pick a **Level** (Cobra → Viper → Mini tracks).
- Optional **Loadout**: 1 passive + 1 active boost (from Shop).
- Quick **Breathing Prompt** (3 deep breaths = +small Focus buffer).

### 2. Chase Run (60–120s)
- **Goal:** Collect N “Resolve Shards” and reach the **Safe Threshold** before the **Temptation** (your chosen chaser) catches you or your **Panic Meter** fills.
- **Arena:** tight 2D map with walls, “no-out-of-frame” boundaries.
- **Phases (rotating every ~20s):**
  - **Shuffle:** map hazards reposition; chaser re-paths.
  - **Chase:** chaser gains speed & line-of-sight bursts.
  - **Lure:** tempting “shortcuts” spawn high-value shards but raise Panic faster.

### 3. Resolve (15–30s)
- **Results:** shards collected → coins, XP, **Sobriety Streak** bonus.
- **Reflection Card:** short optional question “What helped just now?”
- **Buddy Dialogue:** supportive message, optional share to Support Circle.

---

## Win / Lose / Retreat Scenarios

- **Win (Level Clear):** reach Safe Threshold with required shards.  
  → Rewards: Coins = shards × level multiplier, 1–3 Stars, XP toward Stage Mastery.
- **Partial Win:** Threshold reached but under shards → ½ coins, small XP.
- **Lose:** (Caught or Panic 100%) → coins only, no penalty to streak.
- **Retreat:** manual exit “I need a break,” grounding tip shown.

> **Ethos:** Never punish relapse or absence. The game supports; it doesn’t judge.

---

## Difficulty Model (Adaptive + Manual)

**Manual Tiers:** Calm / Standard / Intense

**Adaptive knobs (per level):**
- Chaser base speed: 2.8 → 3.6 → 4.2 tiles/s  
- Panic gain per second in sight: 6% / 9% / 12%  
- Required shards: 12 / 16 / 20  
- Hazard density: 0.15 / 0.22 / 0.30 per tile  
- Shuffle frequency: 22s / 18s / 14s  
- Vision cone length: 6 / 8 / 10 tiles  

**Dynamic Assist:**
- Fail 2× → −10% chaser speed, +2s grace, +1 free Calm Pulse.  
- Win 3× → +1 shard requirement, +5% vision cone.

---

## Enemy AI: the “Temptations”

### 1. Snake – Cobra (Stage I)
- Aggressive lock-on, predictive dashes, taunts during chase.
- **Special:** “Hiss Burst” widens vision cone.

### 2. Snake – Viper (Stage II)
- Patrols intelligently, places “whisper traps.”
- **Special:** “Shed & Sprint” speed burst after losing sight.

### 3. Mini (Stage III)
- Gentler, manipulates Lure spawns (tempting shortcuts).
- **Special:** “Echo Mirror” creates decoy blocking paths.

### Alternative Demons
- **Shadow:** teleports in low light; slower but startling.  
- **Glass of Beer:** ricochets, leaves slowing puddles.

---

## Player Systems

### Movement & Interaction
- **Mobile:** virtual stick (drag anywhere) + single **Action** button.  
- **PC:** WASD + Space.  
- **Hard bounds:** bouncing off edges adds +5% Panic.

### Meters
- **Panic (0–100%)**: rises from vision/hazards; decreases in safe zones.  
- **Focus (0–100%)**: powers boosts; refills via shards & breathing prompts.

---

## Power-Ups & Shop

### Currency & Economy
- **Coins:** from shards & stars.  
- **Sobriety Streak Bonus:** every 10 days → +250 coins + Token (cosmetic unlock).  
- Typical reward: 60–120 coins per win.

### Boost Types

#### Actives (cost Focus)
- **Calm Pulse:** −25% Panic instantly (25s cd)  
- **Shadow Veil:** 3s invisibility (30s cd)  
- **Anchor Dash:** short burst, ignores slows (15s cd)  
- **Safe Beacon:** 4s zone, −8% Panic/s (35s cd)

#### Passives
- **Steady Steps:** −15% Panic on edge bounce  
- **Resolute Heart:** +10% Focus cap  
- **Quiet Footing:** −12% detection range  
- **Map Sense:** see next layout 1s earlier

#### One-Run Consumables
- **Buddy Cheer:** +30 Focus at start  
- **Grounding Stone:** saves you from Panic death once  
- **Shard Magnet:** +1 pickup radius

#### Progression Unlocks
- Tier I: after Cobra 1–3  
- Tier II: after Viper  
- Tier III: after Mini  

---

## Levels & Structure

### World Flow
- **Stage 1: Cobra (5 levels):** dark, tight corridors, corner pressure  
- **Stage 2: Viper (5 levels):** mid-open maps, traps, routing choices  
- **Stage 3: Mini (5 levels):** bright, open fields, smarter lures  

Each level:
- **Goal:** collect N shards → reach Threshold  
- **3 Challenges:** “No Dash,” “Under 70% Panic,” “Find Buddy Note.”  
- **Stars:** unlock passives & cosmetics  

---

## Meta Progression & Personalization

### Onboarding Form
- Quick questions → choose **Persona Archetype:**
  - **Fighter**, **Scholar**, **Connector**  
- Choose **Demon Skin:** snake / shadow / beer glass  
- Choose **Buddy:** bunny / pup / firefly  

### Dialogue System
- Temptation lines match persona (provocative, analytical, or emotional).  
- Buddy lines tailored to motivation.  
- Always short and positive; never guilt-based.

### Home Screen
- **Today Card:** streak & next milestone  
- **Quick Play**, **Levels**, **Shop**, **Support Corner**

### Sobriety Streak Tracker
- Calendar with non-shaming neutrality.  
- Every 10 days: coins + token + Buddy celebration.

---

## Boss (Milestone) Fights

- Every 5th level = **Boss Variant**
- Adds unique rules (Cobra Trial, Viper Trial, etc.)
- 3 phases × 45–60s → win by surviving phase 3
- Rewards: **Core Memory** (permanent passive slot)

---

## Session & Pacing

- **Single Run:** 90–150s  
- **Micro-session:** 3–5 min (2–3 runs)  
- **Daily Loop:** 1 quick win → streak → shop → exit  

---

## UX & Accessibility

- Color-blind friendly palettes  
- Low-stim mode (reduced VFX, softer audio)  
- Notifications opt-in only  
- Support Circle private and invite-only  

---

## Example Balancing

- **Standard:** 16 shards, 3.2 tiles/s, Panic +8%/s in LoS  
- **Coins:** 3 per shard, +10/+20/+40 for stars  
- **Focus:** 100 max, +6 per shard, Calm Pulse = 35  

---

## Dialogue Examples

**Buddy Lines**
- Fighter: “You’ve done harder things. One more shard.”  
- Scholar: “Notice your breath: in 4, out 6. Panic is already dropping.”  
- Connector: “Picture the person who smiles when you win.”  

**Temptation Lines**
- Cobra: “One quick detour. You deserve it.”  
- Viper: “You’re tired—why not rest here?”  
- Mini: “It would be easier to stop now.”  

---

## Core Systems

### Chase Mechanics
- Pathfinding: grid A*, updates every 0.25s  
- Out-of-frame clamp adds small Panic spike  
- Random shard placement each Shuffle phase  
- Threshold activates after shard quota met  

### Home Screen
- Header: streak + next bonus  
- Tiles: Levels | Shop | Support | Settings  
- Motivational pop-ups (1 per session max)

### Shop
- Tabs: Actives | Passives | Consumables | Cosmetics  
- Locks show unlock source  

### Personalization
- Persona sets:
  - Dialogue pools  
  - Default difficulty  
  - Buddy species/voice  
  - Support Circle prompts  

---

## Ethical Guardrails

- No timers or energy systems  
- No real-money purchases  
- No loss punishment  
- Supportive, agency-affirming language  
- Calm visuals (no flashing red, use soft desaturation)  

---

## Roadmap (High-Level)

1. **Vertical Slice:** Cobra Level 1 prototype (1 chaser, shards, Panic, Threshold).  
2. **Meta:** streak tracker, Tier I shop, adaptive assist.  
3. **Content:** 15 total levels (5 per stage) + milestone bosses.  
4. **Polish:** accessibility, persona dialogues, support corner.  

---

### Optional Deliverables
- **Level Spreadsheet** with tuning values  
- **Dialogue JSON Schema** (persona-based lines)  
- **Unity/Godot Task List** for implementation  
