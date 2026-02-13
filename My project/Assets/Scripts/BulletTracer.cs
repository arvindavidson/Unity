using UnityEngine;
using System.Collections;

public class BulletTracer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float duration = 0.1f;

    public void Init(Vector3 start, Vector3 end)
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        float elapsed = 0f;
        Color startColor = lineRenderer.startColor;
        Color endColor = lineRenderer.endColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Color c1 = startColor;
            c1.a = Mathf.Lerp(1f, 0f, t);
            lineRenderer.startColor = c1;

            Color c2 = endColor;
            c2.a = Mathf.Lerp(1f, 0f, t);
            lineRenderer.endColor = c2;

            yield return null;
        }

        Destroy(gameObject);
    }
}
