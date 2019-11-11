using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Path {

    [SerializeField, HideInInspector]
    List<Vector2> points;

    public int SegmentCount => (points.Count - 1) / 3;
    public int PointCount => points.Count;

    public Vector2 this[int i] {
        get => points[i];
        set {
            switch (i % 3) {
                case 0:
                    // It is an anchor, so take care of its controls
                    Vector2 displacement = value - points[i];
                    if (i != points.Count - 1) {
                        points[i + 1] += displacement;
                    }
                    if (i != 0) {
                        points[i - 1] += displacement;
                    }
                    break;
                case 1:
                    if (i != 1) {
                        points[i - 2] = (points[i - 1] * 2f) - points[i];
                    }
                    break;
                case 2:
                    if (i != points.Count - 2) {
                        points[i + 2] = (points[i + 1] * 2f) - points[i];
                    }
                    break;
            }
            ////Another way of doing this
            //int mod = i % 3;
            //if (mod == 0) {
            //    Vector2 displacement = value - points[i];
            //    if (i != points.Count - 1) {
            //        points[i + 1] += displacement;
            //    }
            //    if (i != 0) {
            //        points[i - 1] += displacement;
            //    }
            //} else {
            //    int correspondingControlIndex;
            //    int anchorIndex;
            //    if (mod == 1) {
            //        correspondingControlIndex = i - 2;
            //        anchorIndex = i - 1;
            //    } else {
            //        correspondingControlIndex = i + 2;
            //        anchorIndex = i - 2;
            //    }
            //    if (anchorIndex != 0 && anchorIndex != points.Count - 1) {
            //        points[correspondingControlIndex] = (points[anchorIndex] * 2f) - points[i];
            //    }
            //}

            points[i] = value;
        }
    }

    public Path(Vector2 center) {
        points = new List<Vector2> {
            center + Vector2.left,
            center + new Vector2(-0.5f, 0.5f),
            center + new Vector2(0.5f, -0.5f),
            center + Vector2.right
        };
    }

    public void AddSegment(Vector2 anchorPos) {
        Vector2 lastAnchor = points[points.Count - 1];
        Vector2 lastControl = points[points.Count - 2];
        Vector2 newControl = (lastAnchor * 2f) - lastControl;
        points.Add(newControl);
        points.Add((newControl + anchorPos) * 0.5f);
        points.Add(anchorPos);
    }

    public void DeleteSegment(int anchorIndex) {
        if (anchorIndex != 0) {
            if (anchorIndex != points.Count - 1) {
                points.RemoveRange(anchorIndex - 1, 3);
            } else {
                points.RemoveRange(anchorIndex - 2, 3);
            }
        } else {
            points.RemoveRange(anchorIndex, 3);
        }
    }

    public Vector2[] GetPointsInSegment(int i) {
        return new Vector2[4] { points[i*3], points[i*3 + 1],
            points[i*3 + 2], points[i*3 + 3] };
    }
}
