using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Audits all wall positions, checks for geometry issues like gaps, overlaps, 
/// and rooms not properly enclosed.
/// Also fixes ground material emission.
/// </summary>
public class AuditAndFixLevel : MonoBehaviour
{
    [MenuItem("Level/Audit Level Geometry")]
    public static void Execute()
    {
        var log = new List<string>();
        log.Add("=== LEVEL GEOMETRY AUDIT ===\n");

        // Fix ground material emission first
        FixGroundEmission(log);

        // Collect all walls
        var environment = GameObject.Find("Environment");
        if (environment == null) { Debug.LogError("No Environment found!"); return; }

        var walls = new Dictionary<string, (Vector3 pos, Vector3 size, Vector3 min, Vector3 max)>();
        
        foreach (Transform child in environment.transform)
        {
            if (!child.name.StartsWith("Wall_") && !child.name.StartsWith("Ground_"))
                continue;
            
            var pos = child.position;
            var scale = child.localScale;
            var halfScale = scale / 2f;
            var min = pos - halfScale;
            var max = pos + halfScale;
            
            walls[child.name] = (pos, scale, min, max);
        }

        // Log all walls sorted by name prefix (area)
        log.Add("--- ALL WALLS ---");
        foreach (var kvp in walls.OrderBy(k => k.Key))
        {
            var w = kvp.Value;
            log.Add($"{kvp.Key,-30} pos({w.pos.x:F1},{w.pos.y:F1},{w.pos.z:F1})  size({w.size.x:F1},{w.size.y:F1},{w.size.z:F1})  x:[{w.min.x:F1} to {w.max.x:F1}]  z:[{w.min.z:F1} to {w.max.z:F1}]");
        }

        // Check specific areas for enclosure
        log.Add("\n--- AREA CHECKS ---");
        
        // Area 1: Entry Hall should be x:-8.5 to 8.5, z:-38 to -28
        CheckArea(log, "Area 1 (Entry)", walls, -9f, 9f, -38.5f, -27.5f,
            new[] { "Wall_Entry_E", "Wall_Entry_W", "Wall_Entry_N_L", "Wall_Entry_N_R", "Wall_Perimeter_S" });

        // Area 2: Recon Room should be x:-18.5 to 18.5, z:-22 to -8
        CheckArea(log, "Area 2 (Recon)", walls, -19f, 19f, -22.5f, -7.5f,
            new[] { "Wall_Recon_S_L", "Wall_Recon_S_R", "Wall_Recon_E", "Wall_Recon_W", "Wall_Recon_N_L", "Wall_Recon_N_R" });

        // Area 3: Central Hub should be x:-18.5 to 18.5, z:-2 to 14
        CheckArea(log, "Area 3 (Hub)", walls, -19f, 19f, -2.5f, 14.5f,
            new[] { "Wall_Hub_S_L", "Wall_Hub_S_R", "Wall_Hub_W_Lower", "Wall_Hub_W_Upper", "Wall_Hub_E_Lower", "Wall_Hub_E_Upper", "Wall_Hub_N_L", "Wall_Hub_N_R" });

        // Check corridor 1 walls connect Entry to Recon
        log.Add("\n--- CORRIDOR CHECKS ---");
        CheckCorridorConnection(log, walls, "Corridor 1 (Entry→Recon)",
            "Wall_Entry_N_L", "Wall_Entry_N_R",   // gap in entry north wall
            "Wall_Recon_S_L", "Wall_Recon_S_R",   // gap in recon south wall
            "Wall_Corr1_E", "Wall_Corr1_W");       // corridor side walls

        CheckCorridorConnection(log, walls, "Corridor 2 (Recon→Hub)",
            "Wall_Recon_N_L", "Wall_Recon_N_R",
            "Wall_Hub_S_L", "Wall_Hub_S_R",
            "Wall_Corr2_E", "Wall_Corr2_W");

        CheckCorridorConnection(log, walls, "Corridor 3 (Hub→Command)",
            "Wall_Hub_N_L", "Wall_Hub_N_R",
            "Wall_Cmd_S_L", "Wall_Cmd_S_R",
            "Wall_Corr3_E", "Wall_Corr3_W");

        // Check for gaps between corridor walls and room walls
        log.Add("\n--- GAP ANALYSIS ---");
        
        // Entry north wall gap
        if (walls.ContainsKey("Wall_Entry_N_L") && walls.ContainsKey("Wall_Entry_N_R"))
        {
            var left = walls["Wall_Entry_N_L"];
            var right = walls["Wall_Entry_N_R"];
            float gapStart = left.max.x;
            float gapEnd = right.min.x;
            log.Add($"Entry north doorway: gap from x={gapStart:F1} to x={gapEnd:F1} (width: {gapEnd - gapStart:F1}m)");
        }

        // Corridor 1 walls vs room walls
        if (walls.ContainsKey("Wall_Corr1_E") && walls.ContainsKey("Wall_Entry_N_R"))
        {
            var corrE = walls["Wall_Corr1_E"];
            var entryNR = walls["Wall_Entry_N_R"];
            // Corridor east wall should connect from entry north wall z to recon south wall z
            log.Add($"Corr1 East wall: z=[{corrE.min.z:F1} to {corrE.max.z:F1}]");
            log.Add($"  Should bridge Entry_N (z={entryNR.pos.z:F1}) to Recon_S (z={walls["Wall_Recon_S_L"].pos.z:F1})");
            float entryNZ = entryNR.pos.z;
            float reconSZ = walls["Wall_Recon_S_L"].pos.z;
            log.Add($"  Gap from z={entryNZ:F1} to z={reconSZ:F1} = {reconSZ - entryNZ:F1}m");

            // Check if corridor walls actually span from entry to recon
            if (corrE.min.z > entryNZ + 0.5f || corrE.max.z < reconSZ - 0.5f)
            {
                log.Add("  *** PROBLEM: Corridor wall doesn't fully bridge the rooms! ***");
            }
        }

        // Check hub west wall gap alignment with West Wing
        if (walls.ContainsKey("Wall_Hub_W_Lower") && walls.ContainsKey("Wall_Hub_W_Upper"))
        {
            var lower = walls["Wall_Hub_W_Lower"];
            var upper = walls["Wall_Hub_W_Upper"];
            float gapStart = lower.max.z;
            float gapEnd = upper.min.z;
            log.Add($"Hub west doorway: gap from z={gapStart:F1} to z={gapEnd:F1} (width: {gapEnd - gapStart:F1}m)");
        }

        // Check room wall connections to perimeter
        log.Add("\n--- PERIMETER CONNECTION CHECKS ---");
        // Entry east/west walls should reach perimeter south
        if (walls.ContainsKey("Wall_Entry_E") && walls.ContainsKey("Wall_Perimeter_S"))
        {
            var entryE = walls["Wall_Entry_E"];
            var perimS = walls["Wall_Perimeter_S"];
            float gap = entryE.min.z - perimS.max.z;
            log.Add($"Entry_E bottom z={entryE.min.z:F1} to Perimeter_S top z={perimS.max.z:F1}, gap={gap:F1}m");
            if (gap > 0.5f) log.Add("  *** PROBLEM: Gap between Entry wall and south perimeter! ***");
        }

        // West Wing west wall to perimeter west
        if (walls.ContainsKey("Wall_WestWing_W") && walls.ContainsKey("Wall_Perimeter_W"))
        {
            var wwW = walls["Wall_WestWing_W"];
            var perimW = walls["Wall_Perimeter_W"];
            float gap = wwW.min.x - perimW.max.x;
            log.Add($"WestWing_W left x={wwW.min.x:F1} vs Perimeter_W right x={perimW.max.x:F1}, gap={gap:F1}m");
        }

        log.Add("\n=== AUDIT COMPLETE ===");
        Debug.Log(string.Join("\n", log));
    }

