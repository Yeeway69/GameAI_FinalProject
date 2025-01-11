using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public float cameraHeight = 10f;
    public float cameraDistance = 10f;
    public float mouseSensitivity = 2f;

    private Camera mainCamera;
    private Vector3 offset;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            offset = new Vector3(0, cameraHeight, -cameraDistance);
            mainCamera.transform.position = transform.position + offset;
            mainCamera.transform.LookAt(transform);
        }

        // Tag setup
        gameObject.tag = "Player";
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Update camera position
            mainCamera.transform.position = transform.position + offset;

            // Optional: Rotate camera with right mouse button
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                offset = Quaternion.Euler(0, mouseX, 0) * offset;
            }
        }
    }
}