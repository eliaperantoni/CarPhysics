using UnityEngine;

public class Wheel : MonoBehaviour {
    [Header("Spring")]
    // Length of spring at rest (carrying the weight of the car that's no moving)
    public float springRestLength = 0.45f;
    // Travel length of spring when completely compressed or released
    public float springTravel = 0.1f;
    // How hard the spring is to compress
    public float springStiffness = 40000f;
    
    [Header("Damper")]
    // How hard the damper is to release
    public float damperStiffness = 4000f;
    
    [Header("Wheel")]
    public float wheelRadius = 0.285f;
    // Lateral friction of the wheel. The higher the value the less the slipping.
    public float wheelFriction = 2f;
    // Does the wheel drive the car forward? i.e. does it apply torque?
    public bool wheelDriving;
    
    [Header("Steer")]
    // How long does it take to turn the wheel?
    public float steerTime = 6f;

    [Header("Engine")]
    public float enginePower = 1f;

    [Header("Brakes")]
    public float brakesPower = 2f;
    
    private float _prevSpringLength;
    
    private float SpringMinLength => springRestLength - springTravel;
    private float SpringMaxLength => springRestLength + springTravel;

    // Steering angle is pretty much the target wheel angle. This is updated by the Car's script
    [HideInInspector]
    public float steerAngle;
    // Current wheel angle
    private float _wheelAngle;
    
    private Rigidbody _rb;
    private Transform _wheelMesh;
    
    void Start() {
        // Call expensive functions and store the result
        _rb = transform.root.GetComponent<Rigidbody>();
        _wheelMesh = transform.GetChild(0);
        
        // In the beginning there was no motion and the spring was resting. So the previous spring length should equal
        // the resting spring length.
        _prevSpringLength = springRestLength;
    }

    private float _verticalInput;
    
    void Update() {
        // Slowly move _wheelAngle towards steerAngle which gets set by the Car's script
        _wheelAngle = Mathf.Lerp(_wheelAngle, steerAngle, steerTime * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * _wheelAngle);

        // It's better to get the user input in Update rather than FixedUpdate
        _verticalInput = Input.GetAxis("Vertical");
        
        UpdateWheelMeshRotation();
    }

    // So that we can update wheel rotation in Update rather than FixedUpdate
    private float _groundSpeed;

    void FixedUpdate() {
        // Cast a ray from the top of the wheel rim downwards
        var isContact = Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, SpringMaxLength + wheelRadius);
        if (!isContact) return;

        var springLength = hit.distance - wheelRadius;
        springLength = Mathf.Clamp(springLength, SpringMinLength, SpringMaxLength);
        
        UpdateWheelMeshPosition(springLength);

        // Suspension is spring + damper. We keep it as a float for now because we'll need it later to see how hard
        // the wheels are pressing on the ground.
        var suspensionForce = 0f;
        
        // Hooke's law: F = -k * x
        var springForce = springStiffness * (springRestLength - springLength);
        suspensionForce += springForce;

        var springVelocity = (_prevSpringLength - springLength) / Time.fixedDeltaTime;
        var damperForce = damperStiffness * springVelocity;
        suspensionForce += damperForce;
        
        var force = suspensionForce * transform.up;

        // How the ground is moving relative to the wheel
        var groundVelocity = transform.InverseTransformDirection(_rb.GetPointVelocity(hit.point));
        _groundSpeed = groundVelocity.z;

        var isBraking = _verticalInput * groundVelocity.z < 0;

        if (isBraking) {
            force += _verticalInput * suspensionForce * transform.forward * brakesPower;
        } else if (wheelDriving) {
            // Moves the car forward. The harder the wheel is pressing on the ground, the higher the traction
            force += _verticalInput * suspensionForce * transform.forward * enginePower;
        }
        
        force += groundVelocity.x * suspensionForce * -transform.right * wheelFriction;

        _rb.AddForceAtPosition(force, hit.point);
        
        _prevSpringLength = springLength;
    }

    private void UpdateWheelMeshPosition(float springLength) {
        var wheelMeshPos = _wheelMesh.transform.localPosition;
        wheelMeshPos.y = -springLength;
        _wheelMesh.transform.localPosition = wheelMeshPos;
    }
    
    private void UpdateWheelMeshRotation() {
        _wheelMesh.transform.Rotate(Vector3.right * Mathf.Rad2Deg * _groundSpeed * Time.deltaTime / wheelRadius, Space.Self);
    }
}