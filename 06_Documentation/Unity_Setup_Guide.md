# Bainovia - Unity Setup Guide

## Project Overview

**Project:** Bainovia - AAA Dark Fantasy Action RPG  
**Engine:** Unity (originally designed for UE5, adapted for Unity)  
**Genre:** Action RPG with heavy combat, rune magic, and spirit abilities  
**Inspiration:** Arcane, The Witcher, God of War, Diablo IV

---

## Project Structure

```
03_Bainovia_Game/
├── 01_Design_Documents/     # GDD, Character Design, etc.
├── 02_Art_Assets/           # 3D models, textures, materials
├── 03_Audio/                # Music, SFX, voice lines
├── 04_Code/                 # All C# scripts
│   └── Scripts/             # 14 scripts total (7 original + 7 new)
├── 05_Level_Design/         # Scene files, prefabs
├── 06_Cinematics/           # Timeline, cutscenes
├── 07_Production/           # Project management
└── 08_Builds/               # Compiled builds
```

---

## Unity Project Setup

### 1. Create New Unity Project

1. Open Unity Hub
2. Click "New Project"
3. Select **3D Core** template
4. Project Name: `Bainovia`
5. Location: `c:\Users\thecu\OneDrive\Desktop\project\03_Bainovia_Game`
6. Unity Version: 2022.3 LTS or later
7. Click "Create Project"

### 2. Import Scripts

1. Navigate to `Assets/` folder
2. Create folder structure:
   ```
   Assets/
   ├── Scripts/
   ├── Prefabs/
   ├── Materials/
   ├── Models/
   ├── Audio/
   ├── UI/
   └── Scenes/
   ```

3. Copy all scripts from `04_Code/Scripts/` to `Assets/Scripts/`

### 3. Configure Project Settings

#### Input System

1. Go to **Edit > Project Settings > Input System**
2. Set **Active Input Handling** to **Both** (supports legacy and new input)

#### Physics

1. Go to **Edit > Project Settings > Physics**
2. Set **Fixed Timestep** to `0.02` (50 Hz)
3. Set **Default Solver Iterations** to `8`
4. Set **Default Solver Velocity Iterations** to `4`

#### Quality Settings

1. Go to **Edit > Project Settings > Quality**
2. Set **Pixel Light Count** to `4`
3. Enable **Soft Vegetation**
4. Set **Shadow Distance** to `150`
5. Set **Shadow Cascades** to `4 Cascades`

#### Tags and Layers

Create these layers:
- `Player` (Layer 6)
- `Enemy` (Layer 7)
- `Interactable` (Layer 8)
- `Environment` (Layer 9)
- `UI` (Layer 10)

Create these tags:
- `Player`
- `Enemy`
- `NPC`
- `Interactable`
- `QuestItem`

---

## Scene Setup

### 1. Player Setup

**Create Player GameObject:**
1. Create empty GameObject named "Player"
2. Add **CharacterController** component
3. Add **Animator** component
4. Add **BainoviaCharacterController** script
5. Add **Health** script
6. Add **AudioSource** component

**Configure BainoviaCharacterController:**
- Walk Speed: `3`
- Run Speed: `8`
- Jump Force: `8`
- Attack Range: `2.5`
- Light Attack Damage: `25`
- Heavy Attack Damage: `50`
- Attack Cooldown: `1.2`
- Enemy Layers: `Enemy` layer
- Attack Point: Create empty child "AttackPoint" in front of player

**Configure Health:**
- Max Health: `200`
- Can Regenerate: `true`
- Regeneration Rate: `1`
- Regeneration Delay: `5`

**Add Camera:**
1. Create empty GameObject "CameraRoot" as child of Player
2. Add **CameraController** script to CameraRoot
3. Add Main Camera as child of CameraRoot
4. Set Camera position to `(0, 1.6, 0)`
5. Assign Player Body reference in CameraController

**Add Rune Magic System:**
1. Add **RuneMagicSystem** script to Player
2. Configure available runes: Storm, Frost, Wind, Spirit
3. Assign VFX prefabs and audio clips

**Add Skill Tree System:**
1. Add **SkillTreeSystem** script to Player
2. Configure skills for Combat, Exploration, Social branches
3. Set initial skill points: `0`

