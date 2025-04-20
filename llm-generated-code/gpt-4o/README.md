# Unity FPS Prototype from GPT-4o

A modular 3D first-person prototype in Unity featuring:
- First-person movement (walk, jump, wall jump)
- Projectile-based shooting
- Enemy AI with health and damage
- Procedural terrain generation using Perlin noise
- Environment prefab spawning (trees, rocks, enemies)

---

## Scripts Overview

### 1. `PlayerMovement.cs`
Handles player:
- Walking & jumping
- Wall jumping with wall detection
- Shooting projectiles on left-click

**Inputs:**
- `WASD` or Arrow keys: Move
- `Space`: Jump / Wall Jump
- `Left Click`: Shoot

**Inspector Fields:**
- `Move Speed`, `Jump Height`, `Gravity`, `Wall Jump Force`
- `Projectile Prefab`, `Shoot Point` (empty GameObject at camera/gun tip)

---

### 2. `Projectile.cs`
A basic projectile that:
- Launches forward on spawn
- Damages enemies on collision
- Destroys itself after a duration

**Components Required:**
- Rigidbody (no gravity)
- Collider (isTrigger = true)

---

### 3. `EnemyAI.cs`
Simple enemy AI using NavMesh:
- Follows player
- Takes damage from projectiles
- Dies when health ≤ 0

**Inspector Fields:**
- `Health`, `Move Speed`

**Requires:**
- NavMeshAgent component
- Player tagged as `"Player"`
- Enemy tagged as `"Enemy"`

---

### 4. `ProceduralTerrainGenerator.cs`
Generates a voxel-style terrain grid using Perlin noise.

**Features:**
- Generates terrain height based on Perlin noise
- Places trees, rocks, and enemies based on height and random chance

**Inspector Fields:**
- `Width`, `Depth`, `Scale`, `Height Multiplier`
- `Terrain Block Prefab`, `Tree Prefabs[]`, `Rock Prefabs[]`, `Enemy Prefab`
- `Prefab Placement Rules`: Min/Max heights + chance

**Usage:**
- Attach to an empty GameObject
- Drag all required prefabs in Inspector
- Hit Play to generate the level

---

## Setup Instructions

1. Create a Unity 3D project
2. Import all scripts and create the required prefabs:
   - Terrain block (e.g., cube)
   - Tree and rock models
   - Enemy prefab with `EnemyAI`
   - Projectile prefab with `Projectile`
3. Tag your player object as `"Player"`
4. Add a NavMesh and bake it for enemy navigation
5. Create an empty GameObject (e.g., `LevelGenerator`) and attach `ProceduralTerrainGenerator.cs`
6. Assign all fields in the Inspector
7. Press Play and start shooting some enemies on a fresh world!



