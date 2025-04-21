# LLM-Repozitorijs

# Unity 3D First-Person Shooter with Procedural Generation

## Project Overview

This project combines first-person shooter mechanics with procedural level generation to create a dynamic 3D game experience. The system includes:

1. First-person player controller with advanced movement
2. Weapon and projectile system
3. Enemy AI with chasing and attacking behaviors
4. Procedural terrain generation using Perlin noise
5. Rule-based prefab placement system

## Script Documentation

### 1. First-Person Player Controller (`FirstPersonShooter.cs`)

**Features:**
- WASD movement with walking/running
- Mouse look with adjustable sensitivity
- Advanced jumping:
  - Ground jumps
  - Air jumps (double/triple jump)
  - Wall jumping
- Weapon system:
  - Projectile shooting
  - Ammo management
  - Reloading
  - Sound effects

**Debug Logs:**
- Movement inputs
- Jump states (ground/air/wall)
- Weapon firing and reloading
- Ammo changes
- Collision detection

### 2. Projectile System (`Projectile.cs`)

**Features:**
- Physics-based movement
- Damage application
- Impact effects
- Automatic cleanup

**Debug Logs:**
- Projectile creation
- Velocity and damage values
- Collision events
- Enemy hits

### 3. Enemy AI (`EnemyAI.cs`)

**Features:**
- Patrol behavior with waypoints
- Player detection and chasing
- Attack system
- Health and damage taking
- Death effects

**Debug Logs:**
- State changes (patrol/chase/attack)
- Damage taken
- Attack events
- Death sequence

### 4. Procedural Level Generator (`ProceduralLevelGenerator.cs`)

**Features:**
- Perlin noise terrain generation
- Heightmap texture support
- Rule-based prefab placement:
  - Height constraints
  - Slope constraints
  - Density control
  - Tag requirements
- Player spawn system

**Debug Logs:**
- Generation parameters
- Prefab placement attempts
- Placement rules validation
- Player spawn location

### 5. Prefab Placement Rules (`PrefabPlacementRule` Class)

**Configuration Options:**
- Prefab reference
- Maximum instances
- Placement radius
- Height range
- Slope range
- Surface alignment
- Required tags

## Setup Instructions

1. **Player Setup:**
   - Attach `FirstPersonShooter` to player GameObject
   - Assign camera and weapon muzzle transforms
   - Configure movement and weapon settings

2. **Projectile Setup:**
   - Create projectile prefab with:
     - Rigidbody
     - Collider (trigger)
     - `Projectile` script
     - Visual mesh

3. **Enemy Setup:**
   - Create enemy prefab with:
     - NavMeshAgent
     - Collider
     - `EnemyAI` script
     - Assign "Enemy" tag
   - Set patrol points if needed

4. **Level Generation:**
   - Add `ProceduralLevelGenerator` to empty GameObject
   - Configure terrain size and noise settings
   - Set up prefab placement rules
   - Assign player prefab

5. **Navigation:**
   - Bake NavMesh (Window > AI > Navigation)
   - Mark walkable areas

## Debugging Tools

All scripts include comprehensive debug logging that displays in the Unity Console:

- Player movement states
- Weapon firing events
- Enemy behavior transitions
- Projectile collisions
- Prefab placement results

Visual debug helpers:
- Ground check spheres
- Wall detection indicators
- Enemy detection ranges
- Terrain bounds visualization

## Customization Guide

### Movement Tuning:
- Adjust `walkSpeed`, `runSpeed` in `FirstPersonShooter`
- Modify jump forces (`jumpForce`, `wallJumpVerticalForce`)
- Tweak mouse sensitivity

### Combat Balancing:
- Change `projectileDamage` in player controller
- Adjust enemy `maxHealth` and `attackDamage`
- Modify fire rates and ammo counts

### Level Generation:
- Experiment with Perlin noise `scale` and `heightMultiplier`
- Create custom placement rules for different prefabs
- Adjust density through `maxCount` and `placementRadius`

## Credits

This system was developed as a comprehensive Unity solution combining:
- Advanced first-person controls
- Robust AI behaviors
- Sophisticated procedural generation
- Detailed debugging tools

