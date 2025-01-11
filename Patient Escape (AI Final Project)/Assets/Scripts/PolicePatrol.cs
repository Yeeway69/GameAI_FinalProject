using UnityEngine;
using UnityEngine.AI;

public class PolicePatrol : MonoBehaviour
{
    public Transform[] waypoints;         // Array of waypoints
    private NavMeshAgent agent;           // Reference to the NavMeshAgent
    private int currentWaypointIndex;     // Current waypoint index

    void Start()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();

        // Check if waypoints are assigned
        if (waypoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned to the PolicePatrol script.");
            enabled = false;
            return;
        }

        // Set initial random waypoint
        SetRandomWaypoint();
    }

    void Update()
    {
        // Check if we've reached the waypoint
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetRandomWaypoint();
        }
    }

    void SetRandomWaypoint()
    {
        // Choose a random waypoint that's different from the current one
        int newWaypointIndex;
        do
        {
            newWaypointIndex = Random.Range(0, waypoints.Length);
        } while (waypoints.Length > 1 && newWaypointIndex == currentWaypointIndex);

        currentWaypointIndex = newWaypointIndex;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }
}
