using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor {

    PathCreator creator;
    Path path;

    const float kAnchorHandleSize = 0.1f;

    void OnEnable() {
        creator = (PathCreator)target;
        if (creator.path == null) {
            creator.CreatePath();
        }
        path = creator.path;
    }

    void OnSceneGUI() {
        Draw();
        HandleInput();
    }

    void HandleInput() {
        Event current = Event.current;
        if (current.type == EventType.MouseDown) {
            Vector2 mouseWorldPos = HandleUtility.GUIPointToWorldRay(current.mousePosition).origin;
            if (current.button == 0) {
                // Adding an anchor
                if (!current.shift) {
                    return;
                }
                Undo.RecordObject(creator, "Add segment");
                path.AddSegment(mouseWorldPos);
            } else if (current.button == 1) {
                // Removing an anchor
                for (int i = 0; i < path.PointCount; i += 3) {
                    const float kMinMouseDistance = kAnchorHandleSize * 0.5f;
                    if ((mouseWorldPos - path[i]).sqrMagnitude <= kMinMouseDistance * kMinMouseDistance) {
                        Undo.RecordObject(creator, "Delete segment");
                        path.DeleteSegment(i);
                        break;
                    }
                }
            }
        }
    }

    void Draw() {
        for (int i = 0; i < path.PointCount; i++) {
            if (!creator.showTangents && i % 3 != 0) {
                continue;
            }
            // This was to try snapping. Use GUIUtility.hotControl to identify
            // which handle is being MOVED. Set this id from the freeMoveHandle function (method override)
            //if (Event.current.control && !hasSnapped) {
            //    Vector2 anchor = path[mod == 1 ? i - 1 : i + 1];
            //    Vector2 anchorToTangent = path[i] - anchor;
            //    snap.x = Mathf.Abs(anchorToTangent.x);
            //    snap.y = Mathf.Abs(anchorToTangent.y);
            //    hasSnapped = true;
            //}
            float size = kAnchorHandleSize;
            if (i == 0) {
                Handles.color = Color.blue;
            } else if (i % 3 != 0) {
                Handles.color = Color.black;
                size = kAnchorHandleSize * 0.65f;
            } else {
                Handles.color = Color.red;
            }
            Vector2 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity,
                size, Vector3.zero, Handles.CylinderHandleCap);
            if (newPos != path[i]) {
                Undo.RecordObject(creator, "Move point");
                path[i] = newPos;
            }
        }
        Handles.color = Color.black;
        for (int i = 0; i < path.SegmentCount; i++) {
            Vector2[] segment = path.GetPointsInSegment(i);
            Handles.DrawBezier(segment[0], segment[3], segment[1], segment[2],
                Color.green, null, 2f);
            if (creator.showTangents) {
                Handles.DrawLine(segment[1], segment[0]);
                Handles.DrawLine(segment[2], segment[3]);
            }
        }
    }

    Vector2 CubicBezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t) {
        return (Mathf.Pow(1 - t, 3f) * a) + (3f * (1f - t) * (1f - t) * t * b) +
                    ((3f - (3f * t)) * t * t * c) + (t * t * t * d);
    }
}