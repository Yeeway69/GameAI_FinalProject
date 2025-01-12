using System.Linq;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    private Moves moves;
    private bool isInPlayerView;
    private bool isAware;

    public float awarenessRadius = 10f;
    public bool showDebug = true;

    // Unique identifier for each alert to prevent recursive alerts
    private static int currentAlertID = 0;
    private int lastReceivedAlertID = -1;

    void Start()
    {
        moves = GetComponent<Moves>();
        if (moves != null && moves.target == null)
        {
            // Try to find the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                moves.target = player;
                if (showDebug)
                {
                    Debug.Log($"ZombieController found and set player target for {gameObject.name}");
                }
            }
            else if (showDebug)
            {
                Debug.LogWarning($"ZombieController couldn't find player target for {gameObject.name}");
            }
        }
        if (showDebug)
        {
            Debug.Log($"ZombieController started on {gameObject.name}");
        }
    }

    void Update()
    {
        // Check if any zombie still sees the player
        if (isAware && !isInPlayerView)
        {
            bool anyZombieSeeingPlayer = GameObject.FindObjectsOfType<ZombieController>().Any(z => z.isInPlayerView);
            if (!anyZombieSeeingPlayer)
            {
                isAware = false;
                if (showDebug) Debug.Log($"Zombie {gameObject.name} lost awareness - no zombies see player");
            }
        }

        if (isInPlayerView || isAware)
        {
            moves.Pursue();
            if (showDebug)
            {
                Debug.Log($"Zombie {gameObject.name} is pursuing. IsInView: {isInPlayerView}, IsAware: {isAware}");
                if (moves.target != null)
                {
                    Debug.DrawLine(transform.position, moves.target.transform.position, Color.red);
                }
            }
        }
        else
        {
            if (showDebug)
            {
                Debug.Log($"Zombie {gameObject.name} is chilling");
            }
        }
    }

    public class AlertData
    {
        public Vector3 position;
        public int alertID;

        public AlertData(Vector3 pos, int id)
        {
            position = pos;
            alertID = id;
        }
    }

    void SendZombieAlert(Vector3 position)
    {
        // Generate new alert ID
        currentAlertID++;
        AlertData alertData = new AlertData(position, currentAlertID);

        // Alert all zombies in the scene through broadcast
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (GameObject zombie in zombies)
        {
            zombie.BroadcastMessage("OnZombieAlert", alertData, SendMessageOptions.DontRequireReceiver);
        }

        if (showDebug)
        {
            Debug.Log($"Zombie {gameObject.name} sending alert #{currentAlertID}");
        }
    }

    public void OnZombieAlert(AlertData alertData)
    {
        // Prevent processing the same alert multiple times
        if (alertData.alertID <= lastReceivedAlertID)
        {
            return;
        }

        lastReceivedAlertID = alertData.alertID;

        float distance = Vector3.Distance(transform.position, alertData.position);
        if (showDebug)
        {
            Debug.Log($"Zombie {gameObject.name} received alert #{alertData.alertID}. Distance: {distance}, Radius: {awarenessRadius}");
        }

        if (distance < awarenessRadius)
        {
            isAware = true;
            if (showDebug)
            {
                Debug.Log($"Zombie {gameObject.name} became aware from alert #{alertData.alertID}!");
                Debug.DrawLine(transform.position, alertData.position, Color.yellow, 1f);
            }
        }
    }

    public void OnSpottedByPlayer(bool spotted)
    {
        isInPlayerView = spotted;
        if (spotted)
        {
            if (showDebug)
            {
                Debug.Log($"Zombie {gameObject.name} spotted player!");
            }
            SendZombieAlert(transform.position);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isInPlayerView ? Color.red : (isAware ? Color.yellow : Color.green);
        Gizmos.DrawWireSphere(transform.position, awarenessRadius);

        if (isInPlayerView || isAware)
        {
            Vector3 textPosition = transform.position + Vector3.up * 2;
            string state = isInPlayerView ? "SPOTTED" : "AWARE";
            UnityEditor.Handles.Label(textPosition, state);
        }
    }
}