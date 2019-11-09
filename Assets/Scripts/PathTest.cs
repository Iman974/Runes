using UnityEngine;
using System.Collections.Generic;

public class PathTest : MonoBehaviour {

    //[SerializeField] List<Vector2> rawTemplatePoints = new List<Vector2>();
    [SerializeField] Unistroke template = null;
    [SerializeField] Unistroke userStroke = null;

    public List<Vector2> TemplateRawPoints => template.RawPoints;
    public List<Vector2> UserRawPoints => userStroke.RawPoints;

    bool isRecording;
    Camera mainCamera;

    void Start() {
        mainCamera = Camera.main;
    }

    void Recognize() {
        List<Vector2> templatePoints = template.ProcessPoints();
        List<Vector2> userPoints = userStroke.ProcessPoints();

        // Recognize !
        float a = 0, b = 0;
        for (int i = 0; i < Unistroke.kProcessedPointCount; i++) {
            a += Vector2.Dot(templatePoints[i], userPoints[i]);
            b += (templatePoints[i].x * userPoints[i].y) - (templatePoints[i].y * userPoints[i].x);
        }
        float angle = Mathf.Atan2(b, a);
        float final = Mathf.Acos(a * Mathf.Cos(angle) + b * Mathf.Sin(angle));
        Debug.LogWarning("a: " + a + ", b: " + b + ", angle: " +
            angle * Mathf.Rad2Deg + ", Result: " + final * Mathf.Rad2Deg);
    }

    void Update() {
        var userRawPoints = userStroke.RawPoints;

        if (Input.GetMouseButtonDown(0)) {
            isRecording = true;
            userStroke.RawPoints.Clear();
        } else if (Input.GetMouseButton(0)) {
            if (!isRecording) {
                return;
            }
            if (InputUtility.MouseDelta.sqrMagnitude > 0.05f * 0.05f) {
                userRawPoints.Add(InputUtility.WorldMousePosition);
                int count = userRawPoints.Count;
                if (count >= 2) {
                    Debug.DrawLine(userRawPoints[count - 2], userRawPoints[count - 1], Color.magenta,
                        2f);
                }
            } else if (userRawPoints.Count > 0) {
                isRecording = false;
                Debug.LogWarning("Stopped the recording");
            }
        } else if (Input.GetMouseButtonUp(0)) {
            isRecording = false;
            Recognize();
        }
    }

    [System.Serializable]
    public enum ShowType {
        User,
        Template
    }

    public bool showPoints;
    public ShowType editWhichPoints;

    private void OnDrawGizmosSelected() {
        if (template.RawPoints == null) {
            return;
        }
        Gizmos.color = Color.cyan;
        Vector2[] rawTemplatePoints = template.RawPoints.ToArray();
        for (int i = 1; i < rawTemplatePoints.Length; i++) {
            Gizmos.DrawLine(rawTemplatePoints[i - 1], rawTemplatePoints[i]);
        }
        Gizmos.color = Color.yellow;
        Vector2[] rawUserPoints = userStroke.RawPoints.ToArray();
        for (int i = 1; i < rawUserPoints.Length; i++) {
            Gizmos.DrawLine(rawUserPoints[i - 1], rawUserPoints[i]);
        }
    }
}
