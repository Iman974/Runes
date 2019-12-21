using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Unistroke {

    public const int kEvenPointCount = 32;

    public List<Vector2> RawPoints { get; } = new List<Vector2>();

    //public List<Vector2> ProcessPoints() {
    //    List<Vector2> resampledPoints = ResampleToEvenlySpaced();

    //    // Only for debug purposes
    //    Vector2 centroid = ComputeCentroid(resampledPoints);
    //    //DrawCentroid(centroid, Color.cyan);

    //    TranslateCentroidToOrigin(resampledPoints, centroid);
    //    //DrawCentroid(ComputeCentroid(resampledPoints), Color.green);
    //    for (int i = 1; i < resampledPoints.Count; i++) {
    //        Debug.DrawLine(resampledPoints[i - 1], resampledPoints[i], Color.green, 2f);
    //    }

    //    float delta = -Mathf.Atan2(resampledPoints[0].y, resampledPoints[0].x);
    //    float sum = 0;
    //    // Rotate the template so that the first point is at 0 radians
    //    for (int i = 0; i < resampledPoints.Count; i++) {
    //        resampledPoints[i] = Matrix2x2.CreateRotation(delta) * resampledPoints[i];
    //        sum += resampledPoints[i].sqrMagnitude;
    //    }
    //    // Resize the whole template to a normalized size (using cosine distances)
    //    float magnitude = Mathf.Sqrt(sum);
    //    for (int i = 0; i < resampledPoints.Count; i++) {
    //        resampledPoints[i] /= magnitude;
    //        //if (i > 0) {
    //        //    Debug.DrawLine(resampledPoints[i - 1], resampledPoints[i], Color.red, 2f);
    //        //}
    //    }
    //    return resampledPoints;
    //}

    public Vector2[] ComputeEvenlySpacedPoints() {
        float spacing = GetPathLength(RawPoints) / (kEvenPointCount - 1);
        float remainingDistance = 0f;
        List<Vector2> sourcePoints = new List<Vector2>(RawPoints);
        List<Vector2> resultPoints = new List<Vector2>(kEvenPointCount);
        resultPoints.Add(sourcePoints[0]);

        for (int i = 1; i < sourcePoints.Count; i++) {
            Vector2 point1 = sourcePoints[i - 1];
            Vector2 point2 = sourcePoints[i];

            float distanceSinceLastEvenPoint = Vector2.Distance(point1, point2);

            if ((remainingDistance + distanceSinceLastEvenPoint) >= spacing) {
                // Using Thales theorem, we make a new vector with length = interval and
                // with the same direction as the longest vector. It is a resize operation
                Vector2 resizedVector = new Vector2 {
                    x = point1.x + ((spacing - remainingDistance) * (point2.x - point1.x) /
                    distanceSinceLastEvenPoint),
                    y = point1.y + ((spacing - remainingDistance) * (point2.y - point1.y) /
                    distanceSinceLastEvenPoint)
                };
                resultPoints.Add(resizedVector);
                sourcePoints.Insert(i, resizedVector);
                remainingDistance = 0f;
            } else {
                // If vector's length is smaller than interval
                remainingDistance += distanceSinceLastEvenPoint;
            }
        }

        if (resultPoints.Count == kEvenPointCount - 1) {
            resultPoints.Add(sourcePoints[sourcePoints.Count - 1]);
        }
        return resultPoints.ToArray();
    }

    float GetPathLength(List<Vector2> points) {
        float length = 0;
        for (int i = 1; i < points.Count; i++) {
            length += Vector2.Distance(points[i - 1], points[i]);
        }
        return length;
    }

    //Vector2 ComputeCentroid(List<Vector2> points) {
    //    Vector2 centroid = Vector2.zero;
    //    foreach (Vector2 point in points) {
    //        centroid += point;
    //    }
    //    return centroid / points.Count;
    //}

    //void DrawCentroid(Vector2 pos, Color color) {
    //    Debug.DrawRay(pos + (Vector2.left * 0.25f), Vector2.right * 0.5f, color, 2f);
    //    Debug.DrawRay(pos + (Vector2.down * 0.25f), Vector2.up * 0.5f, color, 2f);
    //}

    //// Move all the points so that the centroid is at the origin (0, 0)
    //void TranslateCentroidToOrigin(List<Vector2> points, Vector2 centroid) {
    //    for (int i = 0; i < points.Count; i++) {
    //        points[i] -= centroid;
    //    }
    //}
}
