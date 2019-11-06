using UnityEngine;

public class InputUtility : MonoBehaviour {

    public static Vector2 MouseDelta => (Vector2)Input.mousePosition - LastMousePosition;

    public static Vector2 LastMousePosition { get; private set; }

    static InputUtility instance;

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
    }

    void LateUpdate() {
        LastMousePosition = Input.mousePosition;
    }
}
