using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Redesigned compound level applying level design principles.
/// v2 — Fixed geometry: wall connections, doorway widths, perimeter enclosure.
///
/// METRICS STANDARD:
///   Player:    1.0 × 2.0m (radius 0.5m)
///   Walk:      5.0 m/s   Sprint: 8.0 m/s
///   Wall:      4.0m tall,  1.0m thick
///   Hallway:   4.0m wide
///   Doorway:   3.0m wide
///   Cover:     1.2m tall,  2.0m wide
///   Detection: 10.0m (enemy view)
/// </summary>
public class BuildCompoundLevel : MonoBehaviour
{
    // ═══════════════════════════════════════
    //  METRICS CONSTANTS
    // ═══════════════════════════════════════
    const float WH = 4f;    // wall height
    const float WT = 1f;    // wall thickness
    const float WY = 2f;    // wall center Y (WH/2)
    const float DOOR = 3f;  // doorway width
    const float COVER_H = 1.2f;
    const float COVER_W = 2f;
    const float COVER_D = 1f;

    [MenuItem("Level/Build Compound Level (Redesigned)")]
    public static void Execute()
    {
        var log = new List<string>();
        log.Add("=== Building Redesigned Compound Level v2 ===");

        // Groups
        var env    = GetOrCreate("Environment");
        var agents = GetOrCreate("Agents");
        var ai     = GetOrCreate("AI");
        var light  = GetOrCreate("Lighting");
        GetOrCreate("Cameras");
        GetOrCreate("UI");
        var sys    = GetOrCreate("Systems");

        // Clear
        Clear(env.transform, log);
        Clear(agents.transform, log);
        Clear(ai.transform, log);

        // ═══════════════════ GROUND ═══════════════════
        // Level: 100 × 80  (x: -50..50, z: -40..40)
        Wall("Ground_01", -25, -0.5f, 0, 50, 1, 80, env);
        Wall("Ground_02", 25, -0.5f, 0, 50, 1, 80, env);

        // ═══════════════════ PERIMETER ═══════════════════
        Wall("Wall_Perimeter_N", 0, WY, 40, 102, WH, WT, env);
        Wall("Wall_Perimeter_S", 0, WY, -40, 102, WH, WT, env);
        Wall("Wall_Perimeter_E", 50, WY, 0, WT, WH, 80, env);
        Wall("Wall_Perimeter_W", -50, WY, 0, WT, WH, 80, env);

        // ═══════════════════════════════════════════════════
        //  AREA 1: ENTRY HALL  x:[-10,10]  z:[-40,-26]
        //  Enclosed by perimeter south + side walls + north wall w/ door
        //  Entry sides run from perimeter (z=-40) to north wall (z=-26)
        // ═══════════════════════════════════════════════════
        float entryW = 10f; // half-width of entry
        float entryN = -26f; // north edge of entry
        float entryLen = 14f; // z: -40 to -26

        Wall("Wall_Entry_E", entryW + 0.5f, WY, -33f, WT, WH, entryLen, env);
        Wall("Wall_Entry_W", -(entryW + 0.5f), WY, -33f, WT, WH, entryLen, env);

        // North wall with 3m doorway centered at x=0
        float entryNseg = (2 * entryW - DOOR) / 2f; // each side segment
        Wall("Wall_Entry_N_L", -(DOOR / 2 + entryNseg / 2), WY, entryN, entryNseg, WH, WT, env);
        Wall("Wall_Entry_N_R", (DOOR / 2 + entryNseg / 2), WY, entryN, entryNseg, WH, WT, env);
        log.Add("Area 1: Entry Hall (z:-40 to -26, safe)");

        // ═══════════════════════════════════════════════════
        //  REST CORRIDOR 1: x:[-1.5,1.5]  z:[-26,-22]
        //  Connects Entry north (z=-26) to Recon south (z=-22)
        //  Fill gaps between Entry walls and Recon walls
        // ═══════════════════════════════════════════════════
        float corrHW = DOOR / 2f; // corridor half-width = 1.5
        float corr1Top = -22f;
        float corr1Bot = entryN;
        float corr1Len = corr1Top - corr1Bot; // 4m
        float corr1Mid = (corr1Top + corr1Bot) / 2f;

        Wall("Wall_Corr1_E", corrHW + 0.5f, WY, corr1Mid, WT, WH, corr1Len, env);
        Wall("Wall_Corr1_W", -(corrHW + 0.5f), WY, corr1Mid, WT, WH, corr1Len, env);

        // Fill walls between entry sides and corridor (blocking off the open space)
        // Left fill: x from -10.5 to -2 at z=-26
        float fillLeftW = entryW + 0.5f - (corrHW + 0.5f);
        Wall("Wall_Fill_Corr1_L", -(corrHW + 0.5f + fillLeftW / 2), WY, corr1Mid, fillLeftW, WH, corr1Len, env);
        Wall("Wall_Fill_Corr1_R", (corrHW + 0.5f + fillLeftW / 2), WY, corr1Mid, fillLeftW, WH, corr1Len, env);
        log.Add("Corridor 1: Entry→Recon (3m wide, 4m long)");

        // ═══════════════════════════════════════════════════
        //  AREA 2: RECON ROOM  x:[-18,18]  z:[-22,-8]
        //  First encounter: 1 enemy, cover blocks
        // ═══════════════════════════════════════════════════
        float reconW = 18f; // half-width
        float reconS = -22f;
        float reconN = -8f;
        float reconH = reconN - reconS; // 14m
        float reconMidZ = (reconN + reconS) / 2f;

        // South wall: two segments with 3m door centered
        float reconSseg = (2 * reconW - DOOR) / 2f;
        Wall("Wall_Recon_S_L", -(DOOR / 2 + reconSseg / 2), WY, reconS, reconSseg, WH, WT, env);
        Wall("Wall_Recon_S_R", (DOOR / 2 + reconSseg / 2), WY, reconS, reconSseg, WH, WT, env);

        // Side walls
        Wall("Wall_Recon_E", reconW + 0.5f, WY, reconMidZ, WT, WH, reconH, env);
        Wall("Wall_Recon_W", -(reconW + 0.5f), WY, reconMidZ, WT, WH, reconH, env);

        // North wall: two segments with 3m door centered
        float reconNseg = (2 * reconW - DOOR) / 2f;
        Wall("Wall_Recon_N_L", -(DOOR / 2 + reconNseg / 2), WY, reconN, reconNseg, WH, WT, env);
        Wall("Wall_Recon_N_R", (DOOR / 2 + reconNseg / 2), WY, reconN, reconNseg, WH, WT, env);

        // Cover
        Cover("Wall_Cover_Recon_1", -8, -16, env);
        Cover("Wall_Cover_Recon_2", 6, -12, env);
        Cover("Wall_Cover_Recon_3", -4, -10, env);
        log.Add("Area 2: Recon Room (36×14m, 1 enemy, 3 cover)");

        // ═══════════════════════════════════════════════════
        //  REST CORRIDOR 2: x:[-1.5,1.5]  z:[-8,-4]
        //  Connects Recon N (z=-8) to Hub S (z=-4)
        // ═══════════════════════════════════════════════════
        float corr2Bot = reconN;
        float corr2Top = -4f;
        float corr2Len = corr2Top - corr2Bot; // 4m
        float corr2Mid = (corr2Top + corr2Bot) / 2f;

        Wall("Wall_Corr2_E", corrHW + 0.5f, WY, corr2Mid, WT, WH, corr2Len, env);
        Wall("Wall_Corr2_W", -(corrHW + 0.5f), WY, corr2Mid, WT, WH, corr2Len, env);

        // Fill walls between recon sides and corridor
        float fill2W = reconW + 0.5f - (corrHW + 0.5f);
        Wall("Wall_Fill_Corr2_L", -(corrHW + 0.5f + fill2W / 2), WY, corr2Mid, fill2W, WH, corr2Len, env);
        Wall("Wall_Fill_Corr2_R", (corrHW + 0.5f + fill2W / 2), WY, corr2Mid, fill2W, WH, corr2Len, env);
        log.Add("Corridor 2: Recon→Hub (3m wide, 4m long)");

        // ═══════════════════════════════════════════════════
        //  AREA 3: CENTRAL HUB  x:[-18,18]  z:[-4,14]
        //  Decision point: 1 enemy, exits W/E/N
        //  West/East doorways at z=3..6 (3m gap)
        //  North doorway centered at x=0 (3m gap)
        // ═══════════════════════════════════════════════════
        float hubW = 18f; // same width as recon for clean alignment
        float hubS = -4f;
        float hubN = 14f;
        float hubH = hubN - hubS; // 18m
        float hubMidZ = (hubN + hubS) / 2f;

        // South wall with 3m door
        float hubSseg = (2 * hubW - DOOR) / 2f;
        Wall("Wall_Hub_S_L", -(DOOR / 2 + hubSseg / 2), WY, hubS, hubSseg, WH, WT, env);
        Wall("Wall_Hub_S_R", (DOOR / 2 + hubSseg / 2), WY, hubS, hubSseg, WH, WT, env);

        // West wall with 3m doorway at z=3.5 to 6.5 (centered z=5)
        float wingDoorCenter = 5f;
        float wingDoorHalf = DOOR / 2f;
        float hubWlowerH = (wingDoorCenter - wingDoorHalf) - hubS; // 3.5 - (-4) = 7.5
        float hubWupperH = hubN - (wingDoorCenter + wingDoorHalf); // 14 - 6.5 = 7.5
        Wall("Wall_Hub_W_Lower", -(hubW + 0.5f), WY, hubS + hubWlowerH / 2, WT, WH, hubWlowerH, env);
        Wall("Wall_Hub_W_Upper", -(hubW + 0.5f), WY, hubN - hubWupperH / 2, WT, WH, hubWupperH, env);

        // East wall with 3m doorway at same position
        Wall("Wall_Hub_E_Lower", hubW + 0.5f, WY, hubS + hubWlowerH / 2, WT, WH, hubWlowerH, env);
        Wall("Wall_Hub_E_Upper", hubW + 0.5f, WY, hubN - hubWupperH / 2, WT, WH, hubWupperH, env);

        // North wall with 3m door
        float hubNseg = (2 * hubW - DOOR) / 2f;
        Wall("Wall_Hub_N_L", -(DOOR / 2 + hubNseg / 2), WY, hubN, hubNseg, WH, WT, env);
        Wall("Wall_Hub_N_R", (DOOR / 2 + hubNseg / 2), WY, hubN, hubNseg, WH, WT, env);

        // Cover
        Cover("Wall_Cover_Hub_1", -10, 1, env);
        Cover("Wall_Cover_Hub_2", 10, 8, env);
        Cover("Wall_Cover_Hub_3", 0, 10, env);
        Cover("Wall_Cover_Hub_4", -6, -2, env);
        log.Add("Area 3: Central Hub (36×18m, 1 enemy, 3 exits)");

        // ═══════════════════════════════════════════════════
        //  AREA 4: WEST WING  x:[-49,-19]  z:[-4,14]
        //  Shares Hub's south and north z-coords
        //  Connects to Hub via west wall doorway
        //  1 enemy + Treasure 1
        // ═══════════════════════════════════════════════════
        float wwLeft = -49f;
        float wwRight = -(hubW + 0.5f); // -18.5 (hub's west wall is the east border)
        float wwWidth = wwRight - wwLeft; // 30.5m
        float wwMidX = (wwLeft + wwRight) / 2f;

        // South wall
        Wall("Wall_WestWing_S", wwMidX, WY, hubS, wwWidth, WH, WT, env);
        // North wall
        Wall("Wall_WestWing_N", wwMidX, WY, hubN, wwWidth, WH, WT, env);
        // West end wall (against perimeter)
        Wall("Wall_WestWing_W", wwLeft - 0.5f, WY, hubMidZ, WT, WH, hubH, env);

        Cover("Wall_Cover_West_1", -30, 2, env);
        Cover("Wall_Cover_West_2", -40, 8, env);
        Cover("Wall_Cover_West_3", -34, 12, env);
        log.Add("Area 4: West Wing (dead end, T1, 1 enemy)");

        // ═══════════════════════════════════════════════════
        //  AREA 5: EAST WING  x:[19,49]  z:[-4,14]
        //  Shares Hub's south and north z-coords
        //  Connects to Hub via east wall doorway
        //  East wall is the perimeter (no separate east wall)
        //  2 enemies + Treasure 2
        // ═══════════════════════════════════════════════════
        float ewLeft = hubW + 0.5f; // 18.5
        float ewRight = 49f; // extend to near perimeter
        float ewWidth = ewRight - ewLeft; // 30.5
        float ewMidX = (ewLeft + ewRight) / 2f;

        Wall("Wall_EastWing_S", ewMidX, WY, hubS, ewWidth, WH, WT, env);
        Wall("Wall_EastWing_N", ewMidX, WY, hubN, ewWidth, WH, WT, env);
        // No separate east wall — perimeter acts as east boundary

        Cover("Wall_Cover_East_1", 28, 2, env);
        Cover("Wall_Cover_East_2", 38, 10, env);
        Cover("Wall_Cover_East_3", 32, 8, env);
        log.Add("Area 5: East Wing (dead end, T2, 2 enemies)");

        // ═══════════════════════════════════════════════════
        //  REST CORRIDOR 3: x:[-1.5,1.5]  z:[14,18]
        //  Connects Hub N (z=14) to Command S (z=18)
        // ═══════════════════════════════════════════════════
        float corr3Bot = hubN;
        float corr3Top = 18f;
        float corr3Len = corr3Top - corr3Bot; // 4m
        float corr3Mid = (corr3Top + corr3Bot) / 2f;

        Wall("Wall_Corr3_E", corrHW + 0.5f, WY, corr3Mid, WT, WH, corr3Len, env);
        Wall("Wall_Corr3_W", -(corrHW + 0.5f), WY, corr3Mid, WT, WH, corr3Len, env);

        float fill3W = hubW + 0.5f - (corrHW + 0.5f);
        Wall("Wall_Fill_Corr3_L", -(corrHW + 0.5f + fill3W / 2), WY, corr3Mid, fill3W, WH, corr3Len, env);
        Wall("Wall_Fill_Corr3_R", (corrHW + 0.5f + fill3W / 2), WY, corr3Mid, fill3W, WH, corr3Len, env);
        log.Add("Corridor 3: Hub→Command (3m wide, 4m long)");

        // ═══════════════════════════════════════════════════
        //  AREA 6: COMMAND CENTER  x:[-18,49]  z:[18,34]
        //  Climax: 2 enemies, Treasure 3
        //  Wider room that spans full east side
        //  North-east corner has doorway into escape passage
        // ═══════════════════════════════════════════════════
        float cmdS = 18f;
        float cmdN = 34f;
        float cmdH = cmdN - cmdS; // 16m
        float cmdMidZ = (cmdN + cmdS) / 2f;

        // South wall: two segments with 3m door centered at x=0
        // Left segment: from hub west wall to door
        float cmdSseg = (2 * hubW - DOOR) / 2f;
        Wall("Wall_Cmd_S_L", -(DOOR / 2 + cmdSseg / 2), WY, cmdS, cmdSseg, WH, WT, env);
        // Right segment: from door to hub east wall (just the hub-width portion)
        Wall("Wall_Cmd_S_R", (DOOR / 2 + cmdSseg / 2), WY, cmdS, cmdSseg, WH, WT, env);
        // Extended south wall from hub east to perimeter (over east wing north wall)
        float cmdSextW = 49f - (hubW + 0.5f); // 49 - 18.5 = 30.5
        float cmdSextX = (hubW + 0.5f) + cmdSextW / 2f;
        Wall("Wall_Cmd_S_Ext", cmdSextX, WY, cmdS, cmdSextW, WH, WT, env);

        // West wall (solid, same as hub's west wall x)
        Wall("Wall_Cmd_W", -(hubW + 0.5f), WY, cmdMidZ, WT, WH, cmdH, env);

        // East wall — use perimeter, no separate east wall needed

        // North wall — spans from west to perimeter
        float cmdNfullW = 49f - (-(hubW + 0.5f)); // 49 + 18.5 = 67.5
        float cmdNmidX = (-(hubW + 0.5f) + 49f) / 2f;
        // North wall with escape doorway near east end
        // Escape door at x=45..48 (3m gap)
        float escDoorX = 46.5f;
        float cmdNleftW = (escDoorX - DOOR / 2f) - (-(hubW + 0.5f)); // 45 - (-18.5) = 63.5
        float cmdNrightW = 49f - (escDoorX + DOOR / 2f); // 49 - 48 = 1
        Wall("Wall_Cmd_N_L", (-(hubW + 0.5f) + (escDoorX - DOOR / 2f)) / 2f, WY, cmdN, cmdNleftW, WH, WT, env);
        if (cmdNrightW > 0.1f)
            Wall("Wall_Cmd_N_R", (escDoorX + DOOR / 2f + 49f) / 2f, WY, cmdN, cmdNrightW, WH, WT, env);

        Cover("Wall_Cover_Cmd_1", -10, 22, env);
        Cover("Wall_Cover_Cmd_2", 8, 28, env);
        Cover("Wall_Cover_Cmd_3", -4, 30, env);
        Cover("Wall_Cover_Cmd_4", 30, 24, env);
        log.Add("Area 6: Command Center (67×16m, 2 enemies, T3)");

        // ═══════════════════════════════════════════════════
        //  AREA 7: ESCAPE PASSAGE
        //  Runs along the east perimeter from Command north door
        //  (z=34) south to near the Entry (z=-34)
        //  x:[45,49]  — 4m wide corridor along east edge
        //  Enclosed by: perimeter east (x=50), own west wall (x=45),
        //  Command north wall gap as entry, bottom cap as exit
        // ═══════════════════════════════════════════════════
        float escWx = 45f;  // west wall of escape
        float escEx = 49f;  // east side is perimeter
        float escMidX = (escWx + escEx) / 2f;
        float escTop = cmdN; // 34 (entry from command)
        float escBot = -34f; // exit near entry

        // West wall: from command north (z=34) down to east wing north (z=14)
        float escW_upper_len = escTop - hubN; // 34 - 14 = 20
        Wall("Wall_Escape_W_Upper", escWx, WY, (escTop + hubN) / 2f, WT, WH, escW_upper_len, env);

        // West wall: through east wing zone (z=14 to z=-4) — this creates the
        // east wing's east boundary AND escape west wall in one piece
        float escW_mid_len = hubN - hubS; // 14 - (-4) = 18
        Wall("Wall_Escape_W_Mid", escWx, WY, hubMidZ, WT, WH, escW_mid_len, env);

        // West wall: from east wing south (z=-4) down to escape bottom (z=-34)
        float escW_lower_len = hubS - escBot; // -4 - (-34) = 30
        Wall("Wall_Escape_W_Lower", escWx, WY, (hubS + escBot) / 2f, WT, WH, escW_lower_len, env);

        // Bottom cap (exit)
        Wall("Wall_Escape_Bot", escMidX, WY, escBot, escEx - escWx, WH, WT, env);
        // Top cap — none needed, entry is the gap in command north wall

        // No east wall — perimeter acts as east boundary

        log.Add("Area 7: Escape Passage (4m wide, along east perimeter, one-way)");

        // ═══════════════════════════════════════════════════
        //  PLAYER
        // ═══════════════════════════════════════════════════
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerAgent.prefab");
        if (playerPrefab != null)
        {
            var p = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            p.name = "PlayerAgent";
            p.transform.position = new Vector3(0, 0, -36);
            p.transform.parent = agents.transform;
            p.tag = "Player";
            log.Add("Player at Entry (0,0,-36)");
        }

        // ═══════════════════════════════════════════════════
        //  ENEMIES (7 total)
        // ═══════════════════════════════════════════════════
        var ePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyAgent.prefab");
        if (ePrefab != null)
        {
            // Area 2: Recon — 1 enemy
            PlaceEnemy(ePrefab, "EnemyAgent_01", V(-10, 0, -18), agents, ai,
                "PatrolPoints_01", new[] {
                    V(-14, 0, -20), V(14, 0, -20), V(14, 0, -10), V(-14, 0, -10)
                }, log, "Recon");

            // Area 3: Hub — 1 enemy
            PlaceEnemy(ePrefab, "EnemyAgent_02", V(-8, 0, 0), agents, ai,
                "PatrolPoints_02", new[] {
                    V(-14, 0, -2), V(14, 0, -2), V(14, 0, 12), V(-14, 0, 12)
                }, log, "Hub");

            // Area 4: West Wing — 1 enemy (guards T1)
            PlaceEnemy(ePrefab, "EnemyAgent_03", V(-36, 0, 5), agents, ai,
                "PatrolPoints_03", new[] {
                    V(-44, 0, -2), V(-24, 0, -2), V(-24, 0, 12), V(-44, 0, 12)
                }, log, "West Wing");

            // Area 5: East Wing — 2 enemies
            PlaceEnemy(ePrefab, "EnemyAgent_04", V(26, 0, 0), agents, ai,
                "PatrolPoints_04", new[] {
                    V(22, 0, -2), V(36, 0, -2), V(36, 0, 12), V(22, 0, 12)
                }, log, "East Wing A");

            PlaceEnemy(ePrefab, "EnemyAgent_05", V(40, 0, 10), agents, ai,
                "PatrolPoints_05", new[] {
                    V(34, 0, 2), V(44, 0, 2), V(44, 0, 12), V(34, 0, 12)
                }, log, "East Wing B");

            // Area 6: Command — 2 enemies
            PlaceEnemy(ePrefab, "EnemyAgent_06", V(-8, 0, 22), agents, ai,
                "PatrolPoints_06", new[] {
                    V(-14, 0, 20), V(0, 0, 20), V(0, 0, 32), V(-14, 0, 32)
                }, log, "Command A");

            PlaceEnemy(ePrefab, "EnemyAgent_07", V(10, 0, 28), agents, ai,
                "PatrolPoints_07", new[] {
                    V(4, 0, 22), V(14, 0, 22), V(14, 0, 32), V(4, 0, 32)
                }, log, "Command B");
        }

        // ═══════════════════════════════════════════════════
        //  TREASURES (3)
        // ═══════════════════════════════════════════════════
        var tPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Treasure.prefab");
        if (tPrefab != null)
        {
            PlaceTreasure(tPrefab, "Treasure_01", V(-46, 0.5f, 5), env, log, "West Wing");
            PlaceTreasure(tPrefab, "Treasure_02", V(44, 0.5f, 5), env, log, "East Wing");
            PlaceTreasure(tPrefab, "Treasure_03", V(0, 0.5f, 32), env, log, "Command");
        }

        // ═══════════════════════════════════════════════════
        //  SYSTEMS
        // ═══════════════════════════════════════════════════
        if (GameObject.Find("GameManager") == null)
        {
            var gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            gm.transform.parent = sys.transform;
        }
        if (GameObject.Find("EventSystem") == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            es.transform.parent = sys.transform;
        }

        // ═══════════════════════════════════════════════════
        //  LIGHTING
        // ═══════════════════════════════════════════════════
        if (GameObject.Find("Sun") == null)
        {
            var sun = new GameObject("Sun");
            var l = sun.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1f, 0.95f, 0.84f);
            l.intensity = 2f;
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
            sun.transform.parent = light.transform;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        log.Add("\n=== DONE: 7 areas, 7 enemies, 3 treasures ===");
        Debug.Log(string.Join("\n", log));
    }

