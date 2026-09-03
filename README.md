# BAINOVIA

A dark-fantasy game design project set in a divided world — the storm-ravaged northern kingdom of **Bainovia** and the sophisticated southern kingdom of **Spikslia** — where ancient forces threaten to reignite a centuries-old conflict.

> Status: pre-production / design & prototype stage

---

## Setting

Two civilizations separated by geography, culture, and ideology:

- **Bainovia (The North)** — subarctic, perpetual storms, aurora skies. Home to the **Bainovians**, werebear-like tribal humanoids (7–9 ft tall) organized into clans across the Frost Peaks, Storm Valley, Spirit Caves, and Frozen Forest.
- **Spikslia (The South)** — a sophisticated kingdom of cities including **Fez Al-Madinah, Meknes El-Kebir, and Chefchaouen El-Azraq**, plus deserts and oases.
- A **border region** of crossroads, ruined gates, and shrines where the two worlds meet.

## Core systems

- **Combat** — combat system design
- **Rune magic** — a rune-based magic system
- **Factions** — the political/faction structure of the world
- **Enemies** — an enemy bestiary (corrupted beasts, rune-mutated, frost giants, sand wraiths, guardians, bosses)
- **Character & worldbuilding** — full lore documents

## Repository structure

```
Bainovia/
├── 01_Design_Documents/   Game design docs (GDD, worldbuilding, combat, magic, factions, bestiary)
├── 02_Art_Assets/         Characters (concepts, models, textures), environments, URSIAN character pack
├── 03_Audio/              Music, SFX, voice, ambient
├── 04_Code/               Gameplay scripts and tools
├── 05_Level_Design/       Whiteboxes, greyboxes, final levels, level docs
├── 06_Cinematics/         Sequencer cutscenes, camera setups, storyboards
├── 07_Production/         Milestones, tasks, meetings, reports
└── 08_Builds/             Playable prototypes & builds
```

## Large assets (Git LFS)

A few oversized binaries are versioned via **Git LFS** (GitHub's 100 MB file limit):

- `08_Builds/proto.glb` (~111 MB)
- `02_Art_Assets/Characters/Untitled.blend` (~110 MB)
- `02_Art_Assets/Characters/New folder/7d59....glb` (~79 MB)

Clone with LFS support:

```bash
git lfs install
git clone <repo-url>
```

## Design documents

- [Bainovia_GDD.md](Bainovia_GDD.md) — game design document
- [Worldbuilding.md](Worldbuilding.md)
- [Character_Design.md](Character_Design.md)
- [Combat_System.md](Combat_System.md)
- [Magic_Runes_System.md](Magic_Runes_System.md)
- [Faction_System.md](Faction_System.md)
- [Enemy_Bestiary.md](Enemy_Bestiary.md)
- [Project_Structure.md](Project_Structure.md) — full folder conventions & naming

---

*Bainovia — a werebear-humanoid dark-fantasy world.*