**Add Inventory System:**
1. Add **InventorySystem** script to Player
2. Set max slots: `30`
3. Configure equipment slots

**Add Spirit Companion System:**
1. Add **SpiritCompanionSystem** script to Player
2. Set max active spirits: `3`
3. Create spirit spawn point child GameObject

**Add Interaction System:**
1. Add **InteractionSystem** script to Player
2. Assign Main Camera reference
3. Set interact distance: `3`
4. Set interactable mask: `Interactable` layer

### 2. Enemy Setup

**Create Enemy Prefab:**
1. Create empty GameObject "Enemy"
2. Add **Capsule Collider**
3. Add **Rigidbody** (isKinematic: true)
4. Add **NavMeshAgent**
5. Add **Health** script
6. Add **EnemyAI** script
7. Add **Animator** component

**Configure EnemyAI:**
- Detection Range: `20`
- Attack Range: `2.5`
- Chase Speed: `5`
- Patrol Speed: `2`
- Attack Cooldown: `2`
- Damage: `20`
- Player Layer: `Player` layer
- Assign patrol points in scene

**Configure Health:**
- Max Health: `100`
- Destroy On Death: `true`
- Destroy Delay: `2`

**Bake NavMesh:**
1. Select terrain/ground GameObjects
2. Set to **Navigation Static**
3. Go to **Window > AI > Navigation**
4. Click **Bake**

### 3. UI Setup

**Create Canvas:**
1. Create **Canvas** (UI > Canvas)
2. Set Render Mode: **Screen Space - Overlay**
3. Add **Canvas Scaler** (Scale with screen size)
4. Add **Graphic Raycaster**

**Create Health Bar:**
1. Create **Slider** under Canvas
2. Name: "HealthBar"
3. Set anchor to top-left
4. Add **Text** for percentage display

**Create Spirit Essence Bar:**
1. Create **Slider** under Canvas
2. Name: "SpiritBar"
3. Set anchor to top-left (below health bar)
4. Add **Text** for percentage display

**Create Rage Meter:**
1. Create **Slider** under Canvas
2. Name: "RageBar"
3. Set anchor to top-left (below spirit bar)
4. Add **Text** for percentage display
5. Create GameObject for berserk indicator

**Create Rune Cooldown UI:**
1. Create empty GameObject "RuneCooldowns"
2. Add **Image** components for each rune (array of 4)
3. Add **Text** components for cooldown timers

**Create Quest Panel:**
1. Create **Panel** under Canvas
2. Name: "QuestPanel"
3. Add **Text** for quest title
4. Add **Text** for quest description
5. Add **Slider** for quest progress

**Create Interaction Prompt:**
1. Create **Panel** under Canvas
2. Name: "InteractionPrompt"
3. Center on screen
4. Add **Text** for prompt text

**Add GameUI Script:**
1. Add **GameUI** script to Canvas
2. Assign all UI references in Inspector
3. Assign Player, RuneMagicSystem, QuestSystem references

### 4. NPC Setup

**Create NPC Prefab:**
1. Create empty GameObject "NPC"
2. Add **Capsule Collider**
3. Add **NPCInteractable** script
4. Add **DialogueSystem** script (or reference to global system)
5. Configure dialogue nodes in Inspector

**Configure NPCInteractable:**
- NPC Name: Character name
- Start Dialogue Node ID: Starting conversation node
- Interaction Text: "Talk to [Name]"

### 5. Quest System Setup

**Create Quest Manager:**
1. Create empty GameObject "QuestManager"
2. Add **QuestSystem** script
3. Configure available quests in Inspector
4. Assign FactionSystem and BainoviaCharacterController references

**Configure Quests:**
- Quest ID: Unique identifier
- Quest Name: Display name
- Description: Quest description
- Faction ID: Associated faction
- Required Reputation: Minimum reputation to accept
- Objectives: Kill, Collect, Talk, Explore, Defend, Escort
- Rewards: Gold, Experience, Items, Reputation, Spirit Essence

### 6. Faction System Setup

**Create Faction Manager:**
1. Create empty GameObject "FactionManager"
2. Add **FactionSystem** script
3. Configure factions in Inspector

