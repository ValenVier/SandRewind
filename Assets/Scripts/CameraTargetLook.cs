using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Rotates this empty child of the player based on mouse/gamepad look input; PlayerController reads it as forward</summary>
public class CameraTargetLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gamepadSensitivity = 120f; // degrees/sec at full stick deflection
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;
    [Tooltip("Locks and hides the cursor for mouse-look. Press Escape to release it, click to recapture.")]
    [SerializeField] private bool lockCursor = true;

    private float yaw;
    private float pitch;

    private void Start()
    {
        Vector3 startEuler = transform.eulerAngles;
        yaw = startEuler.y;
        pitch = startEuler.x;

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
            if (rightStick.sqrMagnitude > 0.02f) // deadzone, ignores stick drift
            {
                lookX += rightStick.x * gamepadSensitivity * Time.deltaTime;
                lookY += rightStick.y * gamepadSensitivity * Time.deltaTime;
            }
        }

        yaw += lookX;
        pitch = Mathf.Clamp(pitch - lookY, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
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
}