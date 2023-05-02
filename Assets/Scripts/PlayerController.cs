using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

[RequireComponent(typeof(Rigidbody),typeof(NoiseMaker))]
public class PlayerController : MonoBehaviour
{
    enum WalkingType {
        Sneaking,
        Walking,
        Sprinting
    }
    Dictionary<WalkingType, float> movementSpeed = new Dictionary<WalkingType, float>()
    {
        {WalkingType.Sneaking, 3},
        {WalkingType.Walking, 6.5f},
        {WalkingType.Sprinting, 10},
    };
    Dictionary<WalkingType, float> footstepInterval = new Dictionary<WalkingType, float>()
    {
        {WalkingType.Sneaking, 0.5f},
        {WalkingType.Walking, 0.35f},
        {WalkingType.Sprinting, 0.25f},
    };
    Dictionary<WalkingType, NoiseSound.Type> walkingTypeNoise = new Dictionary<WalkingType, NoiseSound.Type>()
    {
        {WalkingType.Sneaking, NoiseSound.Type.SneakFootstep},
        {WalkingType.Walking, NoiseSound.Type.WalkFootstep},
        {WalkingType.Sprinting, NoiseSound.Type.RunFootstep},
    };
    [SerializeField] Camera cam;
    [SerializeField] Transform body;
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask levelMask;
    [SerializeField] float interactionDistance = 6f;
    [SerializeField] float testRadius = 6;
    [Header("")]
    [SerializeField] Vector3 viewPointIndoors;
    [SerializeField] Vector3 viewPointOutdoors;
    // bool indoors;
    [Header("")]
    [SerializeField] GameObject thrownPackagePrefab;
    [SerializeField] int maxPackageCount = 3;
    [Header("UI")]
    [SerializeField] TMPro.TMP_Text interactText;
    [SerializeField] PackageList packageListUI;
    [SerializeField] Slider sliderHp;
    public Slider sliderStamina;
    [SerializeField] Image hurtVignette;
    [SerializeField] GameObject deathScreen;
    [Range(0f,1f)] public float hp = 1;
    [Range(0f,1f)] public float stamina = 1;
    float staminaRecovery = 0;
    AudioSource audioSource;
    float lastFootstepTime;
    CinemachineVirtualCamera cvc;
    NoiseMaker noiseMaker;
    Rigidbody rb;
    Vector2 inputDir;
    Vector3 lookDir;
    Vector3 targetPos;
    WalkingType walkingType;
    Transform target;
    public List<Package> packages = new List<Package>();
    [HideInInspector] public float interactionCooldown = 0;
    void Start() {
        rb = GetComponent<Rigidbody>();
        noiseMaker = GetComponent<NoiseMaker>();
        cvc = cam.GetComponent<CinemachineVirtualCamera>();
    }

