using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody),typeof(NoiseMaker))]
public class CarController : MonoBehaviour {
    [Header("Controller info")]
    [SerializeField] float speed;
    [SerializeField] float acceleration = 1f;
    [SerializeField] Vector2 resistance = new Vector2(0.95f,0.95f);
    [SerializeField] Vector2 tilt = new Vector2(5, 5);
    [SerializeField] float turnForce = 1f;
    [SerializeField] List<Transform> turnWheels = new List<Transform>();
    [SerializeField] float wheelTurnAngle = 30;
    [SerializeField] List<ParticleSystem> burnoutParticles = new List<ParticleSystem>();
    [Header("Packages")]
    [SerializeField] Transform getOutPoint;
    [SerializeField] Transform packageEjectionPoint;
    [SerializeField] GameObject packagePrefab;
    [SerializeField] TMPro.TMP_Text packageCounter;
    [SerializeField] int maxPackageCount = 20;
    public List<Package> storedPackages = new List<Package>();
    Transform player;
    CinemachineVirtualCamera cvc;
    Follow minimapCamFollow;
    bool isDriving;
    NoiseMaker noiseMaker;
    Rigidbody rb;
    Vector2 inputDir;
    Vector2 inputVelocity;
    void Start() {
        rb = GetComponent<Rigidbody>();
        noiseMaker = GetComponent<NoiseMaker>();
        minimapCamFollow = GameObject.FindGameObjectWithTag("MinimapCam").GetComponent<Follow>();

        DisableParticles();
    }

    void DisableParticles() {
        foreach(ParticleSystem particleSystem in burnoutParticles) {
            particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    void EnableParticles() {
        foreach(ParticleSystem particleSystem in burnoutParticles) {
            particleSystem.Play(false);
        }
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


            if(Input.GetKeyUp(KeyCode.Space))
            inputVelocity = inputVelocity * 0.75f;
        }
    }

    void FixedUpdate() {

        if(isDriving) inputDir = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        else inputDir = Vector2.zero;
        
        inputVelocity.y += inputDir.y * Time.fixedDeltaTime * acceleration;
        inputVelocity.y = Mathf.Clamp(inputVelocity.y, -1f, 1f) * ((Mathf.Abs(inputDir.y)<0.1f)?(resistance.y):1f);
        
        if(Mathf.Abs(inputDir.x) > 0.01f) inputVelocity.x += inputDir.x * turnForce;
        else inputVelocity.x *= resistance.x;
        inputVelocity.x = Mathf.Clamp(inputVelocity.x, -1f, 1f);

        // transform.localRotation = Quaternion.Euler(Vector3.forward * inputVelocity.x);
        // Quaternion rotation = transform.localRotation;
        // rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, 0);
        // rotation *= Quaternion.Euler(Vector3.forward * inputVelocity.x * tilt.x);
        // transform.localRotation = rotation;

        transform.localRotation = Quaternion.Euler(new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0));

        if(Input.GetKey(KeyCode.Space)) {
            if(rb.velocity.magnitude > 12) EnableParticles();
            else DisableParticles();
            // inputVelocity.x *= resistance.x;
            transform.Rotate(transform.up * inputVelocity.x * turnForce * inputVelocity.y);
            // rb.velocity = transform.forward * inputVelocity.y * speed;
        }
        else {
            DisableParticles();
            transform.Rotate(transform.up * inputVelocity.x * turnForce * inputVelocity.y);
            rb.velocity = transform.forward * inputVelocity.y * speed;
        }

        transform.localRotation = Quaternion.Euler(new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, inputVelocity.x * tilt.x * inputVelocity.y));

        foreach(Transform wheel in turnWheels) {
            wheel.localRotation = Quaternion.Euler(transform.up * inputVelocity.x * wheelTurnAngle);
        }

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
}