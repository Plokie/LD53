using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Camera cam;

    void Update() {
        if(!cam) cam = Camera.main;
        if(!cam) return;

        transform.LookAt(cam.transform.position, Vector3.up);
    }
}
