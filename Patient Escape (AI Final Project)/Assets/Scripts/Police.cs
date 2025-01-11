using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PoliceChasing : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform npc;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        npc = FindObjectOfType<ClickToMove>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(npc.position);
    }
}
