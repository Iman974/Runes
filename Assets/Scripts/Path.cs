using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    public void SplitSegment(Vector2 anchorPos, int segmentIndex) {
        float distanceToPreviousAnchor = (points[segmentIndex * 3] - anchorPos).sqrMagnitude;
        float distanceToNextAnchor = (points[segmentIndex * 3 + 3] - anchorPos).sqrMagnitude;
        int closestControlIndex = distanceToPreviousAnchor < distanceToNextAnchor ?
            segmentIndex * 3 + 1 : segmentIndex * 3 + 2;

        int newControlIndex = ((closestControlIndex % 3) - 1) * 2;
        Vector2[] newPoints = new Vector2[3];
        newPoints[newControlIndex] = (points[closestControlIndex] + anchorPos) * 0.5f;
        newPoints[1] = anchorPos;
        newPoints[2 - newControlIndex] = (anchorPos * 2f) - newPoints[newControlIndex];

        points.InsertRange(segmentIndex * 3 + 2, newPoints);
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

    public Vector2[] CalculateEvenlySpacedPoints() {
        // First pass to calculate the total curve length
        float[] curveLengthsInSegment = new float[SegmentCount];
        for (int i = 0; i < SegmentCount; i++) {
            curveLengthsInSegment[i] = GetCurveLengthInSegment(i);
        }
        float spacing = curveLengthsInSegment.Sum() / (Unistroke.kEvenPointCount - 1);

        Vector2[] resultPoints = new Vector2[Unistroke.kEvenPointCount];
        resultPoints[0] = points[0];
        float remainingDistance = 0f;
        int resultPointIndex = 1;

        // Second pass to calculate the evenly spaced points
        for (int i = 0; i < SegmentCount; i++) {
            const int kResolution = 10;
            int divisions = Mathf.CeilToInt(curveLengthsInSegment[i]) * kResolution;
            Vector2[] segmentPoints = GetPointsInSegment(i);
            Vector2 previousPoint = segmentPoints[0];
            for (int j = 1; j <= divisions; j++) {
                Vector2 pointOnCurve = CalculateBezier(segmentPoints[0], segmentPoints[1],
                    segmentPoints[2], segmentPoints[3], j / (float)divisions);

                float distanceSinceLastEvenPoint = Vector2.Distance(pointOnCurve, previousPoint);

                // During this loop, new points will be placed between the previous and the current one
                // until the distance from the newly placed point to the current one is shorter than spacing
                while (remainingDistance + distanceSinceLastEvenPoint >= spacing) {
                    // Using Thales theorem, we make a new vector with length = spacing and
                    // with the same direction as the longest vector. It is a resize operation
                    Vector2 newEvenPoint = new Vector2 {
                        x = previousPoint.x + (spacing - remainingDistance) *
                            (pointOnCurve.x - previousPoint.x) / distanceSinceLastEvenPoint,
                        y = previousPoint.y + ((spacing - remainingDistance) *
                            (pointOnCurve.y - previousPoint.y) / distanceSinceLastEvenPoint)
                    };
                    resultPoints[resultPointIndex] = newEvenPoint;
                    resultPointIndex++;
                    previousPoint = newEvenPoint;
                    distanceSinceLastEvenPoint -= spacing - remainingDistance;
                    remainingDistance = 0f;
                }
                previousPoint = pointOnCurve;

                // remainingDistance is always shorter than spacing
                remainingDistance += distanceSinceLastEvenPoint;
            }
        }
        // Sometimes we fall a rounding-error short of adding the last point, so add it if so
        if (resultPointIndex == Unistroke.kEvenPointCount - 1) {
            resultPoints[Unistroke.kEvenPointCount - 1] = points[points.Count - 1];
        }
        return resultPoints;
    }

    float GetCurveLengthInSegment(int segmentIndex) {
        const int kPrecision = 50;
        float lengthSum = 0f;
        Vector2[] segmentPoints = GetPointsInSegment(segmentIndex);
        Vector2 previousPoint = segmentPoints[0];
        for (int j = 1; j <= kPrecision; j++) {
            Vector2 pointOnCurve = CalculateBezier(segmentPoints[0], segmentPoints[1],
                segmentPoints[2], segmentPoints[3], j / (float)kPrecision );
            lengthSum += (pointOnCurve - previousPoint).magnitude;
            previousPoint = pointOnCurve;
        }
        return lengthSum;
    }

    Vector2 CalculateBezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t) {
        return a + 3f*(b-a)*t + 3f*(c-2*b+a)*t*t + (d-3*c+3*b-a)*t*t*t;
    }
}
