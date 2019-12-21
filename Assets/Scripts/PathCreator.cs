using UnityEngine;

public class PathCreator : MonoBehaviour {

    public bool showTangents = true;

    [HideInInspector] [SerializeField] Path path;
    public Path Path => path;

    public void CreatePath() {
        path = new Path(transform.position);
    }

    void Reset() {
        CreatePath();
    }
}
