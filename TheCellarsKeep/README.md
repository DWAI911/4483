# The Cellar's Keep - Unity Setup Guide

## Project Overview
First-Person Survival Horror Roguelite built in Unity.

---

## PHASE 1: Unity Project Creation

### Step 1.1 - Create New Unity Project
1. Open **Unity Hub**
2. Click **"New Project"**
3. Select **"3D"** template (NOT "3D Core" or "URP" - just "3D")
4. Project Name: `TheCellarsKeep`
5. Location: `C:\GitHub\4483\TheCellarsKeep`
6. Click **"Create Project"**

### Step 1.2 - Configure Project Settings
Once Unity opens:
1. Go to **Edit → Project Settings → Player**
2. Under **Other Settings → Rendering**:
   - Set **Color Space** to `Linear`
3. Under **Other Settings → Api Compatibility Level**:
   - Set to `.NET Standard 2.1`

### Step 1.3 - Create Folder Structure
In the **Project** window (bottom), right-click `Assets` and create these folders:

```
Assets/
├── Scripts/
│   ├── Player/
│   ├── AI/
│   ├── LevelGeneration/
│   ├── Items/
│   ├── UI/
│   ├── Audio/
│   ├── GameSystems/
│   └── Utilities/
├── Prefabs/
│   ├── Player/
│   ├── Enemies/
│   ├── Items/
│   ├── Rooms/
│   └── Props/
├── Materials/
├── Audio/
│   ├── SFX/
│   └── Music/
├── Scenes/
├── ScriptableObjects/
│   └── Consumables/
└── Textures/
```

To create: Right-click Assets → Create → Folder, then name it. Create subfolders the same way.

---

## PHASE 2: Tags and Layers

### Step 2.1 - Create Tags
1. Go to **Edit → Project Settings → Tags and Layers**
2. Click the `+` button under **Tags** to add these:

| Tag Name |
|----------|
| Player |
| Enemy |
| Interactable |
| Item |

### Step 2.2 - Create Layers
1. Stay in **Tags and Layers** window
2. Expand **Layers** section
3. Find empty slots (Layer 6-10) and name them:

| Layer Number | Layer Name |
|--------------|------------|
| 6 | Ground |
| 7 | Player |
| 8 | Enemy |
| 9 | Interactable |
| 10 | Item |

---

## PHASE 3: Scripts Setup

### Step 3.1 - Verify Scripts Are Present
1. In Project window, navigate to `Assets/Scripts/`
2. You should see all the `.cs` files already created
3. If any folders are empty, let me know which scripts are missing

---

## PHASE 4: Create the Player

### Step 4.1 - Create Player GameObject
1. In **Hierarchy** (left side), right-click → **Create Empty**
2. Rename it to `Player`
3. Set **Position** to `(0, 1, 0)` in the Inspector

### Step 4.2 - Add CharacterController
1. Select `Player` in Hierarchy
2. Click **Add Component** button in Inspector
3. Search for `Character Controller` and add it
4. Set these values:
   - Center: `(0, 0, 0)`
   - Radius: `0.4`
   - Height: `1.8`

### Step 4.3 - Add PlayerController Script
1. Select `Player`
2. Click **Add Component** → search `PlayerController`
3. Click the script to add it

### Step 4.4 - Create Player Camera
1. Right-click `Player` in Hierarchy → **Camera**
2. Rename it to `PlayerCamera`
3. Set **Position** to `(0, 0.6, 0)` (eye height)
4. In Inspector, right-click **Audio Listener** component → **Remove Component**

### Step 4.5 - Create Flashlight
1. Right-click `PlayerCamera` → **Light** → **Spot Light**
2. Rename it to `Flashlight`
3. Set these values in Inspector:
   - Position: `(0, 0, 0.1)`
   - Rotation: `(0, 0, 0)`
   - Range: `20`
   - Spot Angle: `60`
   - Intensity: `2`
   - Color: Click the color box and set RGB to `(255, 240, 200)` for warm light

### Step 4.6 - Link Camera and Flashlight to Script
1. Select `Player` in Hierarchy
2. In Inspector, find **PlayerController** component
3. Drag `PlayerCamera` from Hierarchy into the **Camera Transform** slot
4. Drag `Flashlight` from Hierarchy into the **Flashlight** slot

### Step 4.7 - Add PlayerInteract Script
1. Select `Player`
2. **Add Component** → search `PlayerInteract`
3. Drag `PlayerCamera` into the **Player Camera** slot

