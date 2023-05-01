using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public enum Type {
        Package,
        Zombie
    }
    [SerializeField] Type type;
    [SerializeField] GameObject prefab;
    [SerializeField] float maxDistanceToPlayer = 0;
    [SerializeField] float minDistanceToPlayer = 0;
    [SerializeField] int maxNumOfAliveSpawnees = 20;
    [Range(0f,1f)] [SerializeField] float spawnChance = 1;
    [SerializeField] bool spawnInIntervals;
    [HideIf("spawnInIntervals",false)]
    [SerializeField] float timeBetweenSpawns = 10f;
    [SerializeField] bool spawnOnStart = false;
    [HideIf("spawnOnStart", false)]
    [SerializeField] int spawnOnStartAmount = 1;
    [HideIf("spawnOnStart", false)]
    [SerializeField] float timeBetweenStartSpawns = 1f;
    float lastSpawnTime;
    List<GameObject> spawnees = new List<GameObject>();

    void Start() {
        if(spawnOnStart) {
            StartCoroutine(SpawnStart());
        }
    }
    IEnumerator SpawnStart() {
        for (int i = 0; i < spawnOnStartAmount; i++) {
            if(Random.Range(0f,1f)<=spawnChance)
            Spawn();
            yield return new WaitForSecondsRealtime(timeBetweenStartSpawns);
        }
    }

    void Update() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if(Time.time > lastSpawnTime + timeBetweenSpawns && timeBetweenSpawns != 0 && spawnInIntervals) {
            lastSpawnTime = Time.time;
            if(minDistanceToPlayer==0 && Random.Range(0f,1f)<=spawnChance && spawnees.Count < maxNumOfAliveSpawnees) {
                Spawn();
            }
            else if(player){
                float dist = Vector3.Distance(player.transform.position, transform.position);
                if(dist < minDistanceToPlayer && dist > maxDistanceToPlayer && Random.Range(0f,1f)<=spawnChance && spawnees.Count < maxNumOfAliveSpawnees) {
                    Spawn();
                }
            }
        }

        foreach(GameObject spawnee in spawnees.ToArray()) {
            if(!spawnee) spawnees.Remove(spawnee);
        }
    }

    [ContextMenu("Spawn")]
    public void Spawn() {
        spawnees.Add(Instantiate(prefab, transform.position, Quaternion.identity));

        if(type == Type.Package) GameData.Instance.packagesInExistence++;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 1f);
    }
}
