using UnityEngine;
using UnityEditor;
using UnityEditor.AI;

public class LevelSetup : EditorWindow
{
    [MenuItem("Tools/Setup Level")]
    public static void Setup()
    {
        // Clear existing level objects
        GameObject[] existingWalls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in existingWalls) DestroyImmediate(wall);
        
        GameObject ground = GameObject.Find("Ground");
        if (ground != null) DestroyImmediate(ground);

        GameObject patrolPoints = GameObject.Find("PatrolPoints");
        if (patrolPoints != null) DestroyImmediate(patrolPoints);

        // Setup Tags and Layers
        SetupTagsAndLayers();

        // Create Level
        CreateMaze();
        
        // Re-assign scripts and setup navigation
        AssignScripts.Assign();
    }

    static void SetupTagsAndLayers()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        string[] tags = { "Enemy", "Treasure", "Wall" };
        foreach (string tag in tags)
        {
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(tag)) { found = true; break; }
            }
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
                n.stringValue = tag;
            }
        }

        SerializedProperty layersProp = tagManager.FindProperty("layers");
        string[] layers = { "Player", "Enemy", "Wall" };
        foreach (string layer in layers)
        {
            bool found = false;
            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty l = layersProp.GetArrayElementAtIndex(i);
                if (l.stringValue.Equals(layer)) { found = true; break; }
            }
            if (!found)
            {
                for (int i = 8; i < layersProp.arraySize; i++)
                {
                    SerializedProperty l = layersProp.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(l.stringValue))
                    {
                        l.stringValue = layer;
                        break;
                    }
                }
            }
        }
        tagManager.ApplyModifiedProperties();
    }

    static void CreateMaze()
    {
        // Ground 60x60
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(6, 1, 6);
        ground.transform.position = Vector3.zero;
        GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.NavigationStatic);

        // Outer Walls
        float size = 30f; // Half size (60/2)
        float height = 4f;
        CreateWall(new Vector3(0, height/2, size), new Vector3(size*2, height, 1)); // Top
        CreateWall(new Vector3(0, height/2, -size), new Vector3(size*2, height, 1)); // Bottom
        CreateWall(new Vector3(-size, height/2, 0), new Vector3(1, height, size*2)); // Left
        CreateWall(new Vector3(size, height/2, 0), new Vector3(1, height, size*2)); // Right

        // Maze Walls
        // Quadrant 1 (Top Left)
        CreateWall(new Vector3(-15, height/2, 15), new Vector3(1, height, 10));
        CreateWall(new Vector3(-20, height/2, 5), new Vector3(10, height, 1));
        
        // Quadrant 2 (Top Right)
        CreateWall(new Vector3(15, height/2, 15), new Vector3(1, height, 10));
        CreateWall(new Vector3(20, height/2, 5), new Vector3(10, height, 1));

        // Quadrant 3 (Bottom Left)
        CreateWall(new Vector3(-15, height/2, -15), new Vector3(1, height, 10));
        CreateWall(new Vector3(-20, height/2, -5), new Vector3(10, height, 1));

        // Quadrant 4 (Bottom Right)
        CreateWall(new Vector3(15, height/2, -15), new Vector3(1, height, 10));
        CreateWall(new Vector3(20, height/2, -5), new Vector3(10, height, 1));

        // Center Area Protection
        CreateWall(new Vector3(-5, height/2, 5), new Vector3(1, height, 5));
        CreateWall(new Vector3(5, height/2, 5), new Vector3(1, height, 5));
        CreateWall(new Vector3(-5, height/2, -5), new Vector3(1, height, 5));
        CreateWall(new Vector3(5, height/2, -5), new Vector3(1, height, 5));
        
        // Random obstacles
        CreateWall(new Vector3(0, height/2, 20), new Vector3(10, height, 1));
        CreateWall(new Vector3(0, height/2, -20), new Vector3(10, height, 1));
        CreateWall(new Vector3(20, height/2, 0), new Vector3(1, height, 10));
        CreateWall(new Vector3(-20, height/2, 0), new Vector3(1, height, 10));

        // Reposition Player
        GameObject playerAgent = GameObject.Find("PlayerAgent");
        if (playerAgent != null)
        {
            CharacterController cc = playerAgent.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerAgent.transform.position = new Vector3(-25, 1, -25);
            playerAgent.transform.rotation = Quaternion.LookRotation(new Vector3(1, 0, 1));
            if (cc != null) cc.enabled = true;
        }

        // Reposition Treasure
        GameObject treasure = GameObject.Find("Treasure");
        if (treasure != null)
        {
            treasure.transform.position = new Vector3(0, 0.5f, 0);
        }

        // Reposition Enemy & Patrol Points
        GameObject enemyAgent = GameObject.Find("EnemyAgent");
        if (enemyAgent != null)
        {
            UnityEngine.AI.NavMeshAgent agent = enemyAgent.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;
            enemyAgent.transform.position = new Vector3(25, 1, 25);
            if (agent != null) agent.enabled = true;
        }

        GameObject patrolPointsParent = new GameObject("PatrolPoints");
        CreatePatrolPoint(patrolPointsParent.transform, new Vector3(25, 0, 25));
        CreatePatrolPoint(patrolPointsParent.transform, new Vector3(25, 0, -25));
        CreatePatrolPoint(patrolPointsParent.transform, new Vector3(-25, 0, 25)); // Long patrol
        CreatePatrolPoint(patrolPointsParent.transform, new Vector3(0, 0, 10));
    }

    static void CreateWall(Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.tag = "Wall";
        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer != -1) wall.layer = wallLayer;
        GameObjectUtility.SetStaticEditorFlags(wall, StaticEditorFlags.NavigationStatic);
    }

    static void CreatePatrolPoint(Transform parent, Vector3 position)
    {
        GameObject point = new GameObject("PatrolPoint");
        point.transform.position = position;
        point.transform.parent = parent;
    }
}
