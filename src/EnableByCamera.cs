using UnityEngine;

public class EnableByCamera : MonoBehaviour
{
    Transform cam;
    Vector2 cameraSize;

    // Enables enemies even when they're a little bit off camera
    public Vector2 inflationOfLoadBoundary;
    
    bool isOn;
    public bool IsOn { get { return isOn; }}

    private void Start() {
        cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        cameraSize = cam.GetComponent<Camera>().ViewportSize();
        
        if (IsInside())
            isOn = true;
    }

    private void Update() {
        if (!isOn && IsInside())
            isOn = true;
    }

    public bool IsInside() {
        Rect cameraView = VectorExtensions.RectFromCenter(cam.position, cameraSize + inflationOfLoadBoundary);
        return cameraView.Contains(transform.position);
    }
}
