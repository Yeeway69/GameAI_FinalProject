using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    // Speed of the individual flock (boid/ghost)
    float speed;
    
    // Boolean flag to check if the ghost needs to turn (when hitting boundaries)
    bool turning = false;

    public float minSpeed = 1.5f;
    public float maxSpeed = 3f;
    public float neighbourDistance = 3f;
    public float avoidanceDistance = 1f;
    public float collisionAvoidanceDistance = 2f;
    public LayerMask obstacleLayer;


    // Start is called before the first frame update
    // Initializes the ghost with a random speed within the bounds set by FlockManager
    void Start()
    {
        // Set initial speed randomly between minimum and maximum speed set in the FlockManager
        speed = Random.Range(FlockManager.FM.minSpeed, FlockManager.FM.maxSpeed);
    }

    // Update is called once per frame
    // Handles ghost movement, boundary checks, and rule application (cohesion, separation, alignment)
    void Update()
    {
        if (FlockManager.FM == null || FlockManager.FM.leaderGhost == null) return;

        // Define the flying area boundary (based on the flying set in FlockManager)
        Bounds b = new Bounds(FlockManager.FM.transform.position, FlockManager.FM.flyLimits * 2);

        Vector3 leaderDirection = FlockManager.FM.leaderGhost.transform.position - transform.position;
        float distanceToLeader = leaderDirection.magnitude;

        // If the ghost is outside the boundary, set turning to true to make it turn back
        if (!b.Contains(transform.position))
        {
            turning = true;
        }
        else
        {
            turning = false;
        }

        // If the ghost needs to turn (because it hit a boundary)
        if (turning)
        {
            // Calculate the direction towards the center of the fly area
            Vector3 direction = FlockManager.FM.transform.position - transform.position;
            
            // Smoothly rotate towards the center using Slerp (Spherical Linear Interpolation)
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                Quaternion.LookRotation(direction), 
                FlockManager.FM.rotationSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 avoidanceDirection = CalculateAvoidanceDirection();

            if (avoidanceDirection != Vector3.zero)
            {
                // Prioritize obstacle avoidance
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(avoidanceDirection), FlockManager.FM.rotationSpeed * Time.deltaTime);
            }
            else

            if (distanceToLeader > neighbourDistance)
            {
                // If far from leader, focus on following the leader
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(leaderDirection), FlockManager.FM.rotationSpeed * Time.deltaTime);
                speed = Mathf.Lerp(speed, maxSpeed, 0.5f * Time.deltaTime);
            }
            else
            {
                // If close to leader, apply flocking rules
                ApplyFlockingRules();
                speed = Mathf.Lerp(speed, minSpeed, 0.5f * Time.deltaTime);
            }
        }

        // Add a slight up and down motion to simulate floating
        transform.Translate(0, Mathf.Sin(Time.time) * 0.01f, speed * Time.deltaTime);
    }


    Vector3 CalculateAvoidanceDirection()
    {
        Vector3 avoidanceDirection = Vector3.zero;
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, collisionAvoidanceDistance, obstacleLayer);

        foreach (Collider col in nearbyObjects)
        {
            Vector3 awayFromObject = transform.position - col.ClosestPoint(transform.position);
            avoidanceDirection += awayFromObject.normalized / awayFromObject.magnitude;
        }

        return avoidanceDirection.normalized;
    }

    // ApplyFlockingRules() is responsible for implementing the core flocking behaviors:
    // - Cohesion: Stay near the center of the group.
    // - Separation: Avoid getting too close to other ghost.
    // - Alignment: Match the speed of nearby ghost.
    void ApplyFlockingRules()
    {
        if (FlockManager.FM == null || FlockManager.FM.allGhosts == null) return;

        Vector3 cohesion = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 separation = Vector3.zero;
        int groupSize = 0;

        foreach (GameObject go in FlockManager.FM.allGhosts)
        {
            if (go == null || go == this.gameObject) continue;

            float distance = Vector3.Distance(go.transform.position, this.transform.position);
            if (distance <= neighbourDistance)
            {
                cohesion += go.transform.position;
                alignment += go.transform.forward;
                groupSize++;

                //Separation
                if (distance < avoidanceDistance)
                {
                    separation += (transform.position - go.transform.position) / distance;
                }
            }
        }

        if (groupSize > 0)
        {
            //cohesion
            cohesion = (cohesion / groupSize - transform.position).normalized;
            alignment = alignment / groupSize;

            Vector3 leaderDirection = (FlockManager.FM.leaderGhost.transform.position - transform.position).normalized;
            Vector3 direction = (cohesion + alignment + separation + leaderDirection * 2).normalized;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction), FlockManager.FM.rotationSpeed * Time.deltaTime);
            }
        }
    }


}