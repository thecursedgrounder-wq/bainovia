# BAINOVIA — Combat System Documentation

## Overview

Bainovia features a heavy, deliberate combat system inspired by God of War and Dark Souls, combined with rune magic and spirit abilities. Every attack should feel impactful and consequential, with weight and consequence behind every action.

---

## Core Philosophy

### Design Principles
1. **Weight and Impact** — Every attack has mass and consequence
2. **Deliberate Action** — No button mashing; timing matters
3. **Risk/Reward** — Aggressive play has consequences
4. **Environmental Integration** — Use terrain and weather
5. **Spirit Integration** — Magic complements physical combat
6. **Cinematic Presentation** — Camera angles and choreography

### Combat Feel
- Heavy, deliberate animations
- Screen shake on impacts
- Slow-motion on perfect counters
- Audio feedback for every action
- Visual feedback (blood, sparks, spirit energy)

---

## Basic Combat Mechanics

### Controls

| Action | Input | Description |
|--------|-------|-------------|
| Light Attack | R1 / RB | Quick claw strike, 3-hit combo |
| Heavy Attack | R2 / RT | Charged slam, high damage |
| Dodge | Circle / B | Spirit dash, i-frames |
| Block | L1 / LB | Reduce damage, stagger enemies |
| Parry | L1 + R1 | Perfect timing for counter |
| Spirit Dash | L3 + Circle | Long-range dodge with spirit trail |
| Jump | X / A | Vertical mobility |
| Grab | Square + X | Grapple enemies |

### Attack System

**Light Attack Combo**
- **Hit 1:** Quick horizontal slash, fast startup
- **Hit 2:** Diagonal slash, medium startup
- **Hit 3:** Heavy downward slash, slow but powerful
- **Cancel:** Can cancel into dodge or block after hit 1-2

**Heavy Attack**
- **Charged:** Hold for 1-2 seconds for full damage
- **Uncharged:** Quick slam, reduced damage
- **Armor Break:** Breaks enemy guard on hit
- **Knockback:** Sends smaller enemies flying

**Claw Specials**
- **Rake:** Multi-hit horizontal attack
- **Lunge:** Forward dash attack
- **Overhead:** Vertical smash from above
- **Swipe:** Wide arc for crowd control

---

## Rune Magic Combat

### Rune Abilities

**Storm Strike**
- **Input:** Triangle / Y
- **Effect:** Lightning-infused claw attack
- **Damage:** 150% of normal attack
- **Cooldown:** 8 seconds
- **Visual:** Blue lightning trails on claws
- **Use Case:** High damage single target

**Thunder Slam**
- **Input:** Triangle + R2 / Y + RT
- **Effect:** Ground pound with shockwave
- **Damage:** 200% in radius
- **Cooldown:** 12 seconds
- **Visual:** Lightning bolt from sky, ground crack
- **Use Case:** Crowd control, area damage

**Ice Spikes**
- **Input:** Triangle + L1 / Y + LB
- **Effect:** Summon frozen spikes from earth
- **Damage:** 100% per spike, 3 spikes
- **Cooldown:** 10 seconds
- **Visual:** Ice crystals erupting from ground
- **Use Case:** Trapping enemies, environmental hazard

**Wind Shockwave**
- **Input:** Triangle + Circle / Y + B
- **Effect:** Knockback enemies in radius
- **Damage:** 50% (low damage, high utility)
- **Cooldown:** 6 seconds
- **Visual:** Wind vortex around player
- **Use Case:** Creating space, environmental kills

### Rune Combos

**Storm + Ice**
- Chain lightning that freezes targets
- Combo: Storm Strike → Ice Spikes

**Wind + Thunder**
- Creates lightning tornado
- Combo: Wind Shockwave → Thunder Slam

**Ice + Wind**
- Freezing blizzard
- Combo: Ice Spikes → Wind Shockwave

---

## Spirit Abilities

### Spirit Projection
- **Input:** L3 + R3 (hold)
- **Effect:** Astral form for reconnaissance
- **Duration:** 10 seconds
- **Cooldown:** 30 seconds
- **Uses:** Scout ahead, find secrets, avoid traps

