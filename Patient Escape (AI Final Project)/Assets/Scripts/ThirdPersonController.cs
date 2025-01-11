using UnityEngine;
using UnityEngine.AI;

public class ThirdPersonController : MonoBehaviour
{
    private NavMeshAgent agent;
    public float moveSpeed = 5f;
    public float turnSpeed = 360f;

    [Header("Camera Settings")]
    public float cameraHeight = 10f;
    public float cameraDistance = 10f;
    public float cameraSmoothness = 5f;
    public float mouseSensitivityX = 2f;
    public float mouseSensitivityY = 1f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    private Transform cameraTransform;
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private Vector3 cameraOffset;
    private bool isRotating = false;

    private void Start()
    {
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);

            // Initialize camera position
            Vector3 targetPosition = transform.position + cameraOffset;
            cameraTransform.position = targetPosition;
            cameraTransform.LookAt(transform.position);

            // Get initial rotation values
            currentRotationY = transform.eulerAngles.y;
            currentRotationX = cameraTransform.eulerAngles.x;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = true;
            agent.updatePosition = true;
        }
    }

    private void Update()
    {
        HandleCameraRotation();
        HandleMovement();

        // Toggle cursor lock with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ?
                              CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }

    private void HandleCameraRotation()
    {
        if (cameraTransform == null) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;

        // Update rotation values
        currentRotationY += mouseX;
        currentRotationX -= mouseY; // Inverted for natural camera feel
        currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);

        // Create rotation quaternions
        Quaternion cameraRotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);

        // Rotate player with camera's Y rotation
        transform.rotation = Quaternion.Euler(0, currentRotationY, 0);

        // Calculate and set camera position
        Vector3 targetPosition = transform.position;
        Vector3 rotatedOffset = cameraRotation * Vector3.forward * -cameraDistance;
        rotatedOffset += Vector3.up * cameraHeight;

        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            targetPosition + rotatedOffset,
            Time.deltaTime * cameraSmoothness);

        // Make camera look at player
        cameraTransform.LookAt(targetPosition + Vector3.up * (cameraHeight * 0.5f));
    }

    private void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate movement direction relative to camera
        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;

        if (movement.magnitude > 0.1f)
        {
            // Convert to camera space
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 targetDirection = (forward * movement.z + right * movement.x);

            // Move using NavMeshAgent
            if (agent != null)
            {
                agent.Move(targetDirection * moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.position += targetDirection * moveSpeed * Time.deltaTime;
            }
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}