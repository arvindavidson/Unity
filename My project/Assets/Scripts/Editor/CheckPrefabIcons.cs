using UnityEngine;
using UnityEditor;

public class CheckPrefabIcons : MonoBehaviour
{
    [MenuItem("Tools/Check Prefab Icons")]
    public static void Check()
    {
        string prefabPath = "Assets/Prefabs/EnemyAgent.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        if (prefabRoot == null)
        {
            Debug.LogError("Could not load prefab at " + prefabPath);
            return;
        }

        try
        {
            EnemyStatusUI statusUI = prefabRoot.GetComponentInChildren<EnemyStatusUI>();
            if (statusUI == null)
            {
                Debug.LogError("EnemyStatusUI component missing from prefab!");
            }
            else
            {
                Debug.Log($"Eye Icon: {(statusUI.eyeIcon != null ? statusUI.eyeIcon.name : "NULL")}");
                Debug.Log($"Question Icon: {(statusUI.questionIcon != null ? statusUI.questionIcon.name : "NULL")}");
                Debug.Log($"Exclamation Icon: {(statusUI.exclamationIcon != null ? statusUI.exclamationIcon.name : "NULL")}");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}
