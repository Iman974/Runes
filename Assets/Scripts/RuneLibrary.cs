using UnityEngine;

public class RuneLibrary : MonoBehaviour {

    [System.Serializable]
    public struct RuneSegment {
        [SerializeField] [Range(-179.99f, 180f)] float angle;
        [SerializeField] float length;

        public Vector3 Vector {
            get {
                float radians = angle * Mathf.Deg2Rad;
                return new Vector3(Mathf.Cos(radians), Mathf.Sin(radians)) * length;
            }
        }
    }

    [SerializeField] RuneSegment[] segments = null;

    public RuneSegment this[int index] => segments[index];
    public int Length => segments.Length;

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;

        Vector3 currentPosition = Vector3.zero;
        for (int i = 0; i < segments.Length; i++) {
            Gizmos.DrawRay(currentPosition, segments[i].Vector);
            currentPosition += segments[i].Vector;
        }
    }
}
