using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FixCompoundMaterials
{
    public static string Execute()
    {
        string matFolder = "Assets/Materials";
        Material groundMat = AssetDatabase.LoadAssetAtPath<Material>($"{matFolder}/M_Ground.mat");
        Material wallMat = AssetDatabase.LoadAssetAtPath<Material>($"{matFolder}/M_Wall.mat");
        Material treasureMat = AssetDatabase.LoadAssetAtPath<Material>($"{matFolder}/M_Treasure.mat");
        Material enemyBodyMat = AssetDatabase.LoadAssetAtPath<Material>($"{matFolder}/M_EnemyBody.mat");
        Material enemyVisorMat = AssetDatabase.LoadAssetAtPath<Material>($"{matFolder}/M_EnemyVisor.mat");

        int count = 0;
        MeshRenderer[] allRenderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);

        foreach (var renderer in allRenderers)
        {
            string name = renderer.gameObject.name;
            string parentName = renderer.transform.parent != null ? renderer.transform.parent.name : "";

            if (name.StartsWith("Ground") && groundMat != null)
            {
                renderer.sharedMaterial = groundMat;
                count++;
            }
            else if (name.StartsWith("Wall") && wallMat != null)
            {
                renderer.sharedMaterial = wallMat;
                count++;
            }
            else if (name.StartsWith("Treasure") && treasureMat != null)
            {
                renderer.sharedMaterial = treasureMat;
                count++;
            }
            else if (name == "EnemyBody" && enemyBodyMat != null)
            {
                renderer.sharedMaterial = enemyBodyMat;
                count++;
            }
            else if (name == "Visor" && parentName.StartsWith("EnemyAgent") && enemyVisorMat != null)
            {
                renderer.sharedMaterial = enemyVisorMat;
                count++;
            }

            EditorUtility.SetDirty(renderer);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return $"Fixed materials on {count} objects (using StartsWith matching).";
    }
}
