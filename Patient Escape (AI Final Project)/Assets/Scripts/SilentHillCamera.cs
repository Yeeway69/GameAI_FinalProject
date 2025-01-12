using UnityEngine;
using System.Collections.Generic;

public class SilentHillCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // The player to follow
    public float followSpeed = 5f; // How fast the camera moves to follow the player
    public Vector3 offset = new Vector3(0, 10f, -10f); // Default camera offset from player

    [Header("Camera Path Settings")]
    public List<Transform> pathNodes = new List<Transform>(); // List of path nodes the camera can move between
    public float pathBlendDistance = 5f; // Distance at which we start blending between camera positions
    private int currentNodeIndex = 0;
    private int nextNodeIndex = 1;

    [Header("Camera Behavior")]
    public float rotationSpeed = 2f; // How fast the camera rotates to look at the player
    public float heightOffset = 2f; // Height offset when looking at player (to look slightly above them)
    public bool smoothRotation = true; // Whether to smooth the camera rotation

    private Camera mainCamera;

    private void Start()
    {
        // Get the camera component
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("This script must be attached to a Camera!");
            enabled = false;
            return;
        }

        // If no target is set, try to find the player
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("No player found! Please assign a target for the camera to follow.");
                enabled = false;
                return;
            }
        }

        // Ensure we have path nodes
        if (pathNodes.Count < 2)
        {
            Debug.LogWarning("Camera path needs at least 2 nodes to function properly!");
        }
    }

    private void LateUpdate()
    {
        if (target == null || pathNodes.Count < 2) return;

        // Find the closest path nodes to the player
        UpdatePathNodes();

        // Calculate the ideal camera position based on the current path
        Vector3 targetCameraPosition = CalculateCameraPosition();

        // Smoothly move the camera
        transform.position = Vector3.Lerp(transform.position, targetCameraPosition, Time.deltaTime * followSpeed);

        // Make the camera look at the player
        Vector3 targetLookPosition = target.position + Vector3.up * heightOffset;
        if (smoothRotation)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetLookPosition - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            transform.LookAt(targetLookPosition);
        }
    }

    private void UpdatePathNodes()
    {
        float minDistance = float.MaxValue;
        int closestNode = 0;

        // Find the closest node to the player
        for (int i = 0; i < pathNodes.Count; i++)
        {
            float distance = Vector3.Distance(target.position, pathNodes[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNode = i;
            }
        }

        // Update current and next node indices
        currentNodeIndex = closestNode;
        nextNodeIndex = (closestNode + 1) % pathNodes.Count;
    }

    private Vector3 CalculateCameraPosition()
    {
        Vector3 currentNodePos = pathNodes[currentNodeIndex].position;
        Vector3 nextNodePos = pathNodes[nextNodeIndex].position;

        // Calculate the blend factor based on player's position between nodes
        Vector3 playerPosFlat = new Vector3(target.position.x, 0, target.position.z);
        Vector3 currentNodeFlat = new Vector3(currentNodePos.x, 0, currentNodePos.z);
        Vector3 nextNodeFlat = new Vector3(nextNodePos.x, 0, nextNodePos.z);

        // Project player position onto the path line
        Vector3 pathDirection = (nextNodeFlat - currentNodeFlat).normalized;
        float dotProduct = Vector3.Dot(playerPosFlat - currentNodeFlat, pathDirection);
        float blendFactor = Mathf.Clamp01(dotProduct / Vector3.Distance(currentNodeFlat, nextNodeFlat));

        // Lerp between the two camera positions
        return Vector3.Lerp(currentNodePos, nextNodePos, blendFactor);
    }

    // Helper method to visualize the camera path in the editor
    private void OnDrawGizmos()
    {
        if (pathNodes.Count < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < pathNodes.Count; i++)
        {
            if (pathNodes[i] == null) continue;

            // Draw node
            Gizmos.DrawWireSphere(pathNodes[i].position, 0.5f);

            // Draw line to next node
            if (i < pathNodes.Count - 1 && pathNodes[i + 1] != null)
            {
                Gizmos.DrawLine(pathNodes[i].position, pathNodes[i + 1].position);
            }
            // Connect last node to first node
            else if (i == pathNodes.Count - 1 && pathNodes[0] != null)
            {
                Gizmos.DrawLine(pathNodes[i].position, pathNodes[0].position);
            }
        }
    }
} 