    // ─── HELPERS ──────────────────────────────────────────

    static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);

    static GameObject GetOrCreate(string n)
    {
        var go = GameObject.Find(n);
        if (go == null) { go = new GameObject(n); Undo.RegisterCreatedObjectUndo(go, n); }
        return go;
    }

    static void Clear(Transform p, List<string> log)
    {
        int c = p.childCount;
        for (int i = c - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(p.GetChild(i).gameObject);
        log.Add($"  Cleared {c} from {p.name}");
    }

    static GameObject Wall(string name, float x, float y, float z, float sx, float sy, float sz, GameObject parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = new Vector3(x, y, z);
        go.transform.localScale = new Vector3(sx, sy, sz);
        go.transform.parent = parent.transform;
        Undo.RegisterCreatedObjectUndo(go, name);
        return go;
    }

    static void Cover(string name, float x, float z, GameObject parent)
    {
        Wall(name, x, COVER_H / 2, z, COVER_W, COVER_H, COVER_D, parent);
    }

    static void PlaceEnemy(GameObject prefab, string name, Vector3 pos,
        GameObject agentParent, GameObject aiParent, string ppName,
        Vector3[] pts, List<string> log, string area)
    {
        var e = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        e.name = name; e.transform.position = pos;
        e.transform.parent = agentParent.transform; e.tag = "Enemy";

        var pg = new GameObject(ppName);
        pg.transform.parent = aiParent.transform;
        Undo.RegisterCreatedObjectUndo(pg, ppName);

        var ptList = new List<Transform>();
        for (int i = 0; i < pts.Length; i++)
        {
            var pt = new GameObject($"PatrolPoint_{(char)('A' + i)}");
            pt.transform.position = pts[i];
            pt.transform.parent = pg.transform;
            Undo.RegisterCreatedObjectUndo(pt, "pp");
            ptList.Add(pt.transform);
        }

        var ai = e.GetComponent<EnemyAI>();
        if (ai != null)
        {
            var so = new SerializedObject(ai);
            var pp = so.FindProperty("patrolPoints");
            pp.arraySize = ptList.Count;
            for (int i = 0; i < ptList.Count; i++)
                pp.GetArrayElementAtIndex(i).objectReferenceValue = ptList[i];
            so.ApplyModifiedProperties();
        }
        log.Add($"  {name} in {area}");
    }

    static void PlaceTreasure(GameObject prefab, string name, Vector3 pos,
        GameObject parent, List<string> log, string area)
    {
        var t = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        t.name = name; t.transform.position = pos;
        t.transform.parent = parent.transform;
        log.Add($"  {name} in {area}");
    }
}
