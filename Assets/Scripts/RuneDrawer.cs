using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RuneDrawer : MonoBehaviour {

    [SerializeField] float minMouseDelta = 0.1f;
    [SerializeField] [Range(0f, 359.99f)] float driftAngle = 20f;
    [SerializeField] float turnDistance = 0.2f;

    Vector3 mouseStartPos;
    Vector3 currentMousePos;
    Vector3 previousMousePos;
    Vector3 dragFromStart;
    Vector3 turnStart;
    Camera mainCamera;
    TrailRenderer trailRenderer;
    float minSqrMouseDelta;
    public RuneLibrary runeLibrary;
    bool isDrawing;
    public AnimationCurve mouseDeltaCurve;
    float timeSinceDrawing;
    List<float> angles = new List<float>();
    List<(Vector2 origin, Vector2 vector)> averageVectors = new List<(Vector2, Vector2)>();
    //List<float> realAngles = new List<float>();
    bool isTurning;

    void Start() {
        mainCamera = Camera.main;
        trailRenderer = GetComponent<TrailRenderer>();
        runeLibrary = GetComponent<RuneLibrary>();

        minSqrMouseDelta = minMouseDelta * minMouseDelta;
    }

    void Update() {
        // Careful: should I measure distance between two points of the screen or two points of the world ?
        // Does it influence the magnitude on bigger resolutions ?
        previousMousePos = currentMousePos;
        currentMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        currentMousePos.z = 0f;

        if (Input.GetMouseButton(0)) {
            Vector3 mouseDelta = currentMousePos - previousMousePos;
            if (!isDrawing) {
                if (mouseDelta.sqrMagnitude < minSqrMouseDelta) {
                    return; // Here is the problem, because this prevents the angles list from being cleared <- obsolete
                }
                angles.Clear();
                //realAngles.Clear();
                timeSinceDrawing = 0f;
                //mouseDeltaCurve.keys = new Keyframe[0];
                isDrawing = true;
                transform.position = currentMousePos;
                trailRenderer.Clear();
                mouseStartPos = previousMousePos;
            }

            transform.position = currentMousePos; // put after next test ? (next line)
            if (mouseDelta.sqrMagnitude < minSqrMouseDelta) {
                return;
            }

            //realAngles.Add(mouseDeltaAngle);
            //mouseDeltaCurve.AddKey(timeSinceDrawing, mouseDeltaAngle * Mathf.Rad2Deg);
            //Debug.DrawRay(Vector3.zero, mouseDelta * 2f, Color.yellow, 1f);

            timeSinceDrawing += Time.deltaTime;
            //Debug.DrawRay(Vector3.zero, dragFromStart.normalized * 3f, Color.red);
            mouseDelta.Normalize();
            //Debug.DrawRay(Vector3.zero, mouseDelta * 1.5f, Color.magenta, 0.5f);
            //Debug.DrawRay(Vector3.zero, dragFromStart * 3f, Color.yellow);

            dragFromStart = previousMousePos - mouseStartPos;

            if (isTurning || (Vector2.Dot(mouseDelta, dragFromStart.normalized) < Mathf.Cos(driftAngle * Mathf.Deg2Rad) && dragFromStart.sqrMagnitude > 0f)) {
                if (!isTurning) {
                    turnStart = previousMousePos;
                    isTurning = true;
                    Debug.DrawRay(turnStart + Vector3.left * turnDistance, Vector3.right * turnDistance * 2f, Color.red, 3f);
                    Debug.DrawRay(turnStart + Vector3.up * turnDistance, Vector3.down * turnDistance * 2f, Color.red, 3f);
                    Debug.DrawRay(mouseStartPos, dragFromStart, Color.red, 3f);
                }
                if ((currentMousePos - turnStart).magnitude < turnDistance) {
                    return;
                }
                isTurning = false;
                Debug.LogWarning("Direction change detected! ");

                float averageAngle = angles.Average();
                if (turnStart.x < mouseStartPos.x) {
                    //Debug.Log("On the left");
                    averageAngle = Mathf.PI - averageAngle;
                }
                Vector3 averageVector = new Vector3(Mathf.Cos(averageAngle), Mathf.Sin(averageAngle)) * dragFromStart.magnitude;
                averageVectors.Add((mouseStartPos, averageVector));
                Debug.DrawRay(mouseStartPos, averageVector, Color.cyan, 2.5f);
                angles.Clear();
                //timeSinceDrawing = 0f;
                mouseStartPos = previousMousePos;

                int avgVectorsCount = averageVectors.Count;
                if (avgVectorsCount % 2 == 0) {
                    (Vector2 origin1, Vector2 first) = averageVectors[avgVectorsCount - 2];
                    (Vector2 origin2, Vector2 second) = averageVectors[avgVectorsCount - 1];
                    float denominator = second.x * first.y - first.x * second.y;
                    Vector2 intersection = new Vector2 {
                        x = (first.x * (second.x * origin2.y - second.y * origin2.x - second.x * origin1.y) +
                        second.x * first.y * origin1.x) / denominator,
                        y = (first.y * (second.x * origin2.y - second.y * origin2.x + second.y * origin1.x) -
                        first.x * origin1.y * second.y) / denominator
                    };
                    Debug.DrawLine(Vector3.zero, intersection, Color.white, 5f);
                }
            } else {
                float mouseDeltaAngle = Mathf.Atan2(mouseDelta.y, mouseDelta.x);
                mouseDeltaAngle = Mathf.Sign(mouseDeltaAngle) * Mathf.PingPong(mouseDeltaAngle, Mathf.PI * 0.5f);
                angles.Add(mouseDeltaAngle);
            }
        } else if (Input.GetMouseButtonUp(0)) {
            //float realAvg = realAngles.Average();
            //Vector3 realAvgVector = new Vector3(Mathf.Cos(realAvg), Mathf.Sin(realAvg));
            //Debug.DrawRay(Vector3.zero, realAvgVector * 3f, Color.red, 2.5f);
            isDrawing = false;
        }
    }
}
