using UnityEngine;
using UnityEditor;

public class RebakeLighting : MonoBehaviour
{
    public static void Execute()
    {
        Lightmapping.BakeAsync();
        Debug.Log("Lightmap re-bake started. All static objects now contribute to GI.");
    }
}
