using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medpak : MonoBehaviour
{
    [SerializeField] float healRadius = 3f;
    [SerializeField] LayerMask levelMask;
    [SerializeField] Ring ringRadius;
    PlayerController playerController;
    void Start() {
        playerController = TryGetPlayerController();
    }

    void Update() {
        if(ringRadius) {
            ringRadius.radius = healRadius;
            ringRadius.draw = true;
        }

        if(playerController) {
            // Vector3 dir = (playerController.transform.position - transform.position).normalized;
            Vector3 from = transform.position;
            from.y = 0.1f;
            float dist = Vector3.Distance(from, playerController.transform.position);

            if(dist < healRadius) {
                playerController.Heal(0.1f * Time.deltaTime);
                // if(Physics.Raycast(from, dir, out hit, healRadius, levelMask, QueryTriggerInteraction.Ignore)) {
                //     print(hit.point);
                // }
                // else {
                //     print("Heal");
                // }
            }
        }
        else {
            playerController = TryGetPlayerController();
        }
    }

    PlayerController TryGetPlayerController() {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if(playerObject) {
            return playerObject.GetComponent<PlayerController>();
        }
        return null;
    }
}
