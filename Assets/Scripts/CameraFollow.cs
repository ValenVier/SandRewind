using UnityEngine;
using UnityEngine.InputSystem;

// NOT IN USE
/// <summary>Fixed-offset third-person camera with a small look tilt.</summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 4f, -6f);
    [SerializeField] private float positionSmoothTime = 0.15f;

    [Header("Small look tilt (not an orbit)")]
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float gamepadLookSensitivity = 30f;
    [Tooltip("Max degrees you can tilt the view left/right or up/down. Keep this small - this is a peek, not a turn.")]
    [SerializeField] private float maxLookOffset = 12f;
    [SerializeField] private float lookSmoothSpeed = 8f;

    [Tooltip("Locks and hides the cursor for mouse-look. Press Escape to release it, click to recapture.")]
    [SerializeField] private bool lockCursor = true;

    private Vector3 positionVelocity;
    private float rawYaw;
    private float rawPitch;
    private float smoothedYaw;
    private float smoothedPitch;

    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        HandleCursorLock();

        float lookX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float lookY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (Gamepad.current != null)
        {
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
            lookX += rightStick.x * gamepadLookSensitivity * Time.deltaTime;
            lookY += rightStick.y * gamepadLookSensitivity * Time.deltaTime;
        }

        rawYaw = Mathf.Clamp(rawYaw + lookX, -maxLookOffset, maxLookOffset);
        rawPitch = Mathf.Clamp(rawPitch - lookY, -maxLookOffset, maxLookOffset);

        // Smooth toward the raw value instead of snapping to it
        float smoothing = 1f - Mathf.Exp(-lookSmoothSpeed * Time.deltaTime);
        smoothedYaw = Mathf.Lerp(smoothedYaw, rawYaw, smoothing);
        smoothedPitch = Mathf.Lerp(smoothedPitch, rawPitch, smoothing);
    }

    private void HandleCursorLock()
    {
        if (!lockCursor) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Cursor.lockState != CursorLockMode.Locked &&
                 (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Fixed offset position, never affected by look input
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref positionVelocity, positionSmoothTime);

        // Look at target, then add the small tilt on top
        Vector3 lookDir = (target.position + Vector3.up) - transform.position;
        Quaternion baseRotation = Quaternion.LookRotation(lookDir, Vector3.up);
        Quaternion tilt = Quaternion.Euler(smoothedPitch, smoothedYaw, 0f);
        transform.rotation = baseRotation * tilt;
    }
}