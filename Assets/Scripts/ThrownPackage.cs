using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody),typeof(NoiseMaker))]
public class ThrownPackage : MonoBehaviour
{
    [SerializeField] LayerMask levelMask;
    [SerializeField] AudioClip pickupClip;
    public Package packageData;
    NoiseMaker noiseMaker;
    Rigidbody rb;
    void Start() {
        noiseMaker = GetComponent<NoiseMaker>();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 1f, levelMask, QueryTriggerInteraction.Ignore)) {
            if(hit.collider.tag == "Conveyor") {
                // rb.AddForce(Vector3.back * 10, ForceMode.Force);
                rb.MovePosition(rb.position + (hit.collider.transform.forward * 0.2f));
            }
        }
    }

    void OnTriggerEnter(Collider collider) {
        if(collider.tag == "Door") {
            // print("Put through door");
            Door door = collider.GetComponent<Door>();
            if(door.deliveriesNeeded > 0) {
                door.SubDelivery();
                Destroy(gameObject);
            }
        }
        else if(collider.transform.root.tag == "Vehicle") {
            // print("Store in vehicle");
            CarController carController = collider.transform.root.GetComponent<CarController>();
            if(carController.AddPackage(packageData)) Destroy(gameObject);
        }
        else if(collider.transform.root.tag == "Player") {
            PlayerController player = collider.transform.root.GetComponent<PlayerController>();
            
            if(packageData == null) {
                packageData = new Package();
            }

            if(player.AddPackage(packageData)) {
                // print("Player pickup");
                GameObject pickupAudioObj = new GameObject("pickup audio");
                pickupAudioObj.transform.position = transform.position;
                AudioSource pickupAudio = pickupAudioObj.AddComponent<AudioSource>();
                pickupAudio.clip = pickupClip;
                pickupAudio.dopplerLevel = 0f;
                pickupAudio.spatialBlend = 1f;
                pickupAudio.Play();
                Destroy(pickupAudioObj, 1f);


                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        // print(collision.relativeVelocity.magnitude);
        float velocity = collision.relativeVelocity.magnitude;

        if(velocity > 2.5) {
            noiseMaker.MakeNoise(NoiseSound.Type.PackageFall);
            
            foreach(ContactPoint contact in collision.contacts)
            {
                if(contact.otherCollider.transform.parent && contact.otherCollider.transform.parent.parent)
                {
                    // print(contact.otherCollider.transform.root);
                    if(contact.otherCollider.transform.root.tag == "Zombie") {
                        Zombie zombie = contact.otherCollider.transform.parent.parent.GetComponent<Zombie>();
                        if(zombie) {
                            // zombie.Hurt(velocity * 0.05f, contact.point);
                            zombie.Hurt(0.4f, contact.point, contact.normal);
                        }
                    }
                }
            }
        }
    }
}