### Step 4.8 - Add PlayerInventory Script
1. Select `Player`
2. **Add Component** → search `PlayerInventory`

### Step 4.9 - Set Player Tag and Layer
1. Select `Player` in Hierarchy
2. At the top of Inspector, click the **Layer** dropdown → select `Player`
3. Click **Apply to Children**
4. Click the **Tag** dropdown → select `Player`

### Step 4.10 - Create Player Prefab
1. In Project window, open `Assets/Prefabs/Player/`
2. Drag `Player` from Hierarchy into this folder
3. When prompted, click **Original Prefab**
4. You can now delete `Player` from Hierarchy (we'll spawn it later)

---

## PHASE 5: Create Test Ground

### Step 5.1 - Create Ground Plane
1. In Hierarchy, right-click → **3D Object** → **Plane**
2. Rename to `Ground`
3. Set Position: `(0, 0, 0)`, Scale: `(5, 1, 5)`
4. At the top of Inspector, set **Layer** to `Ground`

### Step 5.2 - Create Test Lighting
1. Right-click Hierarchy → **Light** → **Directional Light**
2. Set Intensity to `0.3` (dim for horror atmosphere)

---

## PHASE 6: Create AI Enemy (Basic)

### Step 6.1 - Create Enemy GameObject
1. In Hierarchy, right-click → **3D Object** → **Capsule**
2. Rename to `AI_Chaser`
3. Set Position: `(5, 1, 5)` (away from player spawn)
4. Set Layer to `Enemy`

### Step 6.2 - Add NavMeshAgent
1. Select `AI_Chaser`
2. **Add Component** → search `Nav Mesh Agent`
3. Set:
   - Speed: `3.5`
   - Angular Speed: `120`
   - Acceleration: `8`

### Step 6.3 - Add AIChaser Script
1. **Add Component** → search `AIChaser`
2. For **Eye Position**, create an empty child:
   - Right-click `AI_Chaser` → **Create Empty**
   - Rename to `EyePosition`
   - Set Position: `(0, 0.5, 0)` (where eyes would be)
   - Drag `EyePosition` into the **Eye Position** slot in AIChaser script

### Step 6.4 - Add Catch Collider
1. Select `AI_Chaser`
2. **Add Component** → search `Sphere Collider`
3. Check **Is Trigger**
4. Set Radius: `1.5`
5. Set Tag to `Enemy`

### Step 6.5 - Create Enemy Prefab
1. Open `Assets/Prefabs/Enemies/`
2. Drag `AI_Chaser` from Hierarchy into folder
3. Delete from Hierarchy after saving

---

## PHASE 7: Setup NavMesh (Required for AI Movement)

### Step 7.1 - Mark Ground as Walkable
1. Select `Ground` in Hierarchy
2. In Inspector, click the **Static** dropdown (top right)
3. Check **Navigation Static**

### Step 7.2 - Bake NavMesh
1. Open **Window** → **AI** → **Navigation**
2. Click the **Bake** tab
3. Click **Bake** button at bottom
4. You should see a blue overlay on the ground (this is the walkable area)

---

## PHASE 8: Create First Room Prefab (Test Room)

### Step 8.1 - Create Room GameObject
1. Create empty GameObject, name it `Room_Corridor`
2. Set Position: `(0, 0, 0)`

### Step 8.2 - Create Floor
1. Right-click `Room_Corridor` → **3D Object** → **Plane**
2. Rename to `Floor`
3. Set Position: `(0, 0, 0)`
4. Set Scale: `(1, 1, 1)`
5. Set Layer to `Ground`
6. Mark as **Navigation Static**

### Step 8.3 - Create Walls
1. Right-click `Room_Corridor` → **3D Object** → **Cube**
2. Rename to `Wall_North`
3. Set Position: `(0, 2, -5)`, Scale: `(10, 4, 0.5)`
4. Repeat for other walls:
   - `Wall_South`: Position `(0, 2, 5)`, Scale `(10, 4, 0.5)`
   - `Wall_East`: Position `(5, 2, 0)`, Scale `(0.5, 4, 10)`
   - `Wall_West`: Position `(-5, 2, 0)`, Scale `(0.5, 4, 10)`

### Step 8.4 - Add Room Script
1. Select `Room_Corridor`
2. **Add Component** → search `Room`
3. In the Room component, set **Room Type** to `Corridor`

### Step 8.5 - Create Spawn Points
Create empty children for spawn locations:

**Item Spawn Points:**
1. Create empty child named `ItemSpawnPoints`
2. Create empty children inside it at positions where items can spawn
3. Example: Position `(2, 0.5, 2)` and `(-2, 0.5, -2)`

**Hiding Spot Spawn Points:**
1. Create empty child named `HidingSpotSpawnPoints`
2. Create empty children inside for closet positions

**Enemy Spawn Point:**
1. Create empty child named `EnemySpawnPoint`
2. Set Position: `(3, 0, 3)`

### Step 8.6 - Save as Prefab
1. Open `Assets/Prefabs/Rooms/`
2. Drag `Room_Corridor` into folder
3. Delete from Hierarchy

---

## PHASE 9: Create Item Prefabs

### Step 9.1 - Create Key Prefab
1. Create **3D Object** → **Cube** (or import a key model)
2. Rename to `Key`
3. Scale: `(0.3, 0.3, 0.1)`
4. **Add Component** → `KeyItem` script
5. **Add Component** → `Sphere Collider`, check **Is Trigger**, Radius `0.5`
6. Set **Layer** to `Item`, **Tag** to `Item`
7. Drag to `Assets/Prefabs/Items/`

### Step 9.2 - Create Fuse Prefab
1. Create **3D Object** → **Cylinder`
2. Rename to `Fuse`
3. Scale: `(0.2, 0.3, 0.2)`
4. **Add Component** → `FuseItem` script
5. **Add Component** → `Sphere Collider`, check **Is Trigger**
6. Set Layer to `Item`, Tag to `Item`
7. Drag to `Assets/Prefabs/Items/`

### Step 9.3 - Create Fear Essence Prefab
1. Create **3D Object** → **Sphere**
2. Rename to `FearEssence`
3. Scale: `(0.2, 0.2, 0.2)`
4. **Add Component** → `FearEssenceItem` script
5. Set **Size** to `Small` in the script
6. **Add Component** → `Sphere Collider`, check **Is Trigger**
7. Set Layer to `Item`, Tag to `Item`
8. Drag to `Assets/Prefabs/Items/`

---

## PHASE 10: Create Consumable ScriptableObjects

### Step 10.1 - Create Stamina Pill
1. In Project window, go to `Assets/ScriptableObjects/Consumables/`
2. Right-click → **Create** → **Items** → **Consumable Item**
3. Name it `StaminaPill`
4. In Inspector, set:
   - Item Name: `Stamina Pill`
   - Description: `Fully restores your stamina.`
   - Cost: `15`
   - Type: `StaminaPill`
   - Effect Value: `100`

### Step 10.2 - Create Flashbang
1. Create another Consumable Item, name it `Flashbang`
2. Set:
   - Item Name: `Flashbang`
   - Description: `Stuns the creature for 3 seconds.`
   - Cost: `25`
   - Type: `Flashbang`
   - Effect Duration: `3`

### Step 10.3 - Create Decoy
1. Create another, name it `Decoy`
2. Set:
   - Item Name: `Decoy`
   - Description: `Creates a noise distraction for the creature.`
   - Cost: `20`
   - Type: `Decoy`

---

## PHASE 11: Create Game Managers

### Step 11.1 - Create GameStateManager
1. Create empty GameObject, name it `GameStateManager`
2. **Add Component** → `GameStateManager` script
3. This will persist between scenes (singleton)

### Step 11.2 - Create AudioManager
1. Create empty GameObject, name it `AudioManager`
2. **Add Component** → `AudioManager` script

### Step 11.3 - Create AtmosphereController
1. Create empty GameObject, name it `AtmosphereController`
2. **Add Component** → `AtmosphereController` script

---

## PHASE 12: Create UI (Basic)

### Step 12.1 - Create Canvas
1. In Hierarchy, right-click → **UI** → **Canvas`
2. Set **Render Mode** to `Screen Space - Overlay`

### Step 12.2 - Create HUD Panel
1. Right-click Canvas → **UI** → **Panel`
2. Rename to `HUD`
3. Set anchor to **Top-Left** (click the square in Inspector, hold Shift+Alt, click top-left)
4. Set Position: `(10, -10, 0)`

### Step 12.3 - Create Stamina Bar
1. Right-click HUD → **UI** → **Slider`
2. Rename to `StaminaBar`
3. Set Position: `(0, 0, 0)`, Size: `(200, 20)`
4. Delete the "Handle Slide Area" child (not needed)
5. In Slider component, uncheck "Interactable"

### Step 12.4 - Create Item Count Texts
1. Right-click HUD → **UI** → **Text - TextMeshPro`
2. (If prompted, click "Import TMP Essentials")
3. Rename to `KeyCount`
4. Set text to `Keys: 0`
5. Repeat for `FuseCount` and `EssenceCount`

### Step 12.5 - Create Interaction Prompt
1. Right-click Canvas → **UI** → **Text - TextMeshPro**
2. Name it `InteractionPrompt`
3. Set anchor to **Bottom-Center**
4. Position: `(0, 50, 0)`
5. Set font size to `24`, alignment to center

### Step 12.6 - Create Crosshair
1. Right-click Canvas → **UI** → **Image**
2. Name it `Crosshair`
3. Set anchor to **Center**
4. Position: `(0, 0, 0)`
5. Set a small white dot or crosshair sprite

### Step 12.7 - Add GameHUD Script
1. Select Canvas
2. **Add Component** → `GameHUD` script
3. Link all UI elements to their slots in the Inspector

---

## PHASE 13: Create Main Scene

### Step 13.1 - Create Game Scene
1. **File** → **New Scene**
2. Save as `Assets/Scenes/Game.unity`

### Step 13.2 - Add Required Objects
Add these to the scene (either instantiate prefabs or create new):
1. `Ground` (Plane with Ground layer, Navigation Static)
2. `Player` (from Prefabs/Player/)
3. `AI_Chaser` (from Prefabs/Enemies/)
4. `Directional Light`
5. `GameStateManager`
6. `AudioManager`
7. `AtmosphereController`
8. `Canvas` with UI
9. `LevelGenerator` (optional for now)

### Step 13.3 - Bake NavMesh
1. Select Ground
2. Open **Window** → **AI** → **Navigation**
3. Click **Bake**

### Step 13.4 - Test the Game
1. Press **Play** button
2. You should be able to:
   - Move with WASD
   - Look around with mouse
   - Run with Shift (watch stamina bar)
   - Toggle flashlight with F

---

## PHASE 14: Build for Windows

### Step 14.1 - Configure Build
1. **File** → **Build Settings**
2. Click **Add Open Scenes** to add your Game scene
3. Select **PC, Mac & Linux Standalone**
4. Target Platform: **Windows**
5. Architecture: **x86_64**

### Step 14.2 - Build
1. Click **Build**
2. Create folder: `C:\GitHub\4483\TheCellarsKeep\Builds\Windows\`
3. Name the executable: `TheCellarsKeep.exe`

---

## WHAT TO TELL ME NEXT

After completing each phase, tell me:

### After PHASE 4 (Player):
> "I finished setting up the player. What should I test?"

### After PHASE 6 (Enemy):
> "I created the enemy. How do I make it chase me?"

### After PHASE 7 (NavMesh):
> "NavMesh is baked. The enemy still doesn't move."

### After PHASE 8 (Room Prefab):
> "I created a room prefab. How do I test level generation?"

### After PHASE 11 (Game Managers):
> "All managers are set up. What's missing?"

### After PHASE 12 (UI):
> "UI is created but nothing updates. How do I link it?"

### After PHASE 13 (Testing):
> "I tested and [DESCRIBE THE PROBLEM]. Can you help debug?"

### For Final Polish:
> "The game runs but I need [SPECIFIC FEATURE]. Can you add that?"

### For Build Issues:
> "Build failed with error: [COPY THE ERROR]. How do I fix it?"

---

## COMMON ISSUES & FIXES

| Problem | Solution |
|---------|----------|
| Player falls through ground | Make sure Ground has `Ground` layer, PlayerController has Ground Mask set to `Ground` |
| Enemy doesn't move | Make sure NavMesh is baked, NavMeshAgent is added |
| Scripts won't compile | Check Console for errors, make sure all scripts are in correct folders |
| Can't see anything | Add a Directional Light or increase ambient light |
| Cursor not locked | Make sure PlayerController runs and Cursor.lockState is set |
| UI not showing | Check Canvas is set to Screen Space - Overlay |

---

## NEXT STEPS AFTER BASIC SETUP

Once you have the basic game running, tell me:

> "The basic game works. Now I need you to help me add [FEATURE]."

Options to add:
1. Procedural level generation with multiple rooms
2. Shop system between deaths
3. Lore notes and story elements
4. Multiple enemy types
5. Sound effects and music
6. Particle effects for items
7. Post-processing (Vignette, Bloom, etc.)
8. Save/Load system
9. Main Menu scene
10. Win/Death screen polish

---

Good luck! Tell me when you complete each phase and I'll guide you to the next step.
