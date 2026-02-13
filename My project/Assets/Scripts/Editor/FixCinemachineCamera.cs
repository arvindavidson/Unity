using UnityEngine;
using UnityEditor;
using Unity.Cinemachine;

public class FixCinemachineCamera : MonoBehaviour
{
    public static void Execute()
    {
        // Find the virtual camera
        GameObject vcamObj = GameObject.Find("CM_TopDownCamera");
        if (vcamObj == null)
        {
            Debug.LogError("CM_TopDownCamera not found!");
            return;
        }

        var vcam = vcamObj.GetComponent<CinemachineCamera>();
        var follow = vcamObj.GetComponent<CinemachineFollow>();
        
        // Remove the RotationComposer — it causes jitter for a fixed top-down view
        var composer = vcamObj.GetComponent<CinemachineRotationComposer>();
        if (composer != null)
        {
            DestroyImmediate(composer);
            Debug.Log("Removed CinemachineRotationComposer (was causing jitter).");
        }

        // Also remove any other aim components
        var hardLook = vcamObj.GetComponent<CinemachineHardLookAt>();
        if (hardLook != null) DestroyImmediate(hardLook);

        // Remove LookAt target — we'll use a fixed rotation instead
        vcam.LookAt = null;

        // Zoom out significantly — higher Y offset and further back Z
        if (follow != null)
        {
            follow.FollowOffset = new Vector3(0, 30, -15);
            
            // Use SerializedObject to set damping since TrackerSettings is a struct
            var so = new SerializedObject(follow);
            var dampProp = so.FindProperty("TrackerSettings.PositionDamping");
            if (dampProp != null)
            {
                dampProp.vector3Value = new Vector3(0.8f, 0.5f, 0.8f);
                so.ApplyModifiedProperties();
                Debug.Log("Set position damping via SerializedObject.");
            }
            else
            {
                Debug.LogWarning("Could not find TrackerSettings.PositionDamping property.");
            }

            Debug.Log("Updated FollowOffset to (0, 30, -15).");
        }

        // Set a fixed downward rotation for stable top-down view
        // Looking down at ~63 degrees gives a nice top-down-ish angle
        vcamObj.transform.rotation = Quaternion.Euler(63f, 0f, 0f);

        // Find the main camera and increase FOV for a wider view
        GameObject mainCam = GameObject.Find("Main Camera");
        if (mainCam != null)
        {
            Camera cam = mainCam.GetComponent<Camera>();
            if (cam != null)
            {
                cam.fieldOfView = 50f;
                EditorUtility.SetDirty(cam);
                Debug.Log("Set Main Camera FOV to 50.");
            }
        }

        EditorUtility.SetDirty(vcamObj);
        EditorUtility.SetDirty(vcam);
        if (follow != null) EditorUtility.SetDirty(follow);

        Debug.Log("Cinemachine camera fix complete — zoomed out, fixed angle, smoother damping.");
    }
}
