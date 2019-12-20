using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor {

    PathCreator creator;

    Path Path => creator.path;

    const float kAnchorHandleSize = 0.1f;
    const float kSegmentSelectMinDistance = 0.1f;

    int selectedSegmentIndex = -1;

    void OnEnable() {
        creator = (PathCreator)target;
        if (creator.path == null) {
            creator.CreatePath();
        }
    }

    void OnSceneGUI() {
        Draw();
        HandleInput();
    }

    void HandleInput() {
        Event guiEvent = Event.current;
        Vector2 mouseWorldPos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
        if (guiEvent.type == EventType.MouseDown) {
            if (guiEvent.button == 0) {
                // Adding an anchor
                if (guiEvent.shift) {
                    if (selectedSegmentIndex != -1) {
                        Undo.RecordObject(creator, "Insert segment");
                        Path.SplitSegment(mouseWorldPos, selectedSegmentIndex);
                    } else {
                        Undo.RecordObject(creator, "Add segment");
                        Path.AddSegment(mouseWorldPos);
                    }
                }
            } else if (guiEvent.button == 1) {
                // Removing an anchor
                const float kMinMouseDistance = kAnchorHandleSize * 0.5f;
                float shortestDistanceToAnchor = kMinMouseDistance;
                int closestAnchorIndex = -1;
                for (int i = 0; i < Path.PointCount; i += 3) {
                    float currentDistance = (mouseWorldPos - Path[i]).sqrMagnitude;
                    if (currentDistance < shortestDistanceToAnchor) {
                        closestAnchorIndex = i;
                        shortestDistanceToAnchor = currentDistance;
                    }
                }
                if (closestAnchorIndex != -1) {
                    Undo.RecordObject(creator, "Delete segment");
                    Path.DeleteSegment(closestAnchorIndex);
                }
            }
        }

        if (guiEvent.type == EventType.MouseMove) {
            int newSelectedSegmentIndex = -1;
            float shortestDistanceToSegment = kSegmentSelectMinDistance;
            for (int i = 0; i < Path.SegmentCount; i++) {
                Vector2[] points = Path.GetPointsInSegment(i);
                float currentDistance = HandleUtility.DistancePointBezier(mouseWorldPos, points[0],
                    points[3], points[1], points[2]);
                if (currentDistance < shortestDistanceToSegment) {
                    newSelectedSegmentIndex = i;
                    shortestDistanceToSegment = currentDistance;
                }
            }
            if (newSelectedSegmentIndex != selectedSegmentIndex) {
                selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }
    }

    void Draw() {
        for (int i = 0; i < Path.PointCount; i++) {
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
                Handles.color = Color.gray;
                size = kAnchorHandleSize * 0.65f;
            } else {
                Handles.color = Color.red;
            }
            Vector2 newPos = Handles.FreeMoveHandle(Path[i], Quaternion.identity,
                size, Vector3.zero, Handles.CylinderHandleCap);
            if (newPos != Path[i]) {
                Undo.RecordObject(creator, "Move point");
                Path[i] = newPos;
            }
        }
        Handles.color = Color.gray;
        for (int i = 0; i < Path.SegmentCount; i++) {
            Vector2[] segment = Path.GetPointsInSegment(i);
            Color color = i == selectedSegmentIndex && Event.current.shift ?
                Color.yellow : Color.green;
            Handles.DrawBezier(segment[0], segment[3], segment[1], segment[2], color, null, 2f);
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