# The Cellar's Keep - Unity Setup Guide

## Unity Version: 2022.3.62f1 (LTS)

---

## PHASE 1: Unity Project Creation

### Step 1.1 - Install Unity 2022.3.62f1
1. Open **Unity Hub**
2. Click **Installs** → **Install Editor**
3. Select **2022.3.62f1 (LTS)**
4. Install with these modules:
   - **Windows Build Support (IL2CPP)**
   - **Visual Studio** (or your preferred IDE)

### Step 1.2 - Create New Unity Project
1. Open **Unity Hub**
2. Click **"New Project"**
3. Select **"3D"** template (NOT "URP" or "HDRP" - just "3D (Built-in)")
4. Project Name: `TheCellarsKeep`
5. Location: `C:\GitHub\4483\TheCellarsKeep`
6. Click **"Create Project"**

### Step 1.3 - Configure Project Settings
Once Unity opens:
1. Go to **Edit → Project Settings → Player**
2. Under **Other Settings → Rendering**:
   - Set **Color Space** to `Linear`
3. Under **Other Settings → Api Compatibility Level**:
   - Set to `.NET Standard 2.1`
4. Under **Other Settings**:
   - Set **Scripting Backend** to `IL2CPP` (for Windows builds)

### Step 1.4 - Import TextMesh Pro (Required for UI)
1. Go to **Window → TextMesh Pro → Import TMP Essentials**
2. Click **Import** in the popup window
3. Wait for import to complete

### Step 1.5 - Create Folder Structure
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

---

## PHASE 2: Tags and Layers

### Step 2.1 - Create Tags
1. Go to **Edit → Project Settings → Tags and Layers**
2. Click the `+` button under **Tags**:

| Tag Name |
|----------|
| Player |
| Enemy |
| Interactable |
| Item |

### Step 2.2 - Create Layers
1. Stay in **Tags and Layers**
2. Expand **Layers**
3. Name these layers:

| Layer Number | Layer Name |
|--------------|------------|
| 6 | Ground |
| 7 | Player |
| 8 | Enemy |
| 9 | Interactable |
| 10 | Item |

---

## PHASE 3: Verify Scripts

### Step 3.1 - Check Script Compilation
1. Open Unity and wait for scripts to compile
2. Open **Console** window (**Window → General → Console**)
3. If there are RED errors, tell me immediately with the error text
4. YELLOW warnings are OK to ignore

---

## PHASE 4: Create the Player

### Step 4.1 - Create Player GameObject
1. In **Hierarchy**, right-click → **Create Empty**
2. Rename to `Player`
3. Set **Position** to `(0, 1, 0)`

### Step 4.2 - Add CharacterController
1. Select `Player`
2. **Add Component** → `Character Controller`
3. Set:
   - Center: `(0, 0, 0)`
   - Radius: `0.4`
   - Height: `1.8`

### Step 4.3 - Add PlayerController Script
1. Select `Player`
2. **Add Component** → `PlayerController`

### Step 4.4 - Create Player Camera
1. Right-click `Player` → **Camera**
2. Rename to `PlayerCamera`
3. Set **Position** to `(0, 0.6, 0)`
4. Right-click **Audio Listener** → **Remove Component**

### Step 4.5 - Create Flashlight
1. Right-click `PlayerCamera` → **Light** → **Spot Light**
2. Rename to `Flashlight`
3. Set:
   - Position: `(0, 0, 0.1)`
   - Rotation: `(0, 0, 0)`
   - Range: `20`
   - Spot Angle: `60`
   - Intensity: `2`
   - Color: RGB `(255, 240, 200)` - warm yellow

### Step 4.6 - Link Camera to Script
1. Select `Player`
2. In **PlayerController** component:
   - Drag `PlayerCamera` → **Camera Transform**
   - Drag `Flashlight` → **Flashlight**
   - Check `Ground` in **Ground Mask**

### Step 4.7 - Add More Scripts
1. Select `Player`
2. **Add Component** → `PlayerInteract`
   - Drag `PlayerCamera` → **Player Camera**
3. **Add Component** → `PlayerInventory`
4. **Add Component** → `PlayerFootsteps`

