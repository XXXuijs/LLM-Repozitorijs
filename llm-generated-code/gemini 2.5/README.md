# Unity FPS & Procedural Level Foundation Scripts by Gemini

This repository/folder contains a set of C# scripts for Unity, providing a foundation for a first-person game featuring procedural level generation. The scripts cover player movement, shooting mechanics, basic enemy AI, and level creation.

**Note:** These scripts were developed conceptually around April 20, 2025. Ensure compatibility with your Unity version.

## Features

* **First-Person Movement:** Smooth character controller-based movement including walking, jumping, and wall jumping.
* **Shooting System:** Basic projectile firing mechanism triggered by player input.
* **Enemy AI:** Simple enemies that detect and follow the player using Unity's NavMesh system, capable of taking damage.
* **Procedural Terrain:** Generation of 3D terrain meshes using multi-octave Perlin noise.
* **Rule-Based Prefab Placement:** System for scattering prefabs (trees, rocks, enemies, etc.) onto the generated terrain based on configurable rules like height, slope, and density.
* **Debugging:** All scripts include detailed `Debug.Log` messages viewable in the Unity Console to aid in understanding execution flow and troubleshooting.

## Scripts Overview

Here's a breakdown of each script and its purpose:

---

### 1. `FirstPersonMovement.cs`

* **Purpose:** Handles all aspects of player movement in a first-person perspective.
* **Features:**
    * Ground movement (Forward/Backward/Strafe) via WASD/Arrow Keys.
    * Jumping.
    * Applies gravity.
    * Wall Jumping: Allows jumping off surfaces tagged correctly.
* **Requires:**
    * `CharacterController` component on the Player GameObject.
* **Key Inspector Settings:**
    * `Move Speed`: Player walking speed.
    * `Jump Height`: How high the player jumps.
    * `Gravity`: Downward force applied when airborne.
    * `Wall Tag`: The tag assigned to surfaces the player can wall jump from (e.g., "Wall").
    * `Wall Jump Upward Force`: Vertical force of the wall jump.
    * `Wall Jump Outward Force`: Horizontal force pushing away from the wall during a wall jump.
* **Setup:**
    * Attach to the main Player GameObject.
    * Ensure a `CharacterController` component is also attached and configured (height, radius, center).
    * Tag any GameObjects intended for wall jumping with the string specified in `Wall Tag`.

---

### 2. `ShootingController.cs`

* **Purpose:** Manages the player's ability to fire projectiles.
* **Features:**
    * Detects input ("Fire1", typically Left Mouse Button).
    * Instantiates a projectile prefab at a specified spawn point.
* **Requires:**
    * A `Camera` in the scene tagged as "MainCamera".
* **Key Inspector Settings:**
    * `Projectile Prefab`: The GameObject prefab to be instantiated as a projectile. (Must have `Projectile.cs` script).
    * `Projectile Spawn Point`: An empty `Transform` (child of the camera or weapon) indicating where projectiles originate.
* **Setup:**
    * Attach to the Player GameObject (or a dedicated weapon child object).
    * Assign the projectile prefab to `Projectile Prefab`.
    * Create an empty GameObject (e.g., "Muzzle") parented to the player's camera or weapon model, position it appropriately, and assign its transform to `Projectile Spawn Point`.
    * Ensure the main player camera is tagged "MainCamera".

---

### 3. `Projectile.cs`

* **Purpose:** Defines the behavior of individual projectiles after being fired.
* **Features:**
    * Moves forward automatically based on its `Speed`.
    * Destroys itself after a set `Lifetime` or upon collision.
    * Detects collisions and attempts to apply `Damage` to objects with an `EnemyAI` script.
* **Requires:**
    * `Rigidbody` component (configure gravity/kinematics as needed).
    * `Collider` component (e.g., SphereCollider, CapsuleCollider).
* **Key Inspector Settings:**
    * `Speed`: Forward velocity of the projectile.
    * `Lifetime`: Duration in seconds before the projectile self-destructs.
    * `Damage`: Amount of damage dealt upon hitting an `EnemyAI`.
* **Setup:**
    * Attach this script to your projectile prefab (e.g., a small Sphere or custom model).
    * Add and configure a `Rigidbody` (disable gravity for bullets, enable for grenades; consider `Continuous Dynamic` collision detection for fast projectiles).
    * Ensure a `Collider` is present.
    * Set the `Speed`, `Lifetime`, and `Damage` values.

---

### 4. `EnemyAI.cs`

* **Purpose:** Provides basic artificial intelligence for enemy characters.
* **Features:**
    * Health system (`Max Health`, `TakeDamage()` method).
    * Player detection within a specified `Detection Radius`.
    * Follows the player using Unity's NavMesh system (`NavMeshAgent`).
    * Stops at a defined `Attack Range` (stopping distance).
    * Dies and gets destroyed when health reaches zero.
