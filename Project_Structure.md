# BAINOVIA — Project Structure

## Folder Organization

Complete folder structure for the Bainovia game project.

---

## Root Directory

```
Bainovia/
├── 01_Design_Documents/
├── 02_Art_Assets/
├── 03_Audio/
├── 04_Code/
├── 05_Level_Design/
├── 06_Cinematics/
├── 07_Production/
└── 08_Builds/
```

---

## 01_Design_Documents/

Contains all game design documentation.

```
01_Design_Documents/
├── Bainovia_GDD.md
├── Worldbuilding.md
├── Character_Design.md
├── Combat_System.md
├── Magic_Runes_System.md
├── Faction_System.md
├── Enemy_Bestiary.md
├── Quest_Design.md
├── Narrative_Design.md
└── Technical_Design.md
```

---

## 02_Art_Assets/

Contains all visual assets.

```
02_Art_Assets/
├── Characters/
│   ├── Protagonist/
│   │   ├── Concepts/
│   │   ├── Models/
│   │   ├── Textures/
│   │   ├── Rigging/
│   │   └── Animations/
│   ├── NPCs/
│   │   ├── Bainovians/
│   │   ├── Spikslans/
│   │   └── Spirits/
│   └── Enemies/
│       ├── Corrupted_Beasts/
│       ├── Rune_Mutated/
│       ├── Frost_Giants/
│       ├── Sand_Wraiths/
│       ├── Humans/
│       ├── Guardians/
│       └── Bosses/
├── Environments/
│   ├── Bainovia/
│   │   ├── Frost_Peaks/
│   │   ├── Frozen_Forest/
│   │   ├── Spirit_Caves/
│   │   ├── Rune_Canyons/
│   │   └── Storm_Hold/
│   ├── Spikslia/
│   │   ├── Fez_Al_Madinah/
│   │   ├── Meknes_El_Kebir/
│   │   ├── Chefchaouen_El_Azraq/
│   │   ├── Desert/
│   │   └── Oasis/
│   └── Border_Region/
│       ├── Crossroads/
│       ├── Ruined_Gate/
│       └── Storm_Shrine/
├── Props/
│   ├── Weapons/
│   ├── Armor/
│   ├── Furniture/
│   ├── Vehicles/
│   └── Miscellaneous/
├── UI/
│   ├── HUD/
│   ├── Menus/
│   ├── Icons/
│   └── Loading_Screens/
├── VFX/
│   ├── Particles/
│   ├── Shaders/
│   └── Post_Processing/
└── Concepts/
    ├── Characters/
    ├── Environments/
    ├── Props/
    └── UI/
```

---

## 03_Audio/

Contains all audio assets.

```
03_Audio/
├── Music/
│   ├── Combat/
│   ├── Exploration/
│   ├── Cinematic/
│   ├── Menu/
│   └── Boss/
├── SFX/
│   ├── Combat/
│   ├── Movement/
│   ├── Environment/
│   ├── UI/
│   └── Magic/
├── Voice/
│   ├── Protagonist/
│   ├── NPCs/
│   ├── Enemies/
│   └── Narrator/
└── Ambient/
    ├── Bainovia/
    ├── Spikslia/
    └── Spirit_Realm/
```

---

## 04_Code/

Contains all source code.

```
04_Code/
├── Unreal_Project/
│   ├── Source/
│   │   ├── Bainovia/
│   │   │   ├── Characters/
│   │   │   ├── Combat/
│   │   │   ├── Magic/
│   │   │   ├── AI/
│   │   │   ├── Quests/
│   │   │   ├── Factions/
│   │   │   ├── UI/
│   │   │   ├── SaveSystem/
│   │   │   └── Utilities/
│   │   └── BainoviaEditor/
│   ├── Config/
│   ├── Content/
│   └── Plugins/
├── Scripts/
│   ├── Python/
│   └── Lua/
└── Tools/
    ├── Asset_Builder/
    ├── Level_Streamer/
    └── Data_Parser/
```

---

## 05_Level_Design/

Contains all level files and design documents.

```
05_Level_Design/
├── Whiteboxes/
│   ├── Bainovia_Levels/
│   ├── Spikslia_Levels/
│   └── Boss_Arenas/
├── Greyboxes/
│   ├── Bainovia_Levels/
│   ├── Spikslia_Levels/
│   └── Boss_Arenas/
├── Final_Levels/
│   ├── Bainovia_Levels/
│   ├── Spikslia_Levels/
│   └── Boss_Arenas/
├── Level_Docs/
│   ├── Bainovia/
│   ├── Spikslia/
│   └── Boss_Arenas/
└── Streaming/
    ├── World_Partition/
    └── Level_Instances/
```

---

## 06_Cinematics/

Contains all cinematic assets.

```
06_Cinematics/
├── Sequencer/
│   ├── Cutscenes/
│   ├── Dialogue_Scenes/
│   └── Intro_Outro/
├── Camera_Setups/
│   ├── Combat_Cameras/
│   ├── Exploration_Cameras/
│   └── Cinematic_Cameras/
└── Storyboards/
    ├── Main_Story/
    ├── Side_Quests/
    └── Character_Moments/
```

