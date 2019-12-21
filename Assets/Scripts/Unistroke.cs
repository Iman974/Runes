using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Unistroke {

    public const int kEvenPointCount = 32;

    [SerializeField] List<Vector2> rawPoints = new List<Vector2>();
    public List<Vector2> RawPoints => rawPoints;

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
                // If vector's length is smaller than interval.
                // The variable remainingDistance is always smaller than spacing
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
}
