using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script should be attached to the ManagerFlocking object in your scene.
// Ensure that you assign the ghost prefab in the Inspector under the variable 'fishPrefab'.

public class FlockManager : MonoBehaviour
{
    // Static instance of FlockManager, allowing access from any Flock script.
    public static FlockManager FM; 
    
    // The ghost prefab to instantiate for each ghost in the flock
    public GameObject ghostPrefab;

    public GameObject leaderPrefab;
    public GameObject leaderGhost;

    

    // Number of ghost in the flock
    public int numGhosts = 20; 
    
    // Array to store all instantiated ghosts
    public GameObject[] allGhosts; 
    
    
    // Defines the 3D space within which the Ghosts can fly
    
    public Vector3 flyLimits = new Vector3(5, 5, 5);

    // Randomly selected goal position for ghost to move towards
    public Vector3 goalPos = Vector3.zero;

    [Header("Ghost Settings")] // Organizes the following variables in the Inspector for easier configuration
    
    // Minimum speed of the ghost (adjustable in Inspector)
    [Range(0.0f, 5.0f)]
    public float minSpeed;
    
    // Maximum speed of the ghost (adjustable in Inspector)
    [Range(0.0f, 5.0f)]
    public float maxSpeed;
    
    // Distance at which ghost recognize each other as neighbors (for cohesion, separation, and alignment)
    [Range(1.0f, 10.0f)]
    public float neighbourDistance;
    
    // Rotation speed of the ghost when changing direction (adjustable in Inspector)
    [Range(1.0f, 5.0f)]
    public float rotationSpeed;

    // Start is called before the first frame update
    // This method initializes the ghost by spawning them within the defined limits
    void Start()
    {
        // Initialize the array to hold all the ghost
        allGhosts= new GameObject[numGhosts];

        // Instiantiate the leader
        Vector3 leaderPos = this.transform.position + new Vector3(
            Random.Range(-flyLimits.x, flyLimits.x),
            Random.Range(-flyLimits.y, flyLimits.y),
            Random.Range(-flyLimits.z, flyLimits.z));
        leaderGhost = Instantiate(leaderPrefab, leaderPos, Quaternion.identity);

        // Loop to create and place each ghost randomly within the fly limits
        for (int i = 0; i < numGhosts - 1; i++)
        {
            // Calculate a random position within the fly limits
            Vector3 pos = this.transform.position + new Vector3(
                Random.Range(-flyLimits.x, flyLimits.x),
                Random.Range(-flyLimits.y, flyLimits.y),  
                Random.Range(-flyLimits.z, flyLimits.z));
            
            // Instantiate the ghost prefab at the random position with no rotation
            allGhosts[i] = Instantiate(ghostPrefab, pos, Quaternion.identity);
        }
        

       
        
        // Add the leader to the end of the allGhosts array
        allGhosts[numGhosts - 1] = leaderGhost;

        // Set the static reference to this instance of FlockManager
        FM = this;
        
        // Initialize the goal position as the FlockManager's position
        goalPos = this.transform.position;
    }

    // Update is called once per frame
    // Randomly updates the goal position for ghost to fly towards
    void Update()
    {
        // Occasionally change the goal position (10% chance each frame)
        if (Random.Range(0, 100) < 10)
        {
            // Calculate a new random goal position within the fly limits
            goalPos = this.transform.position + new Vector3(
                Random.Range(-flyLimits.x, flyLimits.x),
                Random.Range(-flyLimits.y, flyLimits.y),  
                Random.Range(-flyLimits.z, flyLimits.z));
        }
    }
}
