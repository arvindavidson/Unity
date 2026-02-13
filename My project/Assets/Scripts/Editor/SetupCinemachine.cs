using UnityEngine;
using UnityEditor;
using Unity.Cinemachine;

public class SetupCinemachine : MonoBehaviour
{
    public static void Execute()
    {
        // 1. Find the Main Camera and remove the old TopDownCamera script
        GameObject mainCam = GameObject.Find("Main Camera");
        if (mainCam == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }

        TopDownCamera oldCam = mainCam.GetComponent<TopDownCamera>();
        if (oldCam != null)
        {
            DestroyImmediate(oldCam);
            Debug.Log("Removed old TopDownCamera script from Main Camera.");
        }

        // Add CinemachineBrain to the Main Camera if not already there
        var brain = mainCam.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            brain = mainCam.AddComponent<CinemachineBrain>();
            Debug.Log("Added CinemachineBrain to Main Camera.");
        }

        // 2. Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure PlayerAgent has the 'Player' tag.");
            return;
        }

        // 3. Check if a virtual camera already exists
        GameObject existingVCam = GameObject.Find("CM_TopDownCamera");
        if (existingVCam != null)
        {
            DestroyImmediate(existingVCam);
            Debug.Log("Removed existing CM_TopDownCamera.");
        }

        // 4. Create a new GameObject for the virtual camera
        GameObject vcamObj = new GameObject("CM_TopDownCamera");
        Undo.RegisterCreatedObjectUndo(vcamObj, "Create Cinemachine Top Down Camera");

        // 5. Add CinemachineCamera component
        var vcam = vcamObj.AddComponent<CinemachineCamera>();

        // 6. Set the follow and look-at targets
        vcam.Follow = player.transform;
        vcam.LookAt = player.transform;

        // 7. Add and configure CinemachineFollow (body) for top-down offset
        var follow = vcamObj.AddComponent<CinemachineFollow>();
        follow.FollowOffset = new Vector3(0, 15, -8);
        follow.TrackerSettings.PositionDamping = new Vector3(1.5f, 1f, 1.5f);

        // 8. Add and configure CinemachineRotationComposer (aim)
        var composer = vcamObj.AddComponent<CinemachineRotationComposer>();
        composer.Damping = new Vector2(0.5f, 0.5f);

        // 9. Position the virtual camera initially
        vcamObj.transform.position = player.transform.position + new Vector3(0, 15, -8);
        vcamObj.transform.LookAt(player.transform);

        // Mark everything dirty
        EditorUtility.SetDirty(vcamObj);
        EditorUtility.SetDirty(mainCam);

        Debug.Log("Cinemachine setup complete! CM_TopDownCamera created and configured.");
        Debug.Log($"Follow target: {player.name}, Offset: (0, 15, -8), Damping: (1.5, 1, 1.5)");
    }
}
