using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor {

    PathCreator creator;
    Path path;

    void OnEnable() {
        creator = (PathCreator)target;
        if (creator.path == null) {
            creator.CreatePath();
        }
        path = creator.path;
    }


    void OnSceneGUI() {
        Draw();
        Input();
    }

    void Input() {
        Event current = Event.current;
        if (current.type == EventType.MouseDown && current.button == 0 &&
            current.shift) {
            Undo.RecordObject(creator, "Add segment");
            path.AddSegment(HandleUtility.GUIPointToWorldRay(current.mousePosition).origin);
        }
    }

    void Draw() {
        Handles.color = Color.red;
        for (int i = 0; i < path.PointCount; i++) {
            Vector2 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity,
                0.1f, Vector3.zero, Handles.CylinderHandleCap);
            if (newPos != path[i]) {
                Undo.RecordObject(creator, "Move point");
                path[i] = newPos;
            }
        }
        Handles.color = Color.white;
        for (int i = 0; i < path.SegmentCount; i++) {
            Vector2[] segment = path.GetPointsInSegment(i);
            Handles.DrawBezier(segment[0], segment[3], segment[1], segment[2],
                Color.green, null, 2f);
            Handles.DrawLine(segment[1], segment[0]);
            Handles.DrawLine(segment[2], segment[3]);
        }
    }

    Vector2 CubicBezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t) {
        return (Mathf.Pow(1 - t, 3f) * a) + (3f * (1f - t) * (1f - t) * t * b) +
                    ((3f - (3f * t)) * t * t * c) + (t * t * t * d);
    }
}