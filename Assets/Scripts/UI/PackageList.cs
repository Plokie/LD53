using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PackageList : MonoBehaviour
{
    [SerializeField] GameObject packageElementPrefab;
    List<GameObject> instantiatedGameObjects = new List<GameObject>();
    public void DrawPackages(List<Package> packages) {
        foreach(GameObject obj in instantiatedGameObjects) Destroy(obj);
        instantiatedGameObjects.Clear();

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>();
        texts[0].text = packages.Count + "/5";

        foreach(Package package in packages) {
            GameObject packageElement = Instantiate(packageElementPrefab, Vector3.zero, Quaternion.identity, transform);
            instantiatedGameObjects.Add(packageElement);
        }
    }
}