**Configure Factions:**
- Bainovian Clans (7 clans)
- Spikslian Merchant Houses (5 houses)
- Rune Scholars
- Storm Cultists
- Desert Nomads
- Spirit Priests

### 7. Dialogue System Setup

**Create Dialogue Manager:**
1. Create empty GameObject "DialogueManager"
2. Add **DialogueSystem** script
3. Configure dialogue nodes in Inspector
4. Assign FactionSystem, QuestSystem, BainoviaCharacterController references
5. Assign Dialogue UI GameObject

**Create Dialogue UI:**
1. Create **Panel** under Canvas
2. Name: "DialogueUI"
3. Add **Text** for speaker name
4. Add **Text** for dialogue text
5. Create buttons for dialogue options (1-9)

### 8. Environment Setup

**Lighting:**
1. Create **Directional Light** for sun
2. Enable shadows (Hard shadows)
3. Create **Point Lights** for lanterns/interiors
4. Create **Area Lights** for ambient lighting

**Post-Processing:**
1. Install **Universal Render Pipeline (URP)**
2. Add **Volume** component to Main Camera
3. Configure Bloom, Color Grading, Ambient Occlusion

**Audio:**
1. Create **AudioListener** on Main Camera
2. Create **AudioSources** for music, SFX, voice
3. Configure 3D spatial audio for environmental sounds

---

## Script Integration

### Required Script References

Each script needs specific references assigned in the Inspector:

**BainoviaCharacterController:**
- Attack Point (Transform)
- Rune VFX (GameObject array)
- Combat Sounds (AudioClip array)
- Magic Sounds (AudioClip array)
- Human Model (GameObject)
- Werebear Model (GameObject)
- Transformation VFX (GameObject)
- Transformation Sound (AudioClip)

**GameUI:**
- Health Bar (Slider)
- Health Text (Text)
- Spirit Bar (Slider)
- Spirit Text (Text)
- Rage Bar (Slider)
- Rage Text (Text)
- Berserk Indicator (GameObject)
- Rune Cooldown Images (Image array)
- Rune Cooldown Texts (Text array)
- Quest Panel (GameObject)
- Quest Title (Text)
- Quest Description (Text)
- Quest Progress (Slider)
- Player (BainoviaCharacterController)
- Rune System (RuneMagicSystem)
- Quest System (QuestSystem)

**EnemyAI:**
- Health (Health component)
- Player (BainoviaCharacterController)
- Animator (Animator component)
- Patrol Points (Transform array)

**QuestSystem:**
- Faction System (FactionSystem)
- Player (BainoviaCharacterController)

**FactionSystem:**
- Player (BainoviaCharacterController)

**RuneMagicSystem:**
- Character Controller (BainoviaCharacterController)
- Audio Source (AudioSource)
- Rune VFX (GameObject array)
- Rune Sounds (AudioClip array)

**SkillTreeSystem:**
- Player (BainoviaCharacterController)
- Rune System (RuneMagicSystem)
- Faction System (FactionSystem)

**InventorySystem:**
- Player (BainoviaCharacterController)
- Rune System (RuneMagicSystem)

**DialogueSystem:**
- Player (BainoviaCharacterController)
- Faction System (FactionSystem)
- Quest System (QuestSystem)
- Dialogue UI (GameObject)

**SpiritCompanionSystem:**
- Player (BainoviaCharacterController)
- Spirit Spawn Point (Transform)

**CameraController:**
- Player Body (Transform)

**InteractionSystem:**
- Player Camera (Camera)
- Player (BainoviaCharacterController)
- Interaction Prompt (GameObject)
- Prompt Text (Text)

---

## Animation Setup

### Player Animator Parameters

Create these parameters in the Animator Controller:

| Parameter | Type | Description |
|-----------|------|-------------|
| Speed | Float | Movement speed (0-1) |
| IsGrounded | Bool | Is character on ground |
| Attack | Trigger | Light attack trigger |
| HeavyAttack | Trigger | Heavy attack trigger |
| IsBerserk | Bool | Is in berserk mode |
| IsDead | Bool | Is dead |
| Horizontal | Float | Horizontal input |
| Vertical | Float | Vertical input |

