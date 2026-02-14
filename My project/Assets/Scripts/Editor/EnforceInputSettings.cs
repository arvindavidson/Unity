using UnityEngine;
using UnityEditor;

public class EnforceInputSettings
{
    [InitializeOnLoadMethod]
    static void Enforce()
    {
        // 0 = Legacy, 1 = New, 2 = Both
        // checking if we are already on New or Both
        // We want 'Both' to ensure legacy assets/UI bits don't instantly break, 
        // but New is active.
        
        // Actually, let's just set it to 'Both' (2) to be safe.
        // But if the user wants purely new, checking might be better.
        // Given I left Input.mousePosition in Crosshair.cs as fallback, 'Both' is required 
        // unless I fix that too. 
        
        // Let's set it to 'Both'.
        
        /* 
           NOTE: Changing this might restart the editor backend in some versions, 
           but usually it requires a restart. 
           We will just log the current state and warn if it's Old.
        */
        
        // We will strictly use SerializedObject to be safe across versions
        var projectSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
        if (projectSettings.Length > 0)
        {
            var so = new SerializedObject(projectSettings[0]);
            var activeInputHandlerProp = so.FindProperty("activeInputHandler");

            if (activeInputHandlerProp != null)
            {
                int current = activeInputHandlerProp.intValue;
                
                if (current == 0) // Legacy
                {
                    Debug.LogWarning("Switching Active Input Handling to 'Both' (Legacy + New) for Controller Support...");
                    activeInputHandlerProp.intValue = 2; // Both
                    so.ApplyModifiedProperties();
                    Debug.Log("Active Input Handling set to: Both. Editor might need restart.");
                }
                else
                {
                    // 1=New, 2=Both
                     // Debug.Log($"Active Input Handling is already set to: {current} (1=New, 2=Both). Good.");
                }
            }
        }
    }
}