    bool TargetWithinInteractionDistance() {
        return Vector3.Distance(transform.position, targetPos) < interactionDistance || Vector3.Distance(transform.position, target.position) < interactionDistance;
    }
    void Update() {
        inputDir = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask, QueryTriggerInteraction.Ignore)) {
            Vector3 from = new Vector3(body.position.x, 0, body.position.z);
            Vector3 to = new Vector3(hit.point.x, 0, hit.point.z);
            lookDir = (to - from).normalized;
            body.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, levelMask, QueryTriggerInteraction.Ignore)) {
            targetPos = hit.point;
            target = hit.transform.root;

            if(target.GetComponent<Interactable>() && TargetWithinInteractionDistance())
            interactText.text = "[F] " + target.name;
            else
            interactText.text = "";
        }


        if(Input.GetKeyDown(KeyCode.Mouse0)) {
            if(packages.Count > 0) {
                Package packageToThrow = packages[0];
                packages.RemoveAt(0);
                packageListUI.DrawPackages(packages);
                ThrowPackage(packageToThrow, lookDir, targetPos);
            }
        }

        if(Input.GetKeyDown(KeyCode.Mouse1)) {
            if(TargetWithinInteractionDistance())
            if(target.GetComponent<CarController>()) {
                CarController carController = target.GetComponent<CarController>();
                carController.EjectPackage();
            }
        }

        if(Input.GetKeyDown(KeyCode.Space)) {
            noiseMaker.MakeNoise(new NoiseSound(NoiseSound.Type.None, testRadius, 1));
        }


        if(Input.GetKey(KeyCode.F) && interactionCooldown<=0) {
            if(TargetWithinInteractionDistance())
            if(target.GetComponent<CarController>()) {
                CarController carController = target.GetComponent<CarController>();
                interactText.text = "";
                carController.GetInVehicle(transform, cvc);
            }
        }

        if(inputDir.magnitude > 0.1) {
            if(Time.time > lastFootstepTime + footstepInterval[walkingType]) {
                lastFootstepTime = Time.time;
                Footstep();
            }
        }

        Pointers.Instance.UpdatePointers(transform);
        UpdateCamOffset();

        if(interactionCooldown>0) interactionCooldown-=Time.deltaTime;
    }
    void Footstep() {
        noiseMaker.MakeNoise(NoiseSound.defaultNoiseSoundType[walkingTypeNoise[walkingType]]);
    }
    void FixedUpdate() {
        if(Input.GetKey(KeyCode.LeftShift) && stamina>0f) walkingType = WalkingType.Sprinting;
        else if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Tab)) walkingType = WalkingType.Sneaking;
        else walkingType = WalkingType.Walking;

        float speed = movementSpeed[walkingType];

        rb.position = new Vector3(rb.position.x, 0, rb.position.z);
        rb.velocity = new Vector3(inputDir.x, 0, inputDir.y) * speed;
        // rb.MovePosition(new Vector3(rb.position.x, 0.03f, rb.position.z));
        if(rb.velocity.magnitude > 0.1 && walkingType == WalkingType.Sprinting) {
            stamina -= 0.004f;
            staminaRecovery += 0.01f;
            staminaRecovery = Mathf.Min(staminaRecovery, 1f);
        }   
        else {
            stamina += Mathf.Max(0, (0.0085f * ((rb.velocity.magnitude > 0.1)?0.5f:1f)) - staminaRecovery);
            staminaRecovery -= 0.01f * ((rb.velocity.magnitude > 0.1)?0.5f:1f);
            staminaRecovery = Mathf.Max(staminaRecovery, 0);
            stamina = Mathf.Clamp(stamina, 0f, 1f);
        }

        sliderStamina.value = stamina;
        sliderHp.value = hp;
    }

    void ThrowPackage(Package package, Vector3 dir, Vector3 targetPoint, float force = 10) {
        Vector3 spawnPos = transform.position + Vector3.up + (dir * 1.75f);
        float verticalForce = Vector3.Distance(spawnPos, targetPoint) / 2;

        Rigidbody newPackageRb = Instantiate(thrownPackagePrefab, spawnPos, Quaternion.identity).GetComponent<Rigidbody>();
        newPackageRb.AddForce((dir * force) + (Vector3.up * verticalForce), ForceMode.Impulse);
        // newPackageRb.AddRelativeTorque(dir, ForceMode.Impulse);
        newPackageRb.AddRelativeTorque(Random.insideUnitSphere.normalized, ForceMode.Impulse);

        newPackageRb.GetComponent<ThrownPackage>().packageData = package;
    }

    public bool AddPackage(Package package) {
        if(packages.Count < maxPackageCount) {
            packages.Add(package);
            packageListUI.DrawPackages(packages);
            return true;
        }
        return false;
    }
    public void UpdateCamOffset() {
        // cvc.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = viewPointIndoors;
        // cvc.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = viewPointOutdoors;
        // if(indoors) {
        //     cvc.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = viewPointIndoors;
        // }
        // else {
        //     cvc.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = viewPointOutdoors;
        // }
    }

    public void Damage(float damage = 0.1f) {
        hp -= damage;

        hurtVignette.GetComponent<Animator>().SetTrigger("Flash");

        if(hp < 0) {
            deathScreen.SetActive(true);
            TMPro.TMP_Text[] texts = deathScreen.GetComponentsInChildren<TMPro.TMP_Text>();
            texts[0].text = "<b>You Died!</b>\n\nYou delivered "+GameData.Instance.deliveredPackages+" packages";


            Destroy(gameObject);
        }
    }

    public void Heal(float health = 0.1f) {
        hp += health;

        if(hp > 1f) hp = 1f;
    }

    void OnTriggerEnter(Collider collider) {
        // print("Enter "+collider.name);
        // if(collider.name == "IndoorTrigger") indoors = true;
    }

    void OnTriggerExit(Collider collider) {
        // print("Exit "+collider.name);
        // if(collider.name == "IndoorTrigger") indoors = false;
    }
}
