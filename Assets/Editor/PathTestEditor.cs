using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PathTest))]
public class PathTestEditor : Editor {

    const float kMousePixelDistance = 10f;

    PathTest pathTest;
    int pointIndex = -1;
    bool isMouseDown;

    static List<Vector2> userRawPoints;

    void OnEnable() {
        pathTest = (PathTest)target;
        userRawPoints = pathTest.UserRawPoints;
    }

    void OnSceneGUI() {
        if (pathTest == null) {
            return;
        }
        Event currentEvent = Event.current;
        List<Vector2> pointsToEdit = userRawPoints;
        if (pointsToEdit.Count == 0) {
            return;
        }
        // Select closest point if none is currently selected
        if (pointIndex == -1) {
            for (int i = 0; i < userRawPoints.Count; i++) {
                Vector2 GUIpoint = HandleUtility.WorldToGUIPoint(pointsToEdit[i]);
                if ((currentEvent.mousePosition - GUIpoint).sqrMagnitude <= kMousePixelDistance *
                        kMousePixelDistance) {
                    pointIndex = i;
                    break;
                }
            }
            if (pointIndex == -1) {
                return;
            }
        }

        if (currentEvent.type == EventType.MouseDown) {
            isMouseDown = true;
        } else if (currentEvent.type == EventType.MouseUp) {
            isMouseDown = false;
        }

        EditorGUI.BeginChangeCheck();
        Vector2 newPoint = Handles.FreeMoveHandle(pointsToEdit[pointIndex], Quaternion.identity,
            0.1f, Vector3.zero, Handles.CylinderHandleCap);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(pathTest, "Changed point " + pointIndex);
            pointsToEdit[pointIndex] = newPoint;
        } else if (!isMouseDown) {
            pointIndex = -1;
        }
    }

    [DrawGizmo(GizmoType.Selected)]
    static void DrawNodesGizmos(PathTest pathTest, GizmoType gizmoType) {
        if (pathTest == null) {
            return;
        }

        SceneView sceneView = SceneView.currentDrawingSceneView;
        if (sceneView == null) {
            return;
        }
        float zoomLevel = sceneView.camera.orthographicSize;
        Gizmos.color = Color.white;
        for (int i = 0; i < userRawPoints.Count; i++) {
            if (i == 0) {
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(userRawPoints[i], 0.015f * zoomLevel);
                Gizmos.color = Color.white;
                continue;
            }
            Gizmos.DrawSphere(userRawPoints[i], 0.015f * zoomLevel);
        }
    }
}
