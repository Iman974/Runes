using UnityEngine;

public class InputUtility : MonoBehaviour {

    public static Vector2 MouseDelta => (Vector2)Input.mousePosition - LastMousePosition;
    public static Vector2 LastMousePosition { get; private set; }
    public static Vector2 WorldMousePosition {
        get { return mainCamera.ScreenToWorldPoint(Input.mousePosition); }
    }

    static InputUtility instance;
    static Camera mainCamera;

    void Awake() {
        #region Singleton
        if (instance == null) {
            instance = this;
        } else {
            Destroy(this);
            return;
        }
        #endregion
    }

    void Start() {
        LastMousePosition = Input.mousePosition;
        mainCamera = Camera.main;
        UnityEngine.Assertions.Assert.IsNotNull(mainCamera, "Could not find camera.");
    }

    void LateUpdate() {
        LastMousePosition = Input.mousePosition;
    }
}
