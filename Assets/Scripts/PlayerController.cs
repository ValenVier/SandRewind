using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Rigidbody platformer movement, camera-relative, with jump, ground check, and moving-platform support</summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float rotationSpeed = 720f; // degrees/sec, faces move direction

    [Header("Ground Check")]
    [Tooltip("Empty child Transform placed at the capsule's feet.")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Dependencies")]
    [Tooltip("This object's own Rewindable, so movement pauses while it rewinds.")]
    [SerializeField] private Rewindable rewindable;
    [Tooltip("The active camera (drag Main Camera here). Movement input is interpreted relative to this camera's facing - W moves the way the camera is looking, not a fixed world direction.")]
    [SerializeField] private Transform cameraTransform;

    private Rigidbody rb;
    private bool isGrounded;
    private MovingPlatform currentPlatform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Stops physics from tipping the capsule over on collision; MoveRotation still works with these frozen
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                          RigidbodyConstraints.FreezeRotationY |
                          RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        isGrounded = false;
        currentPlatform = null;

        if (groundCheck)
        {
            Collider[] hits = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, groundLayers);
            if (hits.Length > 0)
            {
                isGrounded = true;
                currentPlatform = hits[0].GetComponentInParent<MovingPlatform>();
            }
        }

        if (rewindable != null && rewindable.IsRewinding) return;

        bool jumpPressed = Input.GetButtonDown("Jump") ||
                            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        if (isGrounded && jumpPressed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    private void FixedUpdate()
    {
        if (rewindable != null && rewindable.IsRewinding) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (Mathf.Abs(stick.x) > 0.15f) h = stick.x; // deadzone
            if (Mathf.Abs(stick.y) > 0.15f) v = stick.y;
        }

        Vector3 inputDir = new Vector3(h, 0f, v);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        Vector3 moveDir = inputDir;

        if (inputDir.sqrMagnitude > 0.001f && cameraTransform != null)
        {
            // Flatten camera forward/right onto the ground plane, so pitch doesn't affect speed
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = cameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();

            moveDir = camForward * inputDir.z + camRight * inputDir.x;
        }

        Vector3 targetVelocity = moveDir * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }

        // Add the platform's own movement on top, since velocity is set directly above (friction alone can't carry us)
        if (currentPlatform != null)
        {
            rb.MovePosition(rb.position + currentPlatform.DeltaPosition);
        }
    }
}