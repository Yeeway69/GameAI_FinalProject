using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public GameObject playerTarget; // Add reference to player

    public int numberOfZombies = 5;
    public float spawnRadius = 10f;

    void Start()
    {
        // Ensure we have a player reference
        if (playerTarget == null)
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player");
            if (playerTarget == null)
            {
                Debug.LogError("No player found! Ensure player has 'Player' tag!");
                return;
            }
        }
        SpawnZombies();
    }

    void SpawnZombies()
    {
        for (int i = 0; i < numberOfZombies; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                GameObject zombie = Instantiate(zombiePrefab, hit.position, Quaternion.identity);

                // Explicitly set target reference for both components
                Moves moves = zombie.GetComponent<Moves>();
                if (moves != null)
                {
                    moves.target = playerTarget;
                    Debug.Log($"Set player target for zombie {zombie.name}");
                }
            }
        }
    }

    // Optional: Visualize spawn area in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}