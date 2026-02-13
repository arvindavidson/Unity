using UnityEngine;

public class DebugInput : MonoBehaviour
{
    void OnGUI()
    {
        try
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            
            GUI.Label(new Rect(10, 10, 300, 20), $"Horizontal: {h}");
            GUI.Label(new Rect(10, 30, 300, 20), $"Vertical: {v}");
            GUI.Label(new Rect(10, 50, 300, 20), $"Mouse X: {Input.GetAxis("Mouse X")}");
            GUI.Label(new Rect(10, 70, 300, 20), $"Mouse Y: {Input.GetAxis("Mouse Y")}");
            GUI.Label(new Rect(10, 90, 300, 20), $"Fire1: {Input.GetButton("Fire1")}");
        }
        catch (System.Exception e)
        {
            GUI.Label(new Rect(10, 10, 500, 20), $"Input Error: {e.Message}");
        }
        
        if (Event.current.type == EventType.KeyDown)
        {
            GUI.Label(new Rect(10, 110, 300, 20), $"Key Down: {Event.current.keyCode}");
        }
    }
}