### Step 4.8 - Set Tag and Layer
1. Select `Player`
2. **Tag** → `Player`
3. **Layer** → `Player` → **Apply to Children**

### Step 4.9 - Save as Prefab
1. Open `Assets/Prefabs/Player/`
2. Drag `Player` from Hierarchy into folder
3. Delete `Player` from Hierarchy

---

## PHASE 5: Create Test Ground

### Step 5.1 - Create Ground
1. Hierarchy → right-click → **3D Object** → **Plane**
2. Rename to `Ground`
3. Position: `(0, 0, 0)`, Scale: `(5, 1, 5)`
4. **Layer** → `Ground`
5. Check **Static** checkbox (top right of Inspector)

### Step 5.2 - Add Lighting
1. Right-click Hierarchy → **Light** → **Directional Light**
2. Intensity: `0.3`

---

## PHASE 6: Create Enemy

### Step 6.1 - Create Enemy GameObject
1. Right-click Hierarchy → **3D Object** → **Capsule**
2. Rename to `AI_Chaser`
3. Position: `(5, 1, 5)`
4. **Layer** → `Enemy`

### Step 6.2 - Add NavMeshAgent
1. Select `AI_Chaser`
2. **Add Component** → `Nav Mesh Agent`
3. Set:
   - Speed: `3.5`
   - Angular Speed: `120`
   - Acceleration: `8`

### Step 6.3 - Add AIChaser Script
1. **Add Component** → `AIChaser`
2. Create eye position:
   - Right-click `AI_Chaser` → **Create Empty**
   - Name: `EyePosition`
   - Position: `(0, 0.5, 0)`
3. Drag `EyePosition` → **Eye Position** slot

### Step 6.4 - Add Catch Collider
1. **Add Component** → `Sphere Collider`
2. Check **Is Trigger**
3. Radius: `1.5`
4. **Tag** → `Enemy`

### Step 6.5 - Save Prefab
1. Drag `AI_Chaser` to `Assets/Prefabs/Enemies/`
2. Delete from Hierarchy

---

## PHASE 7: Setup NavMesh

### Step 7.1 - Mark Ground as Walkable
1. Select `Ground`
2. In Inspector, click **Static** dropdown
3. Check **Navigation Static**

### Step 7.2 - Bake NavMesh
1. **Window** → **AI** → **Navigation**
2. Click **Bake** tab
3. Click **Bake** button
4. You'll see blue overlay on ground (walkable area)

---

## PHASE 8: Create Game Scene

### Step 8.1 - Save Scene
1. **File** → **Save As**
2. Save to `Assets/Scenes/Game.unity`

### Step 8.2 - Add Game Managers
Create these GameObjects:

**GameStateManager:**
1. Create Empty → name `GameStateManager`
2. **Add Component** → `GameStateManager`

**AudioManager:**
1. Create Empty → name `AudioManager`
2. **Add Component** → `AudioManager`

**AtmosphereController:**
1. Create Empty → name `AtmosphereController`
2. **Add Component** → `AtmosphereController`

### Step 8.3 - Add Player from Prefab
1. Open `Assets/Prefabs/Player/`
2. Drag `Player` into Hierarchy

### Step 8.4 - Test the Game
1. Press **Play**
2. You should:
   - Move with **WASD**
   - Look around with **Mouse**
   - Run with **Left Shift** (watch stamina)
   - Toggle flashlight with **F**

---

## PHASE 9: Create Basic UI

### Step 9.1 - Create Canvas
1. Right-click Hierarchy → **UI** → **Canvas**
2. Canvas Scaler: **Scale With Screen Size**
3. Reference Resolution: `1920 x 1080`

### Step 9.2 - Create HUD Panel
1. Right-click Canvas → **UI** → **Panel**
2. Name: `HUD`
3. Set anchor to **Top-Left**
4. Delete the default **Image** component's source image (make it invisible)

### Step 9.3 - Create Stamina Bar
1. Right-click HUD → **UI** → **Slider**
2. Name: `StaminaBar`
3. Position: `X: 110, Y: -20`
4. Width: `200`, Height: `20`
5. Delete **Handle Slide Area** child
6. Uncheck **Interactable** in Slider component

