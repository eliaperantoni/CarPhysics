using UnityEngine;

public class Camera : MonoBehaviour {
    public float cameraSpeed = 1f;
    
    public Rigidbody carRigidbody;
    
    public Transform cameraSlot;
    public Transform cameraTarget;

    void Update() {
        transform.position = Vector3.Lerp(transform.position, cameraSlot.position, Time.deltaTime * (carRigidbody.velocity.magnitude + 0.1f) * cameraSpeed);
        transform.LookAt(cameraTarget);
    }
}