    static void FixGroundEmission(List<string> log)
    {
        var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        int fixed_ = 0;
        foreach (var r in renderers)
        {
            if (r.gameObject.name.StartsWith("Ground"))
            {
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat != null)
                    {
                        // Disable emission
                        mat.DisableKeyword("_EMISSION");
                        if (mat.HasProperty("_EmissiveColor"))
                            mat.SetColor("_EmissiveColor", Color.black);
                        if (mat.HasProperty("_EmissionColor"))
                            mat.SetColor("_EmissionColor", Color.black);
                        // Set base color to dark gray
                        if (mat.HasProperty("_BaseColor"))
                            mat.SetColor("_BaseColor", new Color(0.3f, 0.35f, 0.3f, 1f));
                        if (mat.HasProperty("_Color"))
                            mat.SetColor("_Color", new Color(0.3f, 0.35f, 0.3f, 1f));
                        // Reduce smoothness 
                        if (mat.HasProperty("_Smoothness"))
                            mat.SetFloat("_Smoothness", 0.2f);
                        EditorUtility.SetDirty(mat);
                        fixed_++;
                    }
                }
            }
            // Also tone down wall materials
            if (r.gameObject.name.StartsWith("Wall_"))
            {
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat != null)
                    {
                        mat.DisableKeyword("_EMISSION");
                        if (mat.HasProperty("_EmissiveColor"))
                            mat.SetColor("_EmissiveColor", Color.black);
                        if (mat.HasProperty("_EmissionColor"))
                            mat.SetColor("_EmissionColor", Color.black);
                        if (mat.HasProperty("_Smoothness"))
                            mat.SetFloat("_Smoothness", 0.3f);
                        EditorUtility.SetDirty(mat);
                    }
                }
            }
        }
        log.Add($"Fixed emission on {fixed_} ground materials");
    }

    static void CheckArea(List<string> log, string areaName, 
        Dictionary<string, (Vector3 pos, Vector3 size, Vector3 min, Vector3 max)> walls,
        float expectedMinX, float expectedMaxX, float expectedMinZ, float expectedMaxZ,
        string[] wallNames)
    {
        log.Add($"\n{areaName}: expected bounds x:[{expectedMinX:F1},{expectedMaxX:F1}] z:[{expectedMinZ:F1},{expectedMaxZ:F1}]");
        foreach (var name in wallNames)
        {
            if (walls.ContainsKey(name))
            {
                var w = walls[name];
                log.Add($"  {name,-30} x:[{w.min.x:F1},{w.max.x:F1}] z:[{w.min.z:F1},{w.max.z:F1}]");
            }
            else
            {
                log.Add($"  {name,-30} *** MISSING! ***");
            }
        }
    }

    static void CheckCorridorConnection(List<string> log,
        Dictionary<string, (Vector3 pos, Vector3 size, Vector3 min, Vector3 max)> walls,
        string name, string fromL, string fromR, string toL, string toR, string corrE, string corrW)
    {
        log.Add($"\n{name}:");
        if (walls.ContainsKey(corrE) && walls.ContainsKey(corrW))
        {
            var e = walls[corrE];
            var w = walls[corrW];
            float width = e.pos.x - w.pos.x;
            float fromZ = Mathf.Min(e.min.z, w.min.z);
            float toZ = Mathf.Max(e.max.z, w.max.z);
            log.Add($"  Corridor width: {width:F1}m, z-span: [{fromZ:F1} to {toZ:F1}] (length: {toZ - fromZ:F1}m)");
            
            // Check if corridor walls connect to both room walls
            if (walls.ContainsKey(fromL))
            {
                float roomWallZ = walls[fromL].pos.z;
                if (Mathf.Abs(fromZ - roomWallZ) > 1f)
                    log.Add($"  *** GAP: Corridor bottom z={fromZ:F1} doesn't meet room wall at z={roomWallZ:F1}! ***");
            }
            if (walls.ContainsKey(toL))
            {
                float roomWallZ = walls[toL].pos.z;
                if (Mathf.Abs(toZ - roomWallZ) > 1f)
                    log.Add($"  *** GAP: Corridor top z={toZ:F1} doesn't meet room wall at z={roomWallZ:F1}! ***");
            }
        }
    }
}
