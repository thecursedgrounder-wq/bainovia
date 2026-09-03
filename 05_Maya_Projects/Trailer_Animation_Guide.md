# BAINOVIA - Maya Animation Guide for Trailer

**Project:** Bainovia Official Trailer  
**Software:** Autodesk Maya 2024+  
**Duration:** 2:30  
**Target:** Cinematic quality, AAA standard

---

## Table of Contents

1. [Project Setup](#project-setup)
2. [Character Rigging](#character-rigging)
3. [Animation Workflow](#animation-workflow)
4. [Scene-by-Scene Guide](#scene-by-scene-guide)
5. [Camera Work](#camera-work)
6. [VFX and Particles](#vfx-and-particles)
7. [Lighting and Rendering](#lighting-and-rendering)
8. [Export and Compositing](#export-and-compositing)

---

## Project Setup

### 1. Maya Project Structure

```
Bainovia_Trailer/
├── scenes/
│   ├── act01_storm_awakens/
│   ├── act02_guardian/
│   ├── act03_war/
│   ├── act04_power/
│   └── act05_choice/
├── assets/
│   ├── characters/
│   │   ├── kaelar_human/
│   │   ├── kaelar_werebear/
│   │   ├── spirits/
│   │   └── enemies/
│   ├── environments/
│   │   ├── bainovia_mountains/
│   │   ├── spirit_cave/
│   │   ├── borderlands/
│   │   ├── spikslia/
│   │   └── ancient_shrine/
│   └── props/
├── rigs/
├── animation/
├── cache/
└── renders/
```

### 2. Project Settings

**Units:**
- Linear: **Centimeter**
- Angular: **Degree**
- Time: **Film (24 fps)**

**Render Settings:**
- Renderer: **Arnold**
- Resolution: **1920x1080** (HD) or **3840x2160** (4K)
- Pixel Aspect Ratio: **Square (1.0)**
- Frame Range: Per scene (see Scene Guide)

### 3. Reference Setup

**Import Reference Images:**
1. Create image planes for character design references
2. Import concept art for environments
3. Load trailer script as reference in Maya

**Playblast Settings:**
- Format: **QuickTime** or **mp4**
- Codec: **H.264**
- Resolution: **Half** or **Full**
- Display Quality: **High**

---

## Character Rigging

### Kaelar - Human Form

**Rig Requirements:**
- Height: 6'2" (188 cm)
- Poly Count: 80,000-120,000 triangles
- Rig Type: **Humanoid with custom controls**

**Joint Hierarchy:**
```
Root
├── Hips
│   ├── Spine
│   │   ├── Spine1
│   │   │   ├── Spine2
│   │   │   │   ├── Spine3
│   │   │   │   │   ├── Neck
│   │   │   │   │   │   ├── Head
│   │   │   │   │   │   │   ├── Jaw
│   │   │   │   │   │   │   ├── LeftEye
│   │   │   │   │   │   │   └── RightEye
│   │   │   │   │   │   └── LeftEar
│   │   │   │   │   │   └── RightEar
│   │   │   │   ├── LeftShoulder
│   │   │   │   │   ├── LeftArm
│   │   │   │   │   │   ├── LeftForeArm
│   │   │   │   │   │   │   ├── LeftHand
│   │   │   │   │   │   │   │   ├── [Fingers]
│   │   │   │   │   └── RightShoulder...
│   │   │   ├── LeftUpLeg
│   │   │   │   ├── LeftLeg
│   │   │   │   │   ├── LeftFoot
│   │   │   │   │   │   └── LeftToeBase
│   │   │   └── RightUpLeg...
```

**Custom Controls:**
- **Root Control:** Full body translation/rotation
- **Hips Control:** Hip movement, spine twist
- **Chest Control:** Upper body orientation
- **Head Control:** Head orientation, facial expressions
- **Shoulder Controls:** Shoulder independent movement
- **Arm Controls:** IK/FK switching
- **Hand Controls:** Finger curl, spread
- **Leg Controls:** IK/FK switching, foot roll
- **Toe Controls:** Toe curl, pivot

**Facial Rig:**
- **Jaw:** Open/close, side-to-side
- **Eyebrows:** Raise/lower, furrow
- **Eyes:** Look around, blink
- **Mouth:** Smile, frown, lip sync
- **Ears:** Slight movement (werebear trait)

**Runes:**
- Create joint chain for rune glow effect
- Parent to appropriate body parts (forehead, chest, shoulders, forearms)
- Add controls for rune intensity/pulse

### Kaelar - Werebear Form

**Rig Requirements:**
- Height: 8'5" (256 cm)
- Poly Count: 100,000-150,000 triangles
- Rig Type: **Quadruped with bipedal upper body**

**Additional Joints:**
- **Tail:** 5-7 joint chain for tail movement
- **Ears:** 2 joints each for ear mobility
- **Snout:** Extended muzzle with jaw control
- **Claws:** Individual finger/toe claws (3 joints each)
- **Fur:** Simulation bones for fur movement

**Custom Controls:**
- All human controls plus:
- **Spine Hunch Control:** Adjust hunch posture
- **Arm Length Control:** Adjust arm reach (40% longer than human)
- **Leg Digitigrade Control:** Reverse-jointed leg control
- **Claw Extend Control:** Retract/extend claws
- **Fur Control:** Wind simulation, bristle effect

**Transformation Rig:**
- Create blendshape targets for transformation
- **Human → Werebear:** 3-5 blendshape stages
- **Werebear → Human:** Reverse sequence
- **Rune Glow:** Intensity ramp during transformation

### Spirits

**Rig Requirements:**
- Poly Count: 20,000-50,000 triangles
- Rig Type: **Floating/ethereal with particle controls**

**Joint Hierarchy:**
- Simple spine chain (3-5 joints)
- Floating limb controls (no IK)
- Particle emission points
- Glow/intensity controls

**Animation Style:**
- Floating, weightless movement
- Ethereal, ghost-like motion
- Particle trail effects
- Pulsing glow animation

---

## Animation Workflow

### 1. Animation Layers

**Layer Structure:**
1. **Base Layer:** Core movement, locomotion
2. **Combat Layer:** Attacks, dodges, reactions
3. **Magic Layer:** Rune casting, spirit abilities
4. **Facial Layer:** Expressions, lip sync
5. **FX Layer:** Rune glow, fur movement

### 2. Animation Principles

**For Kaelar (Human):**
- **Weight:** Heavy, deliberate movements
- **Timing:** Slow buildup, explosive release
- **Anticipation:** Clear wind-up for attacks
- **Follow-through:** Momentum carries through actions
- **Squash/Stretch:** Subtle for impact emphasis

**For Kaelar (Werebear):**
- **Weight:** Extremely heavy, powerful
- **Timing:** Slower but more devastating
- **Anticipation:** Major wind-up for heavy attacks
- **Follow-through:** Massive momentum, body recoil
- **Secondary Action:** Fur movement, rune pulse

**For Spirits:**
- **Weight:** Weightless, floating
- **Timing:** Smooth, continuous motion
- **Arcs:** Curved, flowing paths
- **Timing:** Slow, ethereal pace
- **Overlap:** Different body parts move at different rates

### 3. Animation Timeline

**Per Scene Frame Allocation:**

| Scene | Duration | Frames (24fps) | Key Animation Focus |
|-------|----------|----------------|---------------------|
| Act I | 0:45 | 1080 | Transformation, spirit awakening |
| Act II | 0:30 | 720 | Movement, sensing, combat intro |
| Act III | 0:30 | 720 | Combat montage, environments |
| Act IV | 0:25 | 600 | Rune magic, berserk, spirits |
| Act V | 0:20 | 480 | Climax, final choice |

### 4. Blocking to Polish Workflow

**Stage 1: Blocking (1-2 days per scene)**
- Create key poses only
- Focus on storytelling and timing
- Use simple geometry (proxy rig)
- No facial animation yet
- Rough camera work

**Stage 2: Splining (2-3 days per scene)**
- Convert stepped to spline tangents
- Refine timing and spacing
- Add breakdown poses
- Basic facial expressions
- Camera refinement

**Stage 3: Polish (3-5 days per scene)**
- Fine-tune easing
- Add secondary motion
- Facial animation polish
- Fur/simulation setup
- Final camera moves
- FX integration

---

## Scene-by-Scene Guide

### ACT I: THE STORM AWAKENS (0:00 - 0:45)

#### Scene 1: Bainovia Mountains (0:00 - 0:10)

**Camera:**
- Wide establishing shot
- Slow pan across mountain range
- Aurora timelapse effect
- Lightning flashes

**Animation:**
- Environment: Wind simulation on snow/ice
- Particles: Snow, aurora particles
- Lighting: Dynamic lightning flashes

**Maya Setup:**
1. Create mountain terrain with displacement map
2. Add particle system for aurora (nParticles)
3. Lightning: Area light with intensity animation
4. Wind: nCloth simulation on any cloth elements

#### Scene 2: Spirit Cave (0:10 - 0:20)

**Camera:**
- Medium shot of Kaelar
- Slow push in
- Focus on rune interaction

**Animation:**
- Kaelar: Slow approach to shrine
- Arm reach: Anticipation, contact, follow-through
- Facial: Curiosity, then realization
- Runes: Subtle glow increase

**Key Poses:**
1. Standing before shrine (frame 0)
2. Arm extension start (frame 60)
3. Hand contact with rune (frame 120)
4. Reaction to energy (frame 180)
5. Eyes widen (frame 240)

#### Scene 3: Forbidden Rune (0:20 - 0:30)

**Camera:**
- Close-up on Kaelar's face
- Slow zoom to eyes
- Rune reflection in eyes

**Animation:**
- Facial: Eyes widening, pupils dilating
- Runes: Pulsing glow intensity
- Hair: Wind simulation from energy surge
- Body: Subtle recoil from energy

**Maya Techniques:**
- Facial blendshapes for eye expression
- Shader animation for rune glow (ramp texture)
- nHair simulation for hair movement
- Light intensity animation for energy surge

#### Scene 4: Spirit Eruption (0:30 - 0:45)

**Camera:**
- Wide shot of cave
- Camera shake on explosion
- White flash transition

**Animation:**
- Spirits: Materialize from particles
- Energy: Explosion shockwave
- Kaelar: Recoil, shielding
- Cave: Structural shake

**Maya Setup:**
- Spirit materialization: nParticles with sprite shader
- Explosion: nParticles with radial velocity
- Camera shake: Camera animation with noise
- White flash: Full-screen plane with opacity animation

### ACT II: THE GUARDIAN (0:45 - 1:15)

#### Scene 5: Borderlands (0:45 - 0:55)

**Camera:**
- Tracking shot following Kaelar
- Low angle for heroic feel
- Slow push to canyon

**Animation:**
- Kaelar: Confident walk, heavy footsteps
- Clothing: Subtle cloth simulation
- Environment: Wind through canyon

**Walk Cycle:**
- Contact frame: Foot touches ground
- Down frame: Weight acceptance
- Passing frame: Body over supporting foot
- Up frame: Body rises
- Recoil frame: Foot leaves ground

#### Scene 6: Sensing Danger (0:55 - 1:05)

**Camera:**
- Medium shot, static
- Focus on Kaelar's reaction
- Slight push in on eyes

**Animation:**
- Kaelar: Stops, head turns, eyes glow
- Runes: Subtle pulse
- Body: Tension buildup

**Key Poses:**
1. Walking (frame 0)
2. Sudden stop (frame 12)
3. Head turn (frame 24)
4. Eyes glow (frame 36)
5. Combat stance (frame 48)

#### Scene 7: Corrupted Spirit Attack (1:05 - 1:15)

**Camera:**
- Quick cut to spirit
- Action camera, dynamic movement
- Slow motion on transformation

**Transformation Sequence:**
1. **Frame 0-12:** Spirit lunges
2. **Frame 12-24:** Kaelar reacts
3. **Frame 24-36:** Transformation begins
4. **Frame 36-48:** Mid-transformation (bones cracking)
5. **Frame 48-60:** Werebear form complete
6. **Frame 60-72:** Werebear attacks
7. **Frame 72-84:** Spirit destroyed

**Maya Techniques:**
- Transformation: Blendshape sequence with offset
- Bone cracking: Jitter animation on skeleton
- Fur: nHair simulation with sudden growth
- Runes: Intensity ramp animation
- Slow motion: Time warping in animation curve

### ACT III: THE WAR (1:15 - 1:45)

#### Scene 8: Bainovia Combat Montage (1:15 - 1:25)

**Camera:**
- Rapid cuts, dynamic angles
- Action camera style
- Slow motion on impacts

**Animation:**
- Kaelar (werebear): Multiple combat scenarios
- Enemies: Various attack patterns
- Environment: Destruction effects

**Combat Moves:**
1. **Claw Combo:** 3-hit chain, increasing damage
2. **Heavy Slam:** Charged attack, area damage
3. **Thunder Slam:** Ground pound with shockwave
4. **Counter:** Perfect timing, devastating

**Maya Setup:**
- Combat: Layered animation (base + combat)
- Destruction: nCloth or rigid body simulation
- Shockwave: nParticles with radial field
- Camera: Shake on impacts

#### Scene 9: Spikslia Montage (1:25 - 1:35)

**Camera:**
- Warm color grading
- Slower, more peaceful pacing
- Establishing shots of architecture

**Animation:**
- Kaelar (human): Walking through city
- NPCs: Daily activities
- Environment: Lantern light, market activity

**Maya Setup:**
- Architecture: Modular building pieces
- Lanterns: Point lights with intensity animation
- NPCs: Simple walk cycles, idle animations
- Cloth: nCloth simulation on clothing

#### Scene 10: Battlefield (1:35 - 1:45)

**Camera:**
- Wide shot of armies
- Low angle, epic scale
- Kaelar in center

**Animation:**
- Armies: Battle formations, charging
- Kaelar: Standing between sides, conflicted
- Environment: Dust, debris, chaos

**Maya Setup:**
- Armies: Instanced geometry with crowd simulation
- Dust: nParticles with turbulence
- Debris: Rigid body simulation
- Camera: Subtle handheld shake

### ACT IV: THE POWER (1:45 - 2:10)

#### Scene 11: Rune Magic (1:45 - 1:55)

**Camera:**
- Close-ups on rune effects
- Dynamic camera moves
- Slow motion on spell casts

**Rune Animations:**

**Storm Rune:**
- Claws glow blue
- Lightning chains between enemies
- Impact: Electrical discharge

**Frost Rune:**
- Ice spikes erupt from ground
- Enemies freeze in place
- Shatter effect on frozen enemies

**Wind Rune:**
- Shockwave expands outward
- Enemies knocked back
- Dust/debris swirl

**Spirit Rune:**
- Ethereal spirits surround Kaelar
- Shield formation
- Absorption effect

**Maya Setup:**
- Rune glow: Shader animation with ramp
- Lightning: nParticles with lightning shader
- Ice spikes: Geometry with growth animation
- Shockwave: nParticles with radial velocity
- Spirit shield: nParticles with sprite shader

#### Scene 12: Berserk Mode (1:55 - 2:05)

**Camera:**
- Aggressive, dynamic movement
- Quick cuts
- Extreme close-ups

**Animation:**
- Kaelar: Rage buildup, berserk activation
- Fur: Bristle effect
- Eyes: Intense glow
- Combat: Devastating, fast-paced

**Berserk Activation:**
1. Rage meter fills (UI overlay)
2. Eyes glow intensely
3. Fur bristles
4. Roar animation
5. Speed increase
6. Combat begins

**Maya Techniques:**
- Fur bristle: nHair simulation with stiffness
- Eye glow: Shader intensity animation
- Roar: Facial blendshapes + jaw animation
- Speed: Time scale adjustment

#### Scene 13: Spirit Companions (2:05 - 2:10)

**Camera:**
- Medium shot of Kaelar + spirits
- Spirits orbiting around
- Focus on spirit abilities

**Animation:**
- Spirits: Summon animation, orbiting
- Kaelar: Command gestures
- Abilities: Each spirit's unique power

**Maya Setup:**
- Summon: Particle materialization effect
- Orbit: Motion path animation
- Abilities: nParticles specific to each spirit type

### ACT V: THE CHOICE (2:10 - 2:30)

#### Scene 14: Ancient Shrine (2:10 - 2:20)

**Camera:**
- Epic wide shot
- Low angle, looking up at spirits
- Slow push in

**Animation:**
- Spirits: Towering, massive scale
- Kaelar: Small, human form, looking up
- Runes: Intense glow
- Environment: Energy crackling

**Maya Setup:**
- Spirits: Scaled up geometry
- Energy: nParticles with electrical shader
- Runes: Maximum intensity
- Camera: Shake from energy

#### Scene 15: Kaelar's Face (2:20 - 2:25)

**Camera:**
- Extreme close-up
- Shallow depth of field
- Focus on eyes/runes

**Animation:**
- Facial: Conflict, determination
- Eyes: Glowing intensely
- Runes: Pulsing with heartbeat

**Maya Setup:**
- Facial: Blendshape animation
- Eyes: Shader glow animation
- Runes: Pulse animation synced to heartbeat
- DOF: Camera depth of field

#### Scene 16: Rapid Montage (2:25 - 2:30)

**Camera:**
- Rapid cuts (6-12 frames per shot)
- Action-packed
- Building to climax

**Animation:**
- Quick cuts from previous scenes
- Emphasis on most impactful moments
- Building tension

**Maya Setup:**
- Edit in Maya or post-production
- Use playblast for timing reference
- Final edit in compositing software

#### Scene 17: Final Shot (2:30)

**Camera:**
- Epic wide shot
- Kaelar (werebear) on mountain peak
- Lightning storm behind
- Slow zoom out

**Animation:**
- Kaelar: Standing tall, roaring
- Runes: Maximum glow
- Fur: Blowing in wind
- Lightning: Frequent flashes

**Maya Setup:**
- Roar: Full body animation + facial
- Lightning: Multiple light sources with animation
- Wind: nHair simulation
- Camera: Slow zoom out with shake

---

## Camera Work

### Camera Types

**Establishing Shot:**
- Wide angle lens (24-35mm)
- Shows environment scale
- Sets scene context

**Medium Shot:**
- Normal lens (50mm)
- Shows character from waist up
- Standard dialogue/interaction

**Close-Up:**
- Telephoto lens (85-135mm)
- Shows facial expressions
- Emotional emphasis

**Action Shot:**
- Wide to medium (35-50mm)
- Dynamic movement
- Follows action

**Extreme Close-Up:**
- Long telephoto (200mm+)
- Shows details (eyes, runes)
- Intense focus

### Camera Movement

**Static:**
- No movement
- Used for tension, stillness

**Pan:**
- Horizontal rotation
- Reveals environment

**Tilt:**
- Vertical rotation
- Shows scale (up/down)

**Dolly/Tracking:**
- Follows subject
- Creates depth

**Zoom:**
- Focal length change
- Draws attention

**Crane/Boom:**
- Vertical movement
- Epic reveals

**Handheld:**
- Subtle shake
- Adds realism, tension

**Camera Shake:**
- Violent shake
- Impact, explosion

### Camera Animation in Maya

**Setup:**
1. Create camera group
2. Add aim group (look at target)
3. Add parent for movement
4. Set keyframes for position/rotation

**Techniques:**
- Use graph editor for smooth curves
- Add noise for handheld shake
- Use motion paths for complex moves
- Parent constraints for object tracking

---

## VFX and Particles

### nParticles Setup

**Spirit Materialization:**
```
Emitter: Point emitter
Rate: 1000 particles/sec
Lifespan: 2 seconds
Velocity: Radial, low speed
Shader: Sprite with glow texture
Color: Blue/white gradient
Opacity: Fade in/out
```

**Lightning Effect:**
```
Emitter: Curve emitter
Rate: 500 particles/sec
Lifespan: 0.5 seconds
Velocity: Along curve, high speed
Shader: Lightning texture
Color: Electric blue
Glow: High intensity
```

**Explosion:**
```
Emitter: Point emitter
Rate: 5000 particles/sec (burst)
Lifespan: 1 second
Velocity: Radial, very high speed
Turbulence: High
Color: Orange/white
```

**Snow:**
```
Emitter: Volume emitter
Rate: 200 particles/sec
Lifespan: 5 seconds
Velocity: Downward, slow
Turbulence: Low
Color: White
Size: Small, varied
```

### nCloth Simulation

**Clothing:**
- Create nCloth from clothing mesh
- Set collision object (character body)
- Adjust stiffness, damping
- Add wind field for movement

**Fur:**
- Use nHair for fur simulation
- Set up hair system
- Add turbulence field
- Adjust stiffness, damping
- Animate wind for movement

### Shader Animation

**Rune Glow:**
- Use ramp texture for glow
- Animate ramp offset
- Connect to intensity
- Pulse with sine wave

**Eye Glow:**
- Incandescence map
- Animate intensity
- Color shift for different states

---

## Lighting and Rendering

### Arnold Renderer Setup

**Render Settings:**
- Renderer: Arnold
- Samples: 4 (preview), 8 (final)
- Max Bounces: 3 (diffuse), 1 (specular)
- Ray Depth: 5
- AA Samples: 3 (preview), 5 (final)

**Lighting:**

**Bainovia (Cold):**
- Key Light: Cool blue (0.6, 0.7, 1.0)
- Fill Light: Dim blue-gray
- Rim Light: White/cyan
- Environment: HDRI of snowy mountains

**Spikslia (Warm):**
- Key Light: Warm gold (1.0, 0.8, 0.6)
- Fill Light: Orange/terracotta
- Rim Light: Warm white
- Environment: HDRI of desert city

**Light Types:**
- **Directional Light:** Sun/moon
- **Area Light:** Windows, lanterns
- **Point Light:** Lamps, magic effects
- **Spot Light:** Focused illumination
- **Volume Light:** Atmospheric beams

### Light Linking

**Selective Lighting:**
- Link lights to specific objects
- Exclude objects from lights
- Use light linker for control

### Light Animation

**Lightning:**
- Animate intensity (0 to 10)
- Quick on/off
- Random timing

**Lanterns:**
- Subtle intensity flicker
- Color temperature shift
- Cast shadow animation

**Magic Effects:**
- Point light at effect origin
- Intensity animation
- Color animation
- Decay rate

### Render Layers

**Layer Setup:**
1. **Beauty:** Full render
2. **Character:** Characters only
3. **Environment:** Background only
4. **Shadows:** Shadow pass
5. **Reflection:** Reflection pass
6. **Specular:** Specular pass
7. **Z-Depth:** Depth pass
8. **Motion Vector:** Motion blur pass

### AOVs (Arbitrary Output Variables)

**Essential AOVs:**
- **RGBA:** Beauty
- **Albedo:** Color without lighting
- **Direct Diffuse:** Direct lighting
- **Indirect Diffuse:** Indirect lighting
- **Specular:** Specular highlights
- **Transmission:** Transmitted light
- **Emission:** Self-illumination
- **Z:** Depth
- **Motion Vector:** Motion blur

---

## Export and Compositing

### Export Settings

**Animation Export:**
- Format: **Alembic (.abc)**
- Frame Range: Per scene
- Selection: Character only
- Options: Include attributes

**Render Export:**
- Format: **OpenEXR (.exr)**
- Bit Depth: **16-bit half**
- Compression: **ZIP**
- Layers: Separate AOVs

**Camera Export:**
- Format: **FBX** or **Maya ASCII**
- Include: Camera, aim, parent

### Compositing Workflow

**Software:** Nuke, After Effects, or Fusion

**Compositing Steps:**
1. Import render layers and AOVs
2. Color grade each layer
3. Combine layers with proper blending
4. Add depth of field (from Z-depth)
5. Add motion blur (from motion vectors)
6. Add film grain
7. Color grade final composite
8. Add titles/text

**Color Grading:**
- **Bainovia:** Cool, desaturated, blue tint
- **Spikslia:** Warm, saturated, orange/gold tint
- **Spirits:** Ethereal, glowing, blue/white

### Final Output

**Format:** **QuickTime (.mov)** or **mp4**
**Codec:** **ProRes 4444** (high quality) or **H.264** (web)
**Resolution:** **1920x1080** (HD) or **3840x2160** (4K)
**Frame Rate:** **24 fps**
**Audio:** **PCM** (uncompressed) or **AAC** (compressed)

---

## Timeline and Production Schedule

### Pre-Production (2 weeks)
- Week 1: Rig setup, character blocking
- Week 2: Environment blocking, camera setup

### Production (6 weeks)
- Week 1-2: Act I animation
- Week 3: Act II animation
- Week 4: Act III animation
- Week 5: Act IV animation
- Week 6: Act V animation

### Post-Production (3 weeks)
- Week 1: Lighting, rendering
- Week 2: Compositing, VFX
- Week 3: Color grading, final output

**Total:** 11 weeks

---

## Tips and Best Practices

### Animation
- Reference real footage for weight and timing
- Use mirror for facial expressions
- Record video reference for complex actions
- Polish silhouettes first, then details
- Check animation from multiple camera angles

### Performance
- Use proxy rigs for blocking
- Hide unnecessary geometry
- Use display layers for organization
- Cache simulations before rendering
- Use render layers for complex scenes

### Workflow
- Save frequently with incremental versions
- Use playblast for quick preview
- Use graph editor for smooth curves
- Use dope sheet for timing adjustments
- Backup project daily

### Collaboration
- Use naming conventions (scene_name_v01.ma)
- Use reference objects for scale
- Use playblasts for review
- Use version control (Git/Perforce)
- Document changes in readme

---

## Troubleshooting

**Common Issues:**

**Animation jitter:**
- Check tangent types (use spline/auto)
- Check for double keys
- Check graph editor for spikes

**Skinning issues:**
- Check joint influences
- Use smooth bind
- Paint weights for problem areas
- Use helper joints for deformation

**Simulation issues:**
- Check collision objects
- Adjust substeps
- Check nucleus settings
- Cache simulation

**Rendering issues:**
- Check light samples
- Check ray depth
- Check render samples
- Use progressive rendering for preview

**Performance issues:**
- Reduce polygon count
- Use proxy geometry
- Disable unnecessary calculations
- Use render layers

---

## Resources

**Maya Documentation:**
- Autodesk Maya Help
- Arnold Renderer Documentation
- Maya Learning Channel

**Reference:**
- Animation survival kit
- Animator's survival kit
- The Illusion of Life
- Character Animation Crash Course

**Inspiration:**
- Arcane (Netflix)
- The Witcher 3 cinematics
- God of War (2018) cinematics
- Diablo IV cinematics

---

*Maya Animation Guide Version 1.0*  
*Last Updated: June 2026*
