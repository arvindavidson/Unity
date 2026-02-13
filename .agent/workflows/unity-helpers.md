---
description: Unity stealth game project - reusable editor scripts, tools, and common workflows
---

# Unity Stealth Game — Project Workflow & Reusable Tools

## Project Info
- **Project Path**: `C:\unitytemp\My project`
- **Unity Version**: Unity 6 (HDRP 17.2.0)
- **Active Scene**: `Assets/OutdoorsScene.unity`
- **Coplay MCP**: Installed (v8.13.0) — use for all Unity Editor interaction

## Key Scripts (Runtime)

| Script | Path | Purpose |
|--------|------|---------|
| `PlayerController.cs` | `Assets/Scripts/` | Movement (WASD), stances (C=crouch, X=crawl), visual stance transitions |
| `EnemyAI.cs` | `Assets/Scripts/` | Patrol, chase, attack states with noise detection |
| `Weapon.cs` | `Assets/Scripts/` | Shooting with raycast, player uses crosshair aim point |
| `Crosshair.cs` | `Assets/Scripts/` | Mouse-following UI crosshair, ground-plane raycast for aim |
| `Health.cs` | `Assets/Scripts/` | Health + damage system for player and enemies |
| `BulletTracer.cs` | `Assets/Scripts/` | Line renderer tracer effect with fade |
| `GameManager.cs` | `Assets/Scripts/` | Win/lose conditions, UI management |
| `Treasure.cs` | `Assets/Scripts/` | Collectible objective |
| `TopDownCamera.cs` | `Assets/Scripts/` | DEPRECATED — replaced by Cinemachine |

## Reusable Editor Scripts

All located in `Assets/Scripts/Editor/`. Run via Coplay MCP:
```
mcp_coplay-mcp_execute_script(filePath="path/to/script.cs", methodName="Execute")
```

### Materials & Visuals

| Script | What It Does | When to Use |
|--------|-------------|-------------|
| `AssignLevelMaterials.cs` | Creates 7 HDRP materials (ground, wall, treasure, player body/visor, enemy body/visor) and assigns to all matching objects | After adding new objects or resetting materials |
| `SetupCrosshair.cs` | Creates crosshair UI on Canvas with procedural texture | After creating a new scene or if crosshair is missing |

### Lighting & Static

| Script | What It Does | When to Use |
|--------|-------------|-------------|
| `CheckAndFixStaticGI.cs` | Audits all Ground/Wall/Treasure objects for ContributeGI flag, fixes any missing | After adding new geometry |
| `SetStaticAndBakeLighting.cs` | Sets all env objects to static + starts async lightmap bake | First-time setup |
| `RebakeLighting.cs` | Re-triggers `Lightmapping.BakeAsync()` | After any static geometry or material changes |

### Camera

| Script | What It Does | When to Use |
|--------|-------------|-------------|
| `SetupCinemachine.cs` | Removes old TopDownCamera, adds CinemachineBrain + CinemachineFollow virtual camera | New scene setup |
| `FixCinemachineCamera.cs` | Removes RotationComposer, zooms out, sets fixed angle, tightens damping | If camera feels janky or too zoomed in |

### Enemy & Patrol

| Script | What It Does | When to Use |
|--------|-------------|-------------|
| `AssignPatrolPointsFixed.cs` | Assigns PatrolPoints2 children to the second enemy | After adding/resetting enemies |
| `LevelSetup.cs` | Full level generation (walls, ground, agents, NavMesh) | Creating a new level from scratch |
| `SceneSetup.cs` | Scene configuration and setup | Initial scene setup |

### Scene Validation

| Script | What It Does | When to Use |
|--------|-------------|-------------|
| `FullSceneValidation.cs` | Validates entire scene (components, references, layers) | Before shipping or after major changes |

## Common Workflows

### After Adding New Walls/Geometry
1. Run `AssignLevelMaterials.cs` → assigns correct material
2. Run `CheckAndFixStaticGI.cs` → ensures ContributeGI is set
3. Run `RebakeLighting.cs` → re-bake lightmaps
4. Save scene

### After Adding New Enemies
1. Create patrol points (parent GO with child transforms)
2. Run patrol point assignment script or manually set in EnemyAI
3. Run `AssignLevelMaterials.cs` → red body + orange visor
4. Ensure enemy has: NavMeshAgent, Health, EnemyAI, Weapon components
5. Set layer to "Enemy", tag to "Enemy"

### Setting Up a New Scene
1. Run `LevelSetup.cs` or manually create geometry
2. Run `SetupCinemachine.cs` → camera
3. Run `SetupCrosshair.cs` → crosshair UI
4. Run `AssignLevelMaterials.cs` → materials
5. Run `SetStaticAndBakeLighting.cs` → static + bake
6. Save scene

## Scene Hierarchy (Key Objects)
- `PlayerAgent` → PlayerBody, Visor, FirePoint
- `EnemyAgent` (×2) → EnemyBody, Visor, FirePoint
- `Ground` (×2) — green ground planes
- `Wall` (×20) — gray walls forming the level layout
- `Treasure` (×2) — gold collectible objectives
- `PatrolPoints` → 6 points (A-F) for Enemy 0
- `PatrolPoints2` → 4 points (A-D) for Enemy 1
- `CM_TopDownCamera` — Cinemachine virtual camera
- `Canvas` → HealthText, ObjectiveText, GameOverPanel, WinPanel, Crosshair

## Materials (Assets/Materials/)
- `M_Ground` — dark green-gray (0.25, 0.3, 0.22)
- `M_Wall` — warm gray (0.45, 0.42, 0.4)
- `M_Treasure` — gold, metallic (0.85, 0.7, 0.15)
- `M_PlayerBody` — deep blue (0.15, 0.3, 0.65)
- `M_PlayerVisor` — cyan emissive (0.1, 0.8, 0.9)
- `M_EnemyBody` — dark red (0.65, 0.15, 0.12)
- `M_EnemyVisor` — orange-red emissive (1.0, 0.35, 0.1)

## Packages
- `com.unity.cinemachine` 3.1.5 — smooth camera follow
- `com.unity.render-pipelines.high-definition` 17.2.0 — HDRP rendering
- `com.unity.ai.navigation` — NavMesh for enemy pathfinding

## Git
- **Repo**: `https://github.com/arvindavidson/Unity`
- **Branch**: `main`
- **Push workflow**: `git add -A` → `git commit -m "message"` → `git push`
