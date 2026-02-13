using UnityEngine;
using UnityEditor;

public class SceneSetup
{
    public static void CreateLevel()
    {
        // Create Ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(5, 1, 5); // 50x50 area
        ground.transform.position = Vector3.zero;
        
        // Create Walls
        CreateWall(new Vector3(-10, 1, 10), new Vector3(20, 2, 1));
        CreateWall(new Vector3(10, 1, 10), new Vector3(20, 2, 1));
        CreateWall(new Vector3(0, 1, -15), new Vector3(40, 2, 1));
        CreateWall(new Vector3(0, 1, 25), new Vector3(40, 2, 1));
        
        // Obstacles
        CreateWall(new Vector3(-5, 1, 0), new Vector3(2, 2, 10));
        CreateWall(new Vector3(5, 1, 5), new Vector3(2, 2, 5));
        CreateWall(new Vector3(10, 1, -5), new Vector3(5, 2, 2));

        // Create Player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1, -10);
        player.tag = "Player";
        
        // Create Treasure
        GameObject treasure = GameObject.CreatePrimitive(PrimitiveType.Cube);
        treasure.name = "Treasure";
        treasure.transform.position = new Vector3(0, 0.5f, 20);
        treasure.GetComponent<Renderer>().material.color = Color.yellow;
        treasure.tag = "Treasure";

        // Create Enemy
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy";
        enemy.transform.position = new Vector3(0, 1, 10);
        enemy.GetComponent<Renderer>().material.color = Color.red;
        enemy.tag = "Enemy";

        // Create Patrol Points
        GameObject patrolPoints = new GameObject("PatrolPoints");
        CreatePatrolPoint(patrolPoints.transform, new Vector3(-10, 0, 10));
        CreatePatrolPoint(patrolPoints.transform, new Vector3(10, 0, 10));
    }

    static void CreateWall(Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.tag = "Wall"; // Ensure tag exists or just use layer later
    }

    static void CreatePatrolPoint(Transform parent, Vector3 position)
    {
        GameObject point = new GameObject("PatrolPoint");
        point.transform.position = position;
        point.transform.parent = parent;
    }
}
