using UnityEngine;

public class ZombiePerception : MonoBehaviour
{
    [Header("Detection Settings")]
    public float viewRadius = 10f;
    public float viewAngle = 90f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    
    private Transform target;
    private ZombieController zombieController;
    public SkinnedMeshRenderer zombieRenderer;

    void Start()
    {
        zombieController = GetComponent<ZombieController>();
        // Find the player - you might want to modify this based on your game's setup
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (zombieController == null || target == null) return;

        bool isSpotted = CheckTargetDetection();
        zombieController.OnSpottedByPlayer(isSpotted);
    }

    bool CheckTargetDetection()
    {
        // Check if target is within detection radius
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= viewRadius)
        {
            // Check if target is within view angle
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            if (angleToTarget <= viewAngle / 2)
            {
                // Check if there are obstacles between zombie and target
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    Debug.Log($"Zombie {gameObject.name} spotted target! Distance: {distanceToTarget}, Angle: {angleToTarget}");
                    return true;
                }
            }
        }
        return false;
    }

    void OnDrawGizmos()
    {
        // Visualize detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // Visualize view cone
        if (Application.isPlaying)
        {
            Vector3 viewAngleLeft = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewRadius;
            Vector3 viewAngleRight = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewRadius;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + viewAngleLeft);
            Gizmos.DrawLine(transform.position, transform.position + viewAngleRight);
            Gizmos.DrawLine(transform.position + viewAngleLeft, transform.position + viewAngleRight);
        }
    }
}