# Unity 3D Terrain Shooter with Procedural Generation from Llama

## Project Overview

This project combines first-person shooting mechanics with procedural terrain generation and enemy AI systems. Key components include:

1. Advanced player movement with wall jumping
2. Projectile-based weapon system
3. Enemy AI with chasing behavior
4. Perlin noise terrain generation
5. Rule-based prefab placement system
6. Custom level generation editor tools

## Script Documentation

### 1. Player Controller (`PlayerMovement.cs` + `PlayerShooting.cs`)

**Features:**
- WASD movement with adjustable speed
- Advanced jumping:
  - Ground jumps
  - Wall jumps
- Physics-based movement
- Projectile shooting system
- Fire point targeting

**Debug Logs:**
- Movement inputs and velocity
- Jump states (ground/wall)
- Projectile instantiation
- Collision detection

### 2. Enemy System (`EnemyMovement.cs` + `EnemyHealth.cs`)

**Features:**
- Player chasing behavior
- Damage taking system
- Health management
- Death sequence

**Debug Logs:**
- Movement direction calculations
- Damage taken events
- Death triggers

### 3. Procedural Terrain Generator (`TerrainGenerator.cs`)

**Features:**
- Perlin noise heightmap generation
- Dynamic mesh creation
- Configurable terrain parameters:
  - Size
  - Resolution
  - Noise scale
  - Height intensity

**Debug Logs:**
- Terrain generation start/complete
- Vertex calculations
- Mesh construction

### 4. Prefab Placement System (`PrefabPlacer.cs`)

**Features:**
- Noise-based object distribution
- Configurable spacing rules
- Terrain-adaptive positioning

**Debug Logs:**
- Placement attempts
- Position validation
- Instantiation events

### 5. Level Generator Editor (`LevelGeneratorEditor.cs`)

**Features:**
- Custom Unity editor window
- Combined terrain + object controls
- One-click generation
- Real-time parameter adjustment

## Setup Instructions

1. **Player Setup:**
   - Attach both `PlayerMovement` and `PlayerShooting` scripts
   - Create required child objects:
     - "FirePoint" (for shooting)
     - "WallCheck" (for wall jumps)
   - Assign projectile prefab

2. **Enemy Setup:**
   - Attach `EnemyMovement` and `EnemyHealth` scripts
   - Assign player reference
   - Set health values

3. **Terrain Setup:**
   - Create empty GameObject
   - Attach `TerrainGenerator` script
   - Configure size/resolution parameters

4. **Prefab Placement:**
   - Attach `PrefabPlacer` to same GameObject
   - Assign prefabs and spacing values

5. **Editor Tools:**
   - Access via `Tools > Level Generator`
   - Adjust all parameters in one window

## Debugging Tools

All scripts include comprehensive debug logging:

- Player:
  - Movement states
  - Jump availability
  - Shooting events

- Enemies:
  - Chase behavior
  - Health changes
  - Death sequence

- Terrain:
  - Generation progress
  - Vertex calculations
  - Mesh construction

Visual debug helpers:
- Ground check indicators
- Wall detection markers
- Enemy path visualization
- Terrain bounds display

## Customization Guide

### Movement Tuning:
```csharp
// In PlayerMovement.cs
public float moveSpeed = 10f;       // Base movement speed
public float jumpForce = 10f;       // Jump power
public float gravity = -9.81f;      // Gravity strength