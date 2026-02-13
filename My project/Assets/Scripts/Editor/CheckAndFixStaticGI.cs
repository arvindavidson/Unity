using UnityEngine;
using UnityEditor;

public class CheckAndFixStaticGI : MonoBehaviour
{
    public static void Execute()
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int fixedCount = 0;

        foreach (var obj in allObjects)
        {
            string name = obj.name;

            if (name == "Ground" || name == "Wall" || name == "Treasure")
            {
                StaticEditorFlags currentFlags = GameObjectUtility.GetStaticEditorFlags(obj);
                bool hasGI = (currentFlags & StaticEditorFlags.ContributeGI) != 0;

                Debug.Log($"'{name}' — isStatic: {obj.isStatic}, Flags: {currentFlags}, ContributeGI: {hasGI}");

                if (!hasGI)
                {
                    // Add ContributeGI flag while keeping existing flags
                    currentFlags |= StaticEditorFlags.ContributeGI;
                    GameObjectUtility.SetStaticEditorFlags(obj, currentFlags);
                    obj.isStatic = true;
                    EditorUtility.SetDirty(obj);
                    fixedCount++;
                    Debug.Log($"  -> FIXED: Added ContributeGI to '{name}'");
                }
            }
        }

        Debug.Log($"Check complete. Fixed {fixedCount} objects.");
    }
}