* **Requires:**
    * `NavMeshAgent` component on the Enemy GameObject.
    * `Collider` component on the Enemy GameObject.
    * A baked NavMesh present in the scene (`Window -> AI -> Navigation -> Bake`).
    * The Player GameObject must have the tag specified in `Player Tag` (default "Player").
* **Key Inspector Settings:**
    * `Max Health`: Starting and maximum health points.
    * `Detection Radius`: Range within which the enemy starts following the player.
    * `Attack Range`: How close the enemy tries to get to the player (sets NavMeshAgent stopping distance).
    * `Player Tag`: The tag used to find the player GameObject.
* **Setup:**
    * Attach to your enemy prefab.
    * Add a `NavMeshAgent` component and configure its speed, size, etc.
    * Ensure a `Collider` is present.
    * Set health and detection parameters.
    * Ensure your Player GameObject is tagged correctly (e.g., "Player").
    * **Crucially:** Bake a NavMesh in your scene after placing static geometry (or generated terrain).

---

### 5. `ProceduralLevelGenerator.cs`

* **Purpose:** Generates a level layout procedurally, including terrain and object placement.
* **Features:**
    * Creates a 3D terrain mesh based on multi-octave Perlin noise.
    * Allows configuration of noise parameters (scale, octaves, etc.) and terrain height.
    * Places prefabs onto the generated terrain based on customizable rules defined in the Inspector.
    * Placement rules include constraints for height, slope, density/count, and clearance from other objects.
    * Uses a `Seed` for reproducible level generation.
    * Option to generate in the Editor via Context Menu or automatically on Start.
* **Requires:**
    * Materials and Prefabs assigned in the Inspector settings.
    * Layers defined in the project for terrain and potentially placed objects (for raycasting and overlap checks).
* **Key Inspector Settings:**
    * `Seed`: Controls the random generation outcome.
    * `Generate On Start`: If checked, generates the level when the scene starts playing.
    * Terrain Settings (`Map Width`, `Map Depth`, `Noise Scale`, `Octaves`, `Persistence`, `Lacunarity`, `Terrain Height Multiplier`, `Height Curve`, `Terrain Material`, `Terrain Layer`).
    * `Placement Rules` (Array): Define rules for each prefab type.
        * `Prefab`: The prefab to place.
        * `Spawn Attempts`: How many times to try placing this prefab.
        * `Spawn Probability`: Chance (0-1) to place if rules are met.
        * `Min/Max Slope`, `Min/Max Height`: Placement constraints.
        * `Placement Clearance`: Minimum distance to other placed objects.
        * `Align To Surface Normal`, `Random Y Rotation`: Placement orientation options.
    * `Placement Overlap Layer`: Layers to check against when testing for placement clearance.
* **Setup:**
    * Attach to an empty GameObject in your scene (e.g., "LevelGenerator").
    * Configure all terrain parameters to achieve the desired landscape style.
    * Define layers for "Terrain" and potentially "PlacedObjects" in `Project Settings -> Tags and Layers`. Assign these layers in the script's Inspector.
    * Set up the `Placement Rules` array, assigning prefabs and defining constraints for each.
    * Assign a `Terrain Material`.
    * Use the `[ContextMenu("Generate Level")]` option (right-click script header in Inspector) or `Generate On Start`.
    * **Remember to manually bake the NavMesh** after generating the level if you need AI navigation.

---

## General Setup & Usage

1.  **Project Setup:** Create a new 3D Unity project. Import necessary assets (models, materials, etc.). Ensure the AI Navigation package is installed (`Window -> Package Manager`).
2.  **Create Core Objects:** Set up a Player GameObject, basic ground/platforms, and any walls needed for testing movement.
3.  **Attach Scripts:** Attach the relevant scripts (`FirstPersonMovement`, `ShootingController`, etc.) to the appropriate GameObjects (Player, Projectile Prefab, Enemy Prefab, LevelGenerator object).
4.  **Configure Inspectors:** Carefully assign all required references (prefabs, materials, spawn points, layers) in each script's Inspector panel. Tune parameters like speed, health, noise settings, etc.
5.  **Tagging & Layers:**
    * Tag the Player object (e.g., "Player").
    * Tag walls intended for wall jumping (e.g., "Wall").
    * Tag the main Camera ("MainCamera").
    * Define and assign Layers for terrain and potentially placed objects as needed by the `ProceduralLevelGenerator`.
6.  **NavMesh Baking:** For levels where `EnemyAI` needs to navigate (including procedurally generated ones), you **must** bake the NavMesh via `Window -> AI -> Navigation -> Bake` after the static geometry is defined or the procedural level is generated.
7.  **Testing:** Play the scene and use the Unity Console window (`Window -> General -> Console`) extensively to monitor the debug logs provided by the scripts.

## Dependencies

* Unity Engine (Developed conceptually for versions supporting `CharacterController`, `NavMeshAgent`, `Mesh` API - likely 2019.x or later).
* Unity AI Navigation package (for `NavMeshAgent` in `EnemyAI.cs`).
