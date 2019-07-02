using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RuneDrawer : MonoBehaviour {

    [SerializeField] float minMouseDelta = 3.5f;
    [SerializeField] [Range(0f, 359.99f)] float driftAngle = 15f;
    //[SerializeField] float trailSmoothTime = 0.1f;

    Vector3 mouseStartPos;
    Vector3 currentMousePos;
    Vector3 previousMousePos;
    Vector3 dragFromStart;
    Camera mainCamera;
    TrailRenderer trailRenderer;
    float minSqrMouseDelta;
    public RuneLibrary runeLibrary;
    bool isDrawing;
    public AnimationCurve mouseDeltaCurve;
    float timeSinceDrawing;
    [Range(-179.99f, 180f)] public float first;
    [Range(-179.99f, 180f)] public float second;
    List<float> angles = new List<float>();
    Rect debugPos = new Rect(Vector2.one * 20f, new Vector2(200f, 60f));
    float realAngle;
    //float trailSmoothVelocity;
    //float widthMultiplier;

    void Start() {
        mainCamera = Camera.main;
        trailRenderer = GetComponent<TrailRenderer>();
        runeLibrary = GetComponent<RuneLibrary>();

        minSqrMouseDelta = minMouseDelta * minMouseDelta;
        //widthMultiplier = 1f / minSqrMouseDelta;
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
                    //Debug.Log(mouseDeltaSqrMagnitude);
                    //trailRenderer.widthMultiplier = Mathf.SmoothDamp(trailRenderer.widthMultiplier,
                    //    mouseDeltaSqrMagnitude * widthMultiplier, ref trailSmoothVelocity, trailSmoothTime);
                    return; // Here is the problem, because this prevents the angles list from being cleared
                }
                angles.Clear();
                isDrawing = true;
                transform.position = currentMousePos;
                trailRenderer.Clear();
                mouseStartPos = previousMousePos;
            }

            transform.position = currentMousePos;
            if (Input.GetMouseButtonDown(0)) {
                //mouseDeltaCurve.keys = new Keyframe[0];
                //angles.Clear();
                //Debug.LogWarning("List cleared!");
                //timeSinceDrawing = 0f;
                trailRenderer.Clear();
            }
            if (mouseDelta.sqrMagnitude >= minSqrMouseDelta) {
                //mouseDelta.Normalize();
                float mouseDeltaAngle = Mathf.Atan2(mouseDelta.y, mouseDelta.x) * Mathf.Rad2Deg;
                //mouseDeltaAngle = Mathf.Sign(mouseDeltaAngle) * Mathf.PingPong(mouseDeltaAngle, Mathf.PI * 0.5f);
                dragFromStart = currentMousePos - mouseStartPos;
                //dragFromStart.Normalize();
                //Debug.DrawRay(Vector3.zero, mouseDelta * 2f, Color.yellow, 1f);
                //mouseDeltaCurve.AddKey(timeSinceDrawing, mouseDeltaAngle);
                angles.Add(mouseDeltaAngle);
                //Debug.Log("Count before release: " + angles.Count);
            } else {
                return;
            }

            //timeSinceDrawing += Time.deltaTime;
            //Debug.DrawRay(Vector3.zero, dragFromStart.normalized * 3f, Color.red);
            mouseDelta.Normalize();
            dragFromStart.Normalize();
            Debug.DrawRay(Vector3.zero, mouseDelta * 2f, Color.magenta, 0.5f);
            Debug.DrawRay(Vector3.zero, dragFromStart * 3f, Color.yellow);

            if (Vector2.Dot(mouseDelta, dragFromStart) < Mathf.Cos(driftAngle * Mathf.Deg2Rad)) {
                float dragAngle = Mathf.Atan2(dragFromStart.y, dragFromStart.x) * Mathf.Rad2Deg;
                Debug.LogWarning("Direction change detected! Min: " + Mathf.Min(angles.ToArray()) +
                    ", Max: " + Mathf.Max(angles.ToArray()) + ", Current: " + dragAngle);
                //Vector2 orthogonal = Vector2.Perpendicular(runeLibrary[0].Vector);
                //float dist = Mathf.Abs(Vector2.Dot(drag, orthogonal)) / orthogonal.magnitude;
                //if (dist > driftDistance) {
                //    Debug.Log("WRONG! " + dist);
                //}
            }

        } else if (Input.GetMouseButtonUp(0)) {
            Debug.Log("Diff: " + (Mathf.Max(angles.ToArray()) - Mathf.Min(angles.ToArray())));
            //float averageMouseAngle = 0;
            //realAngle = 0;
            //Debug.LogWarning("---------------------------------");
            //Debug.Log("final list count: " + angles.Count);
            //Keyframe[] keyframes = mouseDeltaCurve.keys;
            //for (int i = 0; i < keyframes.Length; i++) {
                //AnimationUtility.SetKeyLeftTangentMode(mouseDeltaCurve, i, AnimationUtility.TangentMode.Linear);
                //AnimationUtility.SetKeyRightTangentMode(mouseDeltaCurve, i, AnimationUtility.TangentMode.Linear);
                //averageMouseAngle += keyframes[i].value;
                //realAngle += angles[i];
                //Debug.Log(angles[i]);
            //}
            //averageMouseAngle =/* Mathf.PI - */(averageMouseAngle / keyframes.Length) * Mathf.Deg2Rad;
            //realAngle = realAngle / angles.Count * Mathf.Deg2Rad;
            //Debug.LogWarning("Angle: " + (averageMouseAngle * Mathf.Rad2Deg));
            //Vector3 finalAngle = new Vector3(Mathf.Cos(averageMouseAngle), Mathf.Sin(averageMouseAngle));
            //Vector3 fRealAngle = new Vector3(Mathf.Cos(realAngle), Mathf.Sin(realAngle));
            //Debug.DrawRay(Vector3.zero, finalAngle * 3f, Color.magenta, 3f);
            //Debug.DrawRay(Vector3.zero, fRealAngle * 2.5f, Color.red, 3f);
            isDrawing = false;
        }
    }
}