### Spirit Summon
- **Input:** D-Pad Up
- **Effect:** Call ancient spirit ally
- **Duration:** 20 seconds
- **Cooldown:** 60 seconds
- **Spirit Types:**
  - Bear Spirit: Tank, high damage
  - Wolf Spirit: Fast, multiple targets
  - Hawk Spirit: Aerial support, spotting

### Spirit Shield
- **Input:** L3 + R1
- **Effect:** Protective barrier
- **Duration:** 5 seconds
- **Cooldown:** 20 seconds
- **Strength:** Blocks 3 hits

### Spirit Drain
- **Input:** L3 + R2 (hold on enemy)
- **Effect:** Absorb enemy essence
- **Benefit:** Heal + rune cooldown reduction
- **Risk:** Leaves player vulnerable

---

## Berserk Mode

### Activation
- **Trigger:** Rage meter full (100%)
- **Rage Build:** Taking damage, dealing damage, near-death
- **Input:** R3 + L3
- **Duration:** 15 seconds

### Berserk Effects
- **Damage:** +50% all damage
- **Speed:** +30% movement speed
- **Defense:** -25% damage resistance
- **Abilities:** All rune cooldowns halved
- **Visual:** Eyes glow intensely, fur bristles, spirit energy swirls

### Berserk Abilities
- **Berserk Slam:** Enhanced Thunder Slam
- **Spirit Roar:** AOE fear + damage
- **Claw Frenzy:** Rapid-fire light attacks

### Risks
- Cannot block while berserk
- Taking damage extends berserk but increases risk
- Cannot cancel berserk early
- Post-berserk fatigue (reduced damage for 10 seconds)

---

## Enemy Interaction

### Enemy Types

**Standard Enemies**
- **Health:** 100-300 HP
- **Behavior:** Aggressive, simple patterns
- **Weakness:** Light attacks, parries
- **Finisher:** Available at 25% HP

**Elite Enemies**
- **Health:** 500-1000 HP
- **Behavior:** Complex patterns, special attacks
- **Weakness:** Rune magic, specific timing
- **Finisher:** Available at 10% HP

**Boss Enemies**
- **Health:** 2000-5000 HP
- **Behavior:** Multiple phases, unique mechanics
- **Weakness:** Spirit abilities, environmental kills
- **Finisher:** Cinematic at 0% HP

### Enemy States
- **Idle:** Waiting for player
- **Patrol:** Moving through area
- **Alert:** Detected player
- **Combat:** Engaged in fight
- **Stunned:** Vulnerable to finisher
- **Berserk:** Enraged state
- **Fleeing:** Running away

### Enemy Weaknesses
- **Frost:** Slows movement
- **Lightning:** Stuns briefly
- **Wind:** Knockback
- **Spirit:** Reveals hidden enemies

---

## Finisher System

### Trigger Conditions
- Enemy at 25% HP (standard)
- Enemy at 10% HP (elite)
- Perfect parry (any HP)
- Environmental kill opportunity

### Finisher Types

**Claw Finisher**
- Grab enemy by throat
- Slash with claws
- Throw enemy aside
- Duration: 2 seconds

**Spirit Finisher**
- Spirit projection enters enemy
- Destroys from within
- Spirit emerges
- Duration: 3 seconds

**Environmental Finisher**
- Throw enemy into hazard
- Crush against wall
- Drop from height
- Duration: 1.5 seconds

**Berserk Finisher**
- Brutal multi-hit sequence
- Spirit energy explosion
- Enemy dismemberment
- Duration: 4 seconds

### Finisher Rewards
- Instant kill
- Spirit essence absorption
- Rune cooldown reset
- Health restoration

---

## Environmental Combat

### Environmental Hazards
- **Cliffs:** Throw enemies off
- **Spike Traps:** Lure enemies into
- **Explosive Barrels:** Destroy for AOE
- **Storm Lightning:** Use during storms
- **Ice Crystals:** Break for damage

### Environmental Interactions
- **Climb:** Vertical combat options
- **Swing:** Use rope bridges
- **Destroy:** Breakable walls/objects
- **Weather:** Storms enhance lightning abilities