### Step 9.4 - Create Item Count Texts
1. Right-click HUD → **UI** → **Text - TextMeshPro**
2. Name: `KeyCount`, Text: `Keys: 0`
3. Position: `X: 110, Y: -60`
4. Repeat for:
   - `FuseCount`: Position `X: 110, Y: -90`
   - `EssenceCount`: Position `X: 110, Y: -120`

### Step 9.5 - Create Interaction Prompt
1. Right-click Canvas → **UI** → **Text - TextMeshPro**
2. Name: `InteractionPrompt`
3. Anchor: **Bottom-Center**
4. Position: `X: 0, Y: 80`
5. Font Size: `24`
6. Alignment: **Center**

### Step 9.6 - Create Crosshair
1. Right-click Canvas → **UI** → **Image**
2. Name: `Crosshair`
3. Anchor: **Center**
4. Position: `(0, 0, 0)`
5. Set a small white dot sprite or leave empty

### Step 9.7 - Add GameHUD Script
1. Select Canvas
2. **Add Component** → `GameHUD`
3. Link UI elements:
   - Stamina Slider → `StaminaSlider`
   - Key Count Text → `KeyCountText`
   - Fuse Count Text → `FuseCountText`
   - Essence Count Text → `EssenceCountText`
   - Interaction Prompt → `InteractPrompt`
   - Crosshair → `Crosshair`

---

## PHASE 10: Test Complete Setup

### Step 10.1 - Verify Everything Works
1. Press **Play**
2. Check Console for errors
3. Test:
   - [ ] Player moves with WASD
   - [ ] Camera follows mouse
   - [ ] Shift makes you run (stamina drains)
   - [ ] F toggles flashlight
   - [ ] ESC shows pause (if PauseMenu added)

---

## PHASE 11: Build for Windows

### Step 11.1 - Configure Build
1. **File** → **Build Settings**
2. Click **Add Open Scenes**
3. Platform: **PC, Mac & Linux Standalone**
4. Target Platform: **Windows**
5. Architecture: **x86_64**

### Step 11.2 - Build
1. Click **Build**
2. Create folder: `C:\GitHub\4483\TheCellarsKeep\Builds\`
3. Name: `TheCellarsKeep.exe`
4. Wait for build to complete

---

## WHAT TO TELL ME NEXT

### After completing phases:

| After Phase | Send This Prompt |
|-------------|------------------|
| Phase 3 (Scripts) | `Scripts compiled with errors: [paste errors]` or `Scripts compiled successfully` |
| Phase 4 (Player) | `Player setup complete. Testing now.` |
| Phase 7 (NavMesh) | `NavMesh baked. What next?` |
| Phase 8 (Scene) | `Scene setup done. Game runs but [describe issue].` |
| Phase 10 (Test) | `Everything works! What's next?` or `Problem: [describe]` |

### For specific problems:

| Problem | Prompt |
|---------|--------|
| Player falls through ground | `Player falls through ground. Help!` |
| Enemy doesn't move | `Enemy not moving. NavMesh is baked.` |
| Scripts won't compile | `Script error: [paste full error from Console]` |
| UI not working | `UI elements not showing. Help link them.` |
| Build fails | `Build error: [paste error]` |

---

## QUICK REFERENCE

### Common Issues:

| Problem | Solution |
|---------|----------|
| "All compiler errors must be fixed" | Check Console, paste errors to me |
| Player falls through floor | Ground must have `Ground` layer, PlayerController Ground Mask must include `Ground` |
| Enemy doesn't chase | NavMesh must be baked, enemy needs NavMeshAgent |
| Cursor not locked | Play mode must be active, PlayerController handles this |
| Can't see anything | Add Directional Light to scene |
| Missing TMP | Window → TextMesh Pro → Import TMP Essentials |

### Controls:
- **WASD** - Move
- **Mouse** - Look
- **Left Shift** - Run
- **F** - Flashlight
- **ESC** - Pause

---

**Start with Phase 1 now. Tell me when you've created the project and imported TMP Essentials!**
