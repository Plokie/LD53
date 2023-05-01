using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Follow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public RigidbodyConstraints test;
    void Update() {
        if(target)
        transform.position = target.position + offset;
    }
}
