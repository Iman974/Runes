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
    List<float> realAngles = new List<float>();
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
            Debug.DrawRay(Vector3.zero, mouseDelta * 1.5f, Color.magenta, 0.5f);
            //Debug.DrawRay(Vector3.zero, dragFromStart * 3f, Color.yellow);

            dragFromStart = previousMousePos - mouseStartPos;

            if (isTurning || (Vector2.Dot(mouseDelta, dragFromStart.normalized) < Mathf.Cos(driftAngle * Mathf.Deg2Rad) && dragFromStart.sqrMagnitude > 0f)) {
                if (!isTurning) {
                    turnStart = previousMousePos;
                    isTurning = true;
                    Debug.DrawRay(turnStart, Vector3.down * turnDistance, Color.green, 3f);
                    Debug.DrawRay(turnStart, Vector3.up * turnDistance, Color.green, 3f);
                    Debug.DrawRay(turnStart, Vector3.left * turnDistance, Color.green, 3f);
                    Debug.DrawRay(turnStart, Vector3.right * turnDistance, Color.green, 3f);
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
                Vector3 averageVector = new Vector3(Mathf.Cos(averageAngle), Mathf.Sin(averageAngle));
                Debug.DrawRay(mouseStartPos, averageVector * dragFromStart.magnitude, Color.cyan, 2.5f);
                angles.Clear();
                //timeSinceDrawing = 0f;
                mouseStartPos = previousMousePos;
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
