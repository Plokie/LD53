using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{
    [SerializeField] GameObject bloodSplatPrefab;
    public float health = 1f;
    public Transform target;
    [SerializeField] LayerMask levelMask;
    [SerializeField] LayerMask obstacleMask;
    public float viewFrustrumSize = 0.8f;
    public float immediateDetectionRadius = 2;
    public float viewRadius = 30;
    public float reachDistance = 2f;
    NavMeshAgent agent;
    Vector3 lastKnownTargetPosition = Vector3.forward;
    public bool activated = false;
    [SerializeField] bool drawView;
    [SerializeField] int drawViewResolution = 100;
    [SerializeField] MeshFilter viewConeMeshFilter;
    Mesh viewConeMesh;
    float lastWanderTime = float.MinValue;
    float idleTime = 20;
    float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        lastKnownTargetPosition = transform.position;

        viewConeMesh = new Mesh {
            name = "View Cone Mesh"
        };
    }

    void Update() {
        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
        if(target == null) {
            if(playerGameObject) {
                target = playerGameObject.transform;
            }
        }

        if(agent.isOnNavMesh && target) {
            if(target.tag != "Player" && playerGameObject) {
                if(IsTransformInView(playerGameObject.transform)) {
                    target = playerGameObject.transform;
                }
            }
            
            if(IsTargetInView(true))
            {
                activated = true;
                agent.SetDestination(target.position);
                float dist = Vector3.Distance(transform.position, target.position);

                if(dist < reachDistance) {
                    if(target.tag == "Player" && target.gameObject.activeSelf) {
                        if(Time.time > lastAttackTime + 1) {
                            target.GetComponent<PlayerController>().Damage();
                            lastAttackTime = Time.time;
                        }
                    }
                    else {
                        target = null;
                    }
                }
            }
            else if(activated){
                agent.SetDestination(lastKnownTargetPosition);
            }
        }
        else if(lastKnownTargetPosition != Vector3.zero && activated){
            agent.SetDestination(lastKnownTargetPosition);
        }

        if(!activated ) {
            if(Time.time > lastWanderTime+idleTime) {
                lastWanderTime = Time.time;
                idleTime = Random.Range(50f, 80f);
                Vector3 dir = (Random.insideUnitCircle).normalized;
                dir.z = dir.y; dir.y = 0;

                float distance = Random.Range(7f, 13f);

                if(!Physics.Raycast(transform.position + Vector3.up, dir, distance, obstacleMask, QueryTriggerInteraction.Ignore)) {
                    agent.SetDestination(transform.position + (dir*distance));
                }
            }
        }

        if(drawView || (playerGameObject && !(target == playerGameObject.transform && IsTargetInView()) && Vector3.Distance(playerGameObject.transform.position, transform.position) < (viewRadius * 1.5f))) {
            // Gizmos.color = new Color(1, 1, 1, 0.1f);

            List<Vector3> vertices = new List<Vector3>();
            List<int> indicies = new List<int>();

            Vector3 from = (transform.position + Vector3.up) + (transform.forward * 0.1f);
            
            vertices.Add((transform.forward * 0.1f));

            for (int i = 0; i < drawViewResolution; i++)
            {
                float angle = ((i / (float)drawViewResolution) - 0.5f) * 180;

                Vector3 direction = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), 0, Mathf.Cos(Mathf.Deg2Rad * angle));
                direction = transform.TransformDirection(direction);
                direction.y = 0;

                if (Vector3.Dot(direction, transform.forward) > viewFrustrumSize)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(from, direction, out hit, viewRadius, levelMask, QueryTriggerInteraction.Ignore))
                    {
                        // Gizmos.DrawRay(transform.position + Vector3.up, direction * (Vector3.Distance(transform.position + Vector3.up, hit.point)));
                        // Debug.DrawRay(transform.position + Vector3.up, direction * (Vector3.Distance(transform.position + Vector3.up, hit.point)), new Color(1,1,1,0.1f), Time.deltaTime);
                        vertices.Add((hit.point - transform.position) - Vector3.up);
                    }
                    else {
                        // Gizmos.DrawRay(transform.position + Vector3.up, direction * viewRadius);
                        // Debug.DrawRay(transform.position + Vector3.up, direction * viewRadius, new Color(1,0,0,0.1f), Time.deltaTime);
                        vertices.Add((direction * viewRadius));
                    }

                }
            }

            viewConeMesh.vertices = vertices.ToArray();

            for (int i = 0; i < vertices.Count-1; i++) {
                indicies.Add(0);
                indicies.Add(i);
                indicies.Add(i+1);
            }
            viewConeMesh.triangles = indicies.ToArray();


            viewConeMeshFilter.mesh.Clear();
            viewConeMeshFilter.mesh = viewConeMesh;
            viewConeMeshFilter.transform.rotation = Quaternion.identity;

        }
        else {
            viewConeMeshFilter.mesh.Clear();
        }
    }
    void LateUpdate() {
        if(agent.velocity.sqrMagnitude > Mathf.Epsilon)
        transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);
    }

    public void Hurt(float damage, Vector3? hurtPoint=null, Vector3? hurtNormal=null) {
        health -= damage;
        if(hurtPoint is Vector3 point && hurtPoint.HasValue && hurtNormal is Vector3 normal && hurtNormal.HasValue) {
            // print(damage + "d Hit point : " + point);

            Destroy(Instantiate(bloodSplatPrefab, point, Quaternion.LookRotation(normal, Vector3.up)), 15f);
        }

        if(health <= 0) {
            Destroy(Instantiate(bloodSplatPrefab, transform.position, Quaternion.LookRotation(Vector3.up, Vector3.up)), 15f);
            Destroy(Instantiate(bloodSplatPrefab, transform.position, Quaternion.LookRotation(Vector3.up, Vector3.up)), 15f);
            Destroy(gameObject);
        }
    }

    public void SetTarget(Transform newTarget, bool forceSetTarget=false) {
        if(target == null || forceSetTarget || !IsTargetInView()) {
            target = newTarget;
            lastKnownTargetPosition = newTarget.position;
        }
        activated = true;
    }

    bool IsTargetInView(bool updateLastKnownPosition = false) {
        float distance = Vector3.Distance(transform.position, target.position);
        if(distance < immediateDetectionRadius) {
            lastKnownTargetPosition = target.position;
            return true;
        }
        if(distance > viewRadius) return false;

        Vector3 dir = (target.position - transform.position).normalized;
        if (Physics.Raycast(transform.position + Vector3.up, dir, distance, levelMask, QueryTriggerInteraction.Ignore)) return false; //Target is behind cover
        else {
            Vector3 zombieLookDir = transform.forward;
            if(Vector3.Dot(zombieLookDir, dir) > viewFrustrumSize) { //Target is in view frustrum
                lastKnownTargetPosition = target.position;
                return true;
            }
            return false;
        }
    }

    bool IsTransformInView(Transform t) {
        float distance = Vector3.Distance(transform.position, t.position);
        if(distance < immediateDetectionRadius) return true;
        if(distance > viewRadius) return false;
        
        Vector3 dir = (target.position - t.position).normalized;
        if (Physics.Raycast(t.position + Vector3.up, dir, distance, levelMask, QueryTriggerInteraction.Ignore)) return false; //Target is behind cover
        else {
            Vector3 zombieLookDir = transform.forward;
            if(Vector3.Dot(zombieLookDir, dir) > viewFrustrumSize) return true;
            return false;
        }
    }

    void OnDrawGizmos() {

        
        // if(drawView) {
        //     Gizmos.color = new Color(1, 1, 1, 0.1f);
        //     for (int i = 0; i < drawViewResolution; i++) {
        //         float angle = ((i / (float)drawViewResolution)-0.5f) * 180;
        //         Vector3 direction = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), 0, Mathf.Cos(Mathf.Deg2Rad * angle));
        //         direction = transform.TransformDirection(direction);

        //         if(Vector3.Dot(direction, transform.forward) > viewFrustrumSize) {
        //             RaycastHit hit;
        //             if(Physics.Raycast(transform.position + Vector3.up, direction, out hit, viewRadius, levelMask, QueryTriggerInteraction.Ignore)) {
        //                 Gizmos.DrawRay(transform.position + Vector3.up, direction * (Vector3.Distance(transform.position + Vector3.up, hit.point)));
        //             }
        //             else {
        //                 Gizmos.DrawRay(transform.position + Vector3.up, direction * viewRadius);
        //             }


        //         }
        //     }
        // }
        // Gizmos.DrawRay()
    }
}
