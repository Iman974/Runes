using UnityEngine;
using System.Collections.Generic;

public class PathTest : MonoBehaviour {

    //[SerializeField] List<Vector2> rawTemplatePoints = new List<Vector2>();
    [SerializeField] Unistroke template = null;
    [SerializeField] Unistroke userStroke = null;

    public List<Vector2> TemplateRawPoints => template.RawPoints;
    public List<Vector2> UserRawPoints => userStroke.RawPoints;

    void Start() {
        List<Vector2> templatePoints = template.ProcessPoints();
        List<Vector2> userPoints = userStroke.ProcessPoints();

        // Recognize !
        float a = 0, b = 0;
        for (int i = 0; i < Unistroke.kPointCount; i++) {
            a += Vector2.Dot(templatePoints[i], userPoints[i]);
            b += (templatePoints[i].x * userPoints[i].y) - (templatePoints[i].y * userPoints[i].x);
        }
        float angle = Mathf.Atan2(b, a);
        float final = Mathf.Acos(a * Mathf.Cos(angle) + b * Mathf.Sin(angle));
        Debug.LogWarning("a: " + a + ", b: " + b + ", angle: " +
            angle * Mathf.Rad2Deg + ", Result: " + final * Mathf.Rad2Deg);
    }

    [SerializeField] bool showPoints = true;
    public bool ShowPoints => showPoints;

    private void OnDrawGizmosSelected() {
        if (template.RawPoints == null) {
            return;
        }
        Gizmos.color = Color.cyan;
        Vector2[] rawTemplatePoints = template.RawPoints.ToArray();
        for (int i = 1; i < rawTemplatePoints.Length; i++) {
            Gizmos.DrawLine(rawTemplatePoints[i - 1], rawTemplatePoints[i]);
        }
    }
}