### Enemy Animator Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| IsWalking | Bool | Is walking/patrolling |
| IsRunning | Bool | Is running/chasing |
| IsAttacking | Bool | Is attacking |
| IsStunned | Bool | Is stunned |
| IsDead | Bool | Is dead |
| Attack | Trigger | Attack trigger |

---

## Testing Checklist

### Movement
- [ ] WASD movement works
- [ ] Shift to run works
- [ ] Jump works
- [ ] Gravity applies correctly
- [ ] Camera rotation works

### Combat
- [ ] Light attack (Left Click) deals damage
- [ ] Heavy attack (Right Click) deals damage + knockback
- [ ] Attack cooldown works
- [ ] Rage meter builds on damage dealt/taken
- [ ] Berserk mode activates (F key at full rage)
- [ ] Berserk mode ends after duration

### Rune Magic
- [ ] Rune attack (Q key) works
- [ ] Spirit projection (E key) works
- [ ] Spirit drain (R key hold) works
- [ ] Cooldowns work correctly
- [ ] Spirit essence regenerates

### Transformation
- [ ] Transform to werebear (T key) works
- [ ] Model swaps correctly
- [ ] Stats increase in werebear form
- [ ] Transform back to human works
- [ ] Cooldown applies

### UI
- [ ] Health bar updates correctly
- [ ] Spirit essence bar updates
- [ ] Rage meter updates
- [ ] Rune cooldowns display
- [ ] Quest panel shows active quest
- [ ] Interaction prompt appears

### Interaction
- [ ] Raycasting detects interactables
- [ ] E key interacts with objects
- [ ] Q key uses alternate interaction
- [ ] NPC dialogue starts
- [ ] Item pickup works

### Enemies
- [ ] Enemies patrol correctly
- [ ] Enemies chase player when detected
- [ ] Enemies attack when in range
- [ ] Enemies deal damage to player
- [ ] Enemies die when health reaches 0

### Quests
- [ ] Quests can be accepted
- [ ] Objectives update correctly
- [ ] Quests can be completed
- [ ] Rewards are given
- [ ] Reputation changes

### Save/Load
- [ ] Quest data saves
- [ ] Faction reputation saves
- [ ] Inventory saves
- [ ] Skill tree saves
- [ ] Spirit companions save/load

---

## Common Issues

**Scripts not compiling:**
- Ensure all scripts are in `Assets/Scripts/` folder
- Check for missing using statements
- Verify script file names match class names

**NullReferenceException:**
- Check all Inspector references are assigned
- Verify GameObjects exist in scene
- Check GetComponent calls

**NavMesh not working:**
- Bake NavMesh after placing objects
- Ensure ground is Navigation Static
- Check NavMeshAgent is enabled

**UI not displaying:**
- Check Canvas is active
- Verify Render Mode is correct
- Check RectTransform anchors

**Audio not playing:**
- Verify AudioSource is active
- Check audio clips are assigned
- Ensure audio files are imported correctly

---

## Performance Optimization

### Graphics
- Use LODs for character models
- Implement occlusion culling
- Use texture compression
- Limit real-time lights

### Scripting
- Use object pooling for projectiles/VFX
- Minimize Update() calls
- Use coroutines for timed events
- Cache GetComponent calls

### Audio
- Use compressed audio formats
- Limit simultaneous audio sources
- Implement audio streaming for music

---

## Build Settings

### Windows Build
1. Go to **File > Build Settings**
2. Platform: **Windows**
3. Architecture: **x86_64**
4. Click **Build**

### Settings
- Resolution: `1920x1080`
- Fullscreen: `Windowed`
- Quality: `High`
- VSync: `Count = 1`

---

## Next Steps

1. Create prototype level with terrain
2. Implement basic enemy encounters
3. Add first quest chain
4. Create NPC dialogue
5. Implement skill tree UI
6. Add inventory UI
7. Create first boss encounter
8. Implement save/load system
9. Add main menu
10. Polish and optimize

---

## Contact & Support

For issues or questions, refer to:
- Design Documents in `01_Design_Documents/`
- Script comments in `04_Code/Scripts/`
- Unity Documentation: https://docs.unity3d.com/

---

*Last Updated: June 2026*
