using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderFlock : MonoBehaviour
{
    public float speed = 2f;
    public float rotationSpeed = 2f;
    private Vector3 targetPosition;
    private float swoopTimer = 0f;
    private bool isSwooping = false;
    private float changeDirectionInterval = 10f; // Longer interval for more stable movement
    private float timeSinceLastChange = 0f;

    void Start()
    {
        SetNewTarget();
        
    }

    void Update()
    {
        timeSinceLastChange += Time.deltaTime;

        if (timeSinceLastChange >= changeDirectionInterval || Vector3.Distance(targetPosition, transform.position) <= 1)
        {
            SetNewTarget();
            timeSinceLastChange = 0f;
        }

        if (Vector3.Distance(transform.position, targetPosition) < 1f || Random.Range(0, 1000) < 5)
        {
            SetNewTarget();
        }

        Vector3 direction = (targetPosition - transform.position).normalized;

        if (!isSwooping)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            // Add a slight up and down motion to simulate floating
            transform.Translate(Vector3.up * Mathf.Sin(Time.time * 0.5f) * 0.05f * Time.deltaTime);

            // Randomly start swooping
            if (Random.Range(0, 1000) < 5)
            {
                isSwooping = true;
                swoopTimer = 0f;
            }
        }
        else
        {
            // Perform swooping motion
            swoopTimer += Time.deltaTime;
            float swoopHeight = Mathf.Sin(swoopTimer * 3f) * 2f;
            transform.Translate(direction * speed * 1.5f * Time.deltaTime + Vector3.up * swoopHeight * Time.deltaTime);

            if (swoopTimer > 2f)
            {
                isSwooping = false;
            }
        }
    }

    void SetNewTarget()
    {
        targetPosition = FlockManager.FM.transform.position + new Vector3(
            Random.Range(-FlockManager.FM.flyLimits.x, FlockManager.FM.flyLimits.x),
            Random.Range(-FlockManager.FM.flyLimits.y, FlockManager.FM.flyLimits.y),
            Random.Range(-FlockManager.FM.flyLimits.z, FlockManager.FM.flyLimits.z));
    }
}