---

## 07_Production/

Contains production management files.

```
07_Production/
├── Schedules/
│   ├── Milestones/
│   ├── Sprints/
│   └── Deadlines/
├── Task_Tracking/
│   ├── Jira/
│   ├── Trello/
│   └── Asana/
├── Meetings/
│   ├── Design/
│   ├── Art/
│   ├── Code/
│   └── Production/
├── Reports/
│   ├── Weekly/
│   ├── Monthly/
│   └── Milestone/
└── Documentation/
    ├── Processes/
    ├── Guidelines/
    └── Standards/
```

---

## 08_Builds/

Contains game builds for testing and distribution.

```
08_Builds/
├── Development/
│   ├── Windows/
│   ├── PlayStation/
│   └── Xbox/
├── Alpha/
│   ├── Windows/
│   ├── PlayStation/
│   └── Xbox/
├── Beta/
│   ├── Windows/
│   ├── PlayStation/
│   └── Xbox/
├── Release/
│   ├── Windows/
│   ├── PlayStation/
│   └── Xbox/
└── Patches/
    ├── v1.0/
    ├── v1.1/
    └── v1.2/
```

---

## File Naming Conventions

### Assets
- **Characters:** `CHAR_[Name]_[Variant]` (e.g., `CHAR_Kaelar_Base`)
- **Enemies:** `ENEM_[Type]_[Variant]` (e.g., `ENEM_ThunderWolf_01`)
- **Weapons:** `WEP_[Name]_[Type]` (e.g., `WEP_StormClaw_Melee`)
- **Armor:** `ARM_[Name]_[Slot]` (e.g., `ARM_Tribal_Chest`)
- **Props:** `PROP_[Category]_[Name]` (e.g., `PROP_Furniture_Table`)
- **Materials:** `MAT_[Category]_[Name]` (e.g., `MAT_Character_Fur`)
- **Textures:** `TEX_[Category]_[Name]_[Type]` (e.g., `TEX_Character_Kaelar_Diffuse`)

### Audio
- **Music:** `MUS_[Category]_[Name]` (e.g., `MUS_Combat_MainTheme`)
- **SFX:** `SFX_[Category]_[Action]` (e.g., `SFX_Combat_ClawHit`)
- **Voice:** `VOX_[Character]_[Line]` (e.g., `VOX_Kaelar_Intro_001`)
- **Ambient:** `AMB_[Location]_[Type]` (e.g., `AMB_Bainovia_Wind`)

### Code
- **Classes:** `B_[Category][Name]` (e.g., `BCharacterKaelar`)
- **Functions:** `[Verb][Noun]` (e.g., `PerformAttack`)
- **Variables:** `[b][Noun]` (e.g., `bIsAttacking`)

### Levels
- **Whitebox:** `WB_[Region]_[Area]` (e.g., `WB_Bainovia_FrostPeaks`)
- **Greybox:** `GB_[Region]_[Area]` (e.g., `GB_Bainovia_FrostPeaks`)
- **Final:** `L_[Region]_[Area]` (e.g., `L_Bainovia_FrostPeaks`)

---

## Version Control

### Branch Strategy
- **main:** Production-ready builds
- **develop:** Integration branch
- **feature/[name]:** Feature development
- **bugfix/[name]:** Bug fixes
- **hotfix/[name]:** Emergency fixes

### Commit Message Format
```
[type]: [description]

[type] options:
- feat: New feature
- fix: Bug fix
- docs: Documentation
- style: Code style
- refactor: Code refactoring
- test: Testing
- chore: Maintenance

Example:
feat: Add rune magic system
fix: Resolve collision issues in Frost Peaks
docs: Update combat system documentation
```

---

## Asset Pipeline

### Maya → Unreal
1. Model in Maya
2. Rig and animate
3. Export as FBX
4. Import to Unreal
5. Create materials
6. Set up physics
7. Test in game

### Photoshop → Unreal
1. Create texture in Photoshop
2. Export as PNG/TGA
3. Import to Unreal
4. Create material
5. Apply to mesh
6. Test in game

### Audio → Unreal
1. Record/create audio
2. Edit in DAW
3. Export as WAV/OGG
4. Import to Unreal
5. Set up sound cues
6. Test in game

---

## Quality Assurance

### Asset Checklist
- [ ] Correct naming convention
- [ ] Proper file format
- [ ] Correct resolution
- [ ] Optimized poly count
- [ ] Proper UV layout
- [ ] Correct material setup
- [ ] Tested in engine
- [ ] No errors in logs

### Code Checklist
- [ ] Follows coding standards
- [ ] Proper comments
- [ ] Error handling
- [ ] Optimized performance
- [ ] No memory leaks
- [ ] Tested in game
- [ ] No warnings in logs

---

## Backup Strategy

### Daily Backups
- Automated backup to cloud storage
- Incremental backups only
- Retain 30 days

### Weekly Backups
- Full backup to external drive
- Compressed archives
- Retain 12 weeks

### Milestone Backups
- Complete project backup
- Multiple storage locations
- Permanent retention

---

*Document Version: 1.0*  
*Last Updated: May 2026*
