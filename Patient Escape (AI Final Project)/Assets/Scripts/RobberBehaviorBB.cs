using UnityEngine;
using UnityEngine.AI;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using Pada1.BBCore.Framework;

namespace RobberBehaviors
{

    [Condition("RobberConditions/IsTreasureStolen")]
    public class IsTreasureStolen : ConditionBase
    {
        [InParam("treasure")]
        public GameObject treasure;

        public override bool Check()
        {
            if (treasure == null) return false;
            return !treasure.GetComponent<Renderer>().enabled;
        }
    }

    // Primary condition to check if cop is near treasure
    [Condition("RobberConditions/IsCopNear")]
    public class IsCopNear : ConditionBase
    {
        [InParam("cop")]
        public Transform cop;

        [InParam("treasure")]
        public GameObject treasure;

        [InParam("dist2Steal")]
        public float dist2Steal = 10f;

        public override bool Check()
        {
            if (cop == null || treasure == null) return true;

            float distance = Vector3.Distance(cop.position, treasure.transform.position);
            bool isNear;
            if(distance < dist2Steal)
            {
                isNear = true;
            }
            else
            {
                isNear = false;
            }

            return isNear;
        }
    }

    // Wander Action
    [Action("RobberActions/WanderBB")]
    public class WanderBB : BasePrimitiveAction
    {
        [InParam("robber")]
        public GameObject robber;

        private NavMeshAgent agent;
        private Vector3 wanderPoint;
        private float wanderRadius = 10f;

        public override void OnStart()
        {
            agent = robber.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = 2f;
                Debug.Log("Started Wandering");
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (agent == null) return TaskStatus.FAILED;

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
                randomDirection += robber.transform.position;
                randomDirection.y = robber.transform.position.y;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }

            return TaskStatus.RUNNING;
        }
    }

    // Approach and Steal Action
    [Action("RobberActions/ApproachAndStealBB")]
    public class ApproachAndStealBB : BasePrimitiveAction
    {
        [InParam("robber")]
        public GameObject robber;

        [InParam("treasure")]
        public GameObject treasure;

        [InParam("cop")]
        public Transform cop;

        [InParam("dist2Steal")]
        public float dist2Steal = 10f;

        private NavMeshAgent agent;
        private bool hasStartedApproaching = false;

        public override void OnStart()
        {
            agent = robber.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = 3f;
                hasStartedApproaching = false;
                Debug.Log("Started Approaching");
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (agent == null || treasure == null) return TaskStatus.FAILED;

            // If cop comes back, return to wandering
            float copDistance = Vector3.Distance(cop.position, treasure.transform.position);
            if (copDistance < dist2Steal)
            {
                Debug.Log("Cop returned, going back to wander");
                agent.speed = 1f;
                return TaskStatus.FAILED;
            }

            // Check if close enough to steal
            float distanceToTreasure = Vector3.Distance(robber.transform.position, treasure.transform.position);
            if (distanceToTreasure < 2f)
            {
                treasure.GetComponent<Renderer>().enabled = false;
                Debug.Log("Treasure stolen!");
                return TaskStatus.COMPLETED;
            }

            // Keep approaching
            if (!hasStartedApproaching || agent.remainingDistance < 0.1f)
            {
                agent.SetDestination(treasure.transform.position);
                hasStartedApproaching = true;
            }

            return TaskStatus.RUNNING;
        }
    }

    // Hide Action
    [Action("RobberActions/HideBB")]
    public class HideBB : BasePrimitiveAction
    {
        [InParam("robber")]
        public GameObject robber;

        private NavMeshAgent agent;
        private bool hasFoundHidingSpot = false;
        private Vector3 hidingSpot;

        public override void OnStart()
        {
            agent = robber.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = 3.5f;
                hasFoundHidingSpot = false;
                Debug.Log("Started Hiding");
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (agent == null) return TaskStatus.FAILED;

            if (!hasFoundHidingSpot)
            {
                GameObject[] hideSpots = GameObject.FindGameObjectsWithTag("Hide");
                if (hideSpots.Length > 0)
                {
                    // Find furthest hiding spot
                    float furthestDistance = 0f;
                    foreach (GameObject spot in hideSpots)
                    {
                        float distance = Vector3.Distance(robber.transform.position, spot.transform.position);
                        if (distance > furthestDistance)
                        {
                            furthestDistance = distance;
                            hidingSpot = spot.transform.position;
                        }
                    }
                    agent.SetDestination(hidingSpot);
                    hasFoundHidingSpot = true;
                }
            }

            return TaskStatus.RUNNING;  // Keep hiding forever
        }
    }
}