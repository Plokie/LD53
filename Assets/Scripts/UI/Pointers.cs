using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointers : MonoBehaviour
{
    [SerializeField] GameObject pointerPrefab;
    List<GameObject> instantiatedPointers = new List<GameObject>();
    public static Pointers Instance { get; private set; }
    void Awake() 
    { 
        if (Instance != null && Instance != this) Destroy(this); 
        else Instance = this;
    }
    public void UpdatePointers(Transform playerPos) {
        if(instantiatedPointers.Count != GameData.Instance.targetDoors.Count) {
            foreach(GameObject pointer in instantiatedPointers) if(pointer)Destroy(pointer);
            instantiatedPointers.Clear();

            foreach(Door door in GameData.Instance.targetDoors) {
                GameObject newPointer = Instantiate(pointerPrefab, Vector3.zero, Quaternion.identity);
                newPointer.transform.SetParent(transform);
                newPointer.transform.localPosition = Vector3.zero;

                Vector3 dir = (door.transform.position - playerPos.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);

                newPointer.transform.rotation = rotation;
                instantiatedPointers.Add(newPointer);
            }
        }
        else {
            for (int i = 0; i < GameData.Instance.targetDoors.ToArray().Length; i++)
            {
                Door door = GameData.Instance.targetDoors[i];
                GameObject pointer = instantiatedPointers[i];

                if (door.deliveriesNeeded == 0)
                {
                    GameData.Instance.CompleteDoor(door);
                    Destroy(pointer);
                    continue;
                }

                Vector3 dir = (door.transform.position - playerPos.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);
                

                pointer.transform.rotation = Quaternion.Euler(0,0,-rotation.eulerAngles.y);
            }
        }
        
    }
}
