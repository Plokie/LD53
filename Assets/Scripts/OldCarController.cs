using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody),typeof(NoiseMaker))]
public class OldCarController : MonoBehaviour
{
    float horizontalInput;
    float verticalInput;
    float currentSteerAngle;
    float currentbreakForce;
    bool isBreaking;
    bool isDriving = false;
    Rigidbody rb;
    NoiseMaker noiseMaker;
    [SerializeField] float motorForce;
    [SerializeField] float breakForce;
    [SerializeField] float maxSteerAngle;
    [Header("")]
    [SerializeField] WheelCollider frontLeftWheelCollider;
    [SerializeField] WheelCollider frontRightWheelCollider;
    [SerializeField] WheelCollider rearLeftWheelCollider;
    [SerializeField] WheelCollider rearRightWheelCollider;
    [Header("")]
    [SerializeField] Transform frontLeftWheelTransform;
    [SerializeField] Transform frontRightWheeTransform;
    [SerializeField] Transform rearLeftWheelTransform;
    [SerializeField] Transform rearRightWheelTransform;
    [Header("")]
    [SerializeField] Transform getOutPoint;
    [SerializeField] Transform packageEjectionPoint;
    [SerializeField] GameObject packagePrefab;
    [SerializeField] TMPro.TMP_Text packageCounter;
    [SerializeField] int maxPackageCount = 20;
    public List<Package> storedPackages = new List<Package>();
    Transform player;
    CinemachineVirtualCamera cvc;
    Follow minimapCamFollow;
    void Start() {
        rb = GetComponent<Rigidbody>();
        noiseMaker = GetComponent<NoiseMaker>();
        minimapCamFollow = GameObject.FindGameObjectWithTag("MinimapCam").GetComponent<Follow>();
    }
    public void GetInVehicle(Transform player, CinemachineVirtualCamera cvc) {
        noiseMaker = GetComponent<NoiseMaker>();
        this.player = player;
        player.gameObject.SetActive(false);
        isDriving = true;

        this.cvc = cvc;
        cvc.LookAt = transform;
        cvc.Follow = transform;
        cvc.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = new Vector3(0, 10.5f, 0);

        minimapCamFollow.target = transform;

        noiseMaker.SetLoopingNoise(NoiseSound.defaultNoiseSoundType[NoiseSound.Type.HeavyEngine]);
    }
    
    public void ExitVehicle() {
        player.gameObject.SetActive(true);
        player.transform.position = getOutPoint.transform.position;

        cvc.LookAt = player;
        cvc.Follow = player;
        cvc.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = new Vector3(0, 2.5f, 0);

        minimapCamFollow.target = player;
        
        // player.GetComponent<PlayerController>().UpdateCamOffset();
        player.GetComponent<PlayerController>().interactionCooldown = 1f;
        noiseMaker.StopLooping();
        isDriving = false;
    }

    void Update() {
        if(isDriving) {
            Pointers.Instance.UpdatePointers(transform);
            if(Input.GetKeyDown(KeyCode.F)) {
                ExitVehicle();
            }

            if(Vector3.Dot(Vector3.up, transform.up) < 0.5) {
                packageCounter.text = "[R] to flip";
                if(Input.GetKeyDown(KeyCode.R)) {
                    transform.up = Vector3.up;
                    transform.position += Vector3.up;
                }
            }
            else {
                UpdateCounter();
            }

        }
    }

    void FixedUpdate()
    {
        rb.centerOfMass = new Vector3(0, 0.6f, 0);
        if(isDriving) {
            GetInput();
            HandleMotor();
            HandleSteering();
            UpdateWheels();
        }
        else {
            currentbreakForce = breakForce;
            ApplyBreaking();
        }
    }


    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentbreakForce = (isBreaking || (verticalInput < 0 && rb.velocity.z>2)) ? breakForce : 0f;
        ApplyBreaking();       
    }

    void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    void OnCollisionEnter(Collision collision) {
        // print(collision.relativeVelocity.magnitude);
        float velocity = collision.relativeVelocity.magnitude;
        

        if(velocity > 2) {
            // noiseMaker.MakeNoise(NoiseSound.Type.PackageFall);
            
            foreach(ContactPoint contact in collision.contacts)
            {
                if(contact.otherCollider.transform.parent && contact.otherCollider.transform.parent.parent)
                {
                    // print(contact.otherCollider.transform.root);
                    if(contact.otherCollider.transform.root.tag == "Zombie") {
                        Zombie zombie = contact.otherCollider.transform.parent.parent.GetComponent<Zombie>();
                        rb.velocity = -collision.relativeVelocity;
                        if(zombie && velocity > 3.5f) {
                            // zombie.Hurt(velocity * 0.05f, contact.point);
                            zombie.Hurt(velocity / 10f, contact.point, contact.normal);
                            
                            break;
                        }
                    }
                }
            }
        }
    }

    public void EjectPackage() {
        if(storedPackages.Count > 0) {
            Package ejectedPackage = storedPackages[0];
            storedPackages.RemoveAt(0);

            ThrownPackage newPackage = Instantiate(packagePrefab, packageEjectionPoint.position, Quaternion.identity).GetComponent<ThrownPackage>();
            newPackage.packageData = ejectedPackage;
            Rigidbody newPackageRb = newPackage.GetComponent<Rigidbody>();

            Vector3 dir = -transform.forward;
            float force = 5;
            float verticalForce = 3;

            newPackageRb.AddForce((dir * force) + (Vector3.up * verticalForce), ForceMode.Impulse);
            newPackageRb.AddRelativeTorque(Random.insideUnitSphere.normalized, ForceMode.Impulse);
            UpdateCounter();
        }
    }

    public bool AddPackage(Package package) {
        if(storedPackages.Count < maxPackageCount) {
            storedPackages.Add(package);
            UpdateCounter();
            return true;
        }
        return false;
    }

    void UpdateCounter() {
        if(storedPackages.Count == 0) packageCounter.text = "";
        else packageCounter.text = storedPackages.Count+"/"+maxPackageCount;
    }
}