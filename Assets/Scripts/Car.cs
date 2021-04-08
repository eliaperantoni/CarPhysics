using UnityEngine;

public class Car : MonoBehaviour {
    public Wheel wheelFl;
    public Wheel wheelFr;

    [Header("Car Specs")]
    // Distance between front and rear axle
    public float wheelBase;
    // Distance between the wheels on an axle
    public float rearTrack;
    // Radius of the smallest turning circle the car is able to make
    public float turnRadius;

    private void Update() {
        var steerInput = Input.GetAxis("Horizontal");

        // Ackermann steering is used so that the wheels have the same turning circle (by having different angles)
        if (steerInput > 0) {
            wheelFl.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack / 2)) * steerInput;
            wheelFr.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack / 2)) * steerInput;
        } else if (steerInput < 0) {
            wheelFl.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack / 2)) * steerInput;
            wheelFr.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack / 2)) * steerInput;
        } else {
            wheelFl.steerAngle = 0;
            wheelFr.steerAngle = 0;
        }
    }
}