---

## Combat Flow

### Engagement Phase
1. **Assessment:** Scan enemy types and patterns
2. **Positioning:** Choose optimal starting position
3. **Preparation:** Apply buffs, summon spirits
4. **Initiation:** Choose opening move

### Combat Phase
1. **Basic Attacks:** Build rage, chip damage
2. **Rune Magic:** Use abilities strategically
3. **Spirit Abilities:** Summon allies, use shields
4. **Dodging:** Avoid damage, create openings
5. **Parrying:** Perfect timing for counters
6. **Berserk:** Activate when rage meter full

### Execution Phase
1. **Finishers:** Execute stunned enemies
2. **Spirit Drain:** Absorb essence
3. **Environmental Kills:** Use hazards
4. **Berserk Finishers:** Brutal multi-kills

### Recovery Phase
1. **Heal:** Use potions or spirit drain
2. **Cooldown Management:** Time rune abilities
3. **Reposition:** Move to advantageous position
4. **Prepare:** Buff for next engagement

---

## Difficulty Scaling

### Difficulty Levels

**Story Mode**
- Reduced enemy damage (50%)
- Increased player damage (150%)
- Extended parry windows
- More frequent checkpoints

**Normal**
- Standard damage values
- Standard parry windows
- Standard checkpoint frequency

**Hard**
- Increased enemy damage (150%)
- Reduced player damage (80%)
- Tighter parry windows
- Fewer checkpoints

**Nightmare**
- Increased enemy damage (200%)
- Reduced player damage (60%)
- Very tight parry windows
- No checkpoints (perma-death optional)

### Adaptive Difficulty
- Adjusts based on player performance
- Death streak: Slightly easier
- Win streak: Slightly harder
- Can be disabled in options

---

## Combat UI

### HUD Elements
- **Health Bar:** Top left, red
- **Rage Meter:** Below health, orange
- **Rune Cooldowns:** Bottom center, icons
- **Spirit Essence:** Bottom right, blue
- **Enemy Health:** Top right, per enemy
- **Combo Counter:** Center screen, fades out

### Visual Feedback
- **Damage Numbers:** Floating text
- **Hit Markers:** Screen flash
- **Slow Motion:** On perfect parry
- **Camera Shake:** On heavy impacts
- **Blood Effects:** On critical hits

---

## Audio Design

### Combat Sounds
- **Claw Hits:** Wet, visceral impacts
- **Thunder Slams:** Deep, resonant booms
- **Spirit Abilities:** Ethereal, otherworldly
- **Berserk:** Distorted, aggressive
- **Finishers:** Dramatic, impactful

### Music
- **Combat Music:** Intense, rhythmic
- **Berserk Music:** Aggressive, driving
- **Victory Music:** Triumphant, resolving
- **Defeat Music:** Somber, melancholic

---

## Technical Considerations

### Animation
- **Root Motion:** Enabled for attacks
- **Blend Trees:** For smooth transitions
- **Hitboxes:** Precise, frame-perfect
- **Hit Stop:** 2-3 frames on heavy impacts
- **Camera Shake:** Synchronized with impacts

### Performance
- **Particle Effects:** Optimized for 60 FPS
- **Lighting:** Dynamic but performant
- **Physics:** Limited to essential interactions
- **AI:** Behavior trees for enemy logic

---

## Development Priorities

### Phase 1: Core Combat
1. Basic attack system
2. Dodge and block mechanics
3. Enemy hit detection
4. Basic enemy AI

### Phase 2: Rune Magic
1. Storm Strike implementation
2. Thunder Slam implementation
3. Rune cooldown system
4. Visual effects

### Phase 3: Spirit Abilities
1. Spirit projection
2. Spirit summon
3. Spirit shield
4. Spirit drain

### Phase 4: Advanced Systems
1. Berserk mode
2. Finisher system
3. Environmental combat
4. Boss mechanics

### Phase 5: Polish
1. Animation polish
2. Audio integration
3. UI refinement
4. Difficulty balancing

---

*Document Version: 1.0*  
*Last Updated: May 2026*
