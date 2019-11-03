﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PathTest))]
public class PathTestEditor : Editor {

    const float kMousePixelDistance = 10f;

    PathTest pathTest;
    int pointIndex = -1;
    bool isMouseDown;

    static List<Vector2> templateRawPoints;
    static List<Vector2> userRawPoints;

    void OnEnable() {
        pathTest = (PathTest)target;
        templateRawPoints = pathTest.TemplateRawPoints;
        userRawPoints = pathTest.UserRawPoints;
    }

    void OnSceneGUI() {
        if (pathTest == null) {
            return;
        }
        Event currentEvent = Event.current;

        // Select closest point if none is currently selected
        if (pointIndex == -1) {
            for (int i = 0; i < templateRawPoints.Count; i++) {
                Vector2 GUIpoint = HandleUtility.WorldToGUIPoint(templateRawPoints[i]);
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
        Vector2 newPoint = Handles.PositionHandle(templateRawPoints[pointIndex], Quaternion.identity);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(pathTest, "Changed point " + pointIndex);
            templateRawPoints[pointIndex] = newPoint;
        } else if (!isMouseDown) {
            pointIndex = -1;
        }
    }

    [DrawGizmo(GizmoType.Selected)]
    static void DrawNodesGizmos(PathTest pathTest, GizmoType gizmoType) {
        if (pathTest == null || !pathTest.ShowPoints) {
            return;
        }

        SceneView sceneView = SceneView.currentDrawingSceneView;
        if (sceneView == null) {
            return;
        }
        float zoomLevel = sceneView.camera.orthographicSize;
        Gizmos.color = Color.white;
        for (int i = 0; i < templateRawPoints.Count; i++) {
            Gizmos.DrawSphere(templateRawPoints[i], 0.015f * zoomLevel);
        }
    }
}
