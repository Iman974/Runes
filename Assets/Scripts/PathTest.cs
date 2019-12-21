using UnityEngine;
using System.Collections.Generic;

public class PathTest : MonoBehaviour {

    //[SerializeField] List<Vector2> rawTemplatePoints = new List<Vector2>();
    [SerializeField] PathCreator templatePathCreator = null;

    //public List<Vector2> TemplateRawPoints => template.RawPoints;
    public List<Vector2> UserRawPoints => userStroke.RawPoints;

    public bool oSensitive;
    public bool usePresetUserPoints;
    public bool regenerate;
    public float sensibility = 0.04f;
    public float minPointDst = 0.02f;

    bool isRecording;
    Camera mainCamera;
    Path template;
    [SerializeField] Unistroke userStroke = new Unistroke();

    void Start() {
        mainCamera = Camera.main;
        template = templatePathCreator.Path;
        if (usePresetUserPoints) {
            Recognize();
        }
        Application.targetFrameRate = 25;
        QualitySettings.vSyncCount = 0;
    }

    void Update() {
        if (usePresetUserPoints) {
            if (regenerate) {
                Recognize();
                regenerate = false;
            }
            return;
        }
        List<Vector2> userRawPoints = userStroke.RawPoints;
#if UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0)) {
            isRecording = true;
            userStroke.RawPoints.Clear();
        } else if (Input.GetMouseButton(0)) {
            if (!isRecording) {
                return;
            }
            if (InputUtility.MouseDelta.sqrMagnitude >= 0.05f * 0.05f) {
                userRawPoints.Add(InputUtility.WorldMousePosition);
                int count = userRawPoints.Count;
                if (count >= 2) {
                    Debug.DrawLine(userRawPoints[count - 2], userRawPoints[count - 1], Color.magenta, 2f);
                }
            } else if (userRawPoints.Count > 0) {
                isRecording = false;
                Debug.LogWarning("Stopped the recording");
            }
        } else if (Input.GetMouseButtonUp(0)) {
            isRecording = false;
            Recognize();
        }
#elif UNITY_ANDROID
        if (Input.touchCount == 0) {
            return;
        }
        Touch touch = Input.GetTouch(0);
        if (touch.deltaPosition.sqrMagnitude >= sensibility * sensibility) {
            if (!isRecording) {
                Debug.LogWarning("Started recording");
                isRecording = true;
                userStroke.RawPoints.Clear();
            }
            int count = userRawPoints.Count;
            Vector2 newPos = InputUtility.WorldMousePosition;
            if (count == 0) {
                userRawPoints.Add(InputUtility.WorldMousePosition);
            } else if ((newPos - userRawPoints[count - 1]).sqrMagnitude >= minPointDst * minPointDst) {
                userRawPoints.Add(InputUtility.WorldMousePosition);
                if (count >= 2) {
                    Debug.DrawLine(userRawPoints[count - 2], userRawPoints[count - 1], Color.magenta, 4f);
                }
            } else {
                Debug.Log("Canceled point");
            }
        }
        if (touch.phase == TouchPhase.Ended) {
            Debug.LogWarning("Recording is over");
            isRecording = false;
            Recognize();
        }
#endif
    }

    void Recognize() {
        Vector2[] templatePoints = template.ComputeEvenlySpacedPoints();
        Vector2[] userPoints = userStroke.ComputeEvenlySpacedPoints();
        if (userPoints.Length != Unistroke.kEvenPointCount) {
            Debug.LogWarning("Aborted recognization");
            return;
        }

        NormalizePoints(templatePoints);
        NormalizePoints(userPoints);

        for (int i = 1; i < Unistroke.kEvenPointCount; i++) {
            Debug.DrawLine(templatePoints[i-1], templatePoints[i], Color.red, 5f);
            Debug.DrawLine(userPoints[i - 1], userPoints[i], Color.cyan, 5f);
        }

        float a = 0, b = 0;
        for (int i = 0; i < Unistroke.kEvenPointCount; i++) {
            a += Vector2.Dot(templatePoints[i], userPoints[i]);
            b += (templatePoints[i].x * userPoints[i].y) - (templatePoints[i].y * userPoints[i].x);
        }
        float angle = Mathf.Atan2(b, a);
        float final = Mathf.Acos(a * Mathf.Cos(angle) + b * Mathf.Sin(angle));
        Debug.Log("a: " + a + ", b: " + b + ", angle: " +
            angle * Mathf.Rad2Deg + ", Result: " + final * Mathf.Rad2Deg);
    }

    void NormalizePoints(Vector2[] evenlySpacedPoints) {
        Vector2 centroid = ComputeCentroid(evenlySpacedPoints);
        TranslateCentroidToOrigin(evenlySpacedPoints, centroid);

        float indicativeAngle = Mathf.Atan2(evenlySpacedPoints[0].y, evenlySpacedPoints[0].x);
        float delta;
        if (oSensitive) {
            delta = Mathf.PI * 0.25f *
            Mathf.Floor((indicativeAngle + Mathf.PI / 8) / (Mathf.PI * 0.25f)) - indicativeAngle;
        } else {
            delta = -indicativeAngle;
        }
        Debug.LogWarning("Delta: " + delta);
        float sum = 0;
        // Rotate the template so that the first point (vector) is at 0 radians
        for (int i = 0; i < evenlySpacedPoints.Length; i++) {
            evenlySpacedPoints[i] = Matrix2x2.CreateRotation(delta) * evenlySpacedPoints[i];
            sum += evenlySpacedPoints[i].sqrMagnitude;
        }
        // Resize the whole path to a normalized size (using cosine distances)
        float magnitude = Mathf.Sqrt(sum);
        for (int i = 0; i < evenlySpacedPoints.Length; i++) {
            evenlySpacedPoints[i] /= magnitude;
        }
    }

    Vector2 ComputeCentroid(Vector2[] points) {
        Vector2 centroid = Vector2.zero;
        foreach (Vector2 point in points) {
            centroid += point;
        }
        return centroid / points.Length;
    }

    // Move all the points so that the centroid is at the origin (0, 0)
    void TranslateCentroidToOrigin(Vector2[] points, Vector2 centroid) {
        for (int i = 0; i < points.Length; i++) {
            points[i] -= centroid;
        }
    }

    private void OnDrawGizmosSelected() {
        if (userStroke.RawPoints.Count == 0) {
            return;
        }
        Gizmos.color = Color.cyan;
        var rawTemplatePoints = userStroke.RawPoints;
        for (int i = 1; i < rawTemplatePoints.Count; i++) {
            Gizmos.DrawLine(rawTemplatePoints[i - 1], rawTemplatePoints[i]);
        }
    }
}
