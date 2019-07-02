using UnityEngine;

public class TrailRendererTest : MonoBehaviour {

    Camera mainCamera;
    TrailRenderer trail;

    void Start() {
        mainCamera = Camera.main;
        trail = GetComponent<TrailRenderer>();
    }

    void Update() {
        if (Input.GetMouseButton(0)) {
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            transform.position = mousePosition;

            if (Input.GetMouseButtonDown(0)) {
                trail.Clear();
            }

        }
    }
}
