using UnityEngine;

public class PathCreator : MonoBehaviour {

    public bool showTangents = true;

    [HideInInspector] public Path path;

    public void CreatePath() {
        path = new Path(transform.position);
    }
}
