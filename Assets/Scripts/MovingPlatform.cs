using UnityEngine;

/// <summary>Back-and-forth kinematic platform; exposes DeltaPosition so riders can be carried along</summary>
[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;
    [Tooltip("Seconds to pause at each end before turning back - gives the player a stable moment to land.")]
    [SerializeField] private float waitTime = 0.5f;

    private Rigidbody rb;
    private Vector3 worldA;
    private Vector3 worldB;
    private bool headingToB = true;
    private float waitTimer;

    /// <summary>How far this platform moved last FixedUpdate; riders add this to their own position</summary>
    public Vector3 DeltaPosition { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void Start()
    {
        // Freeze as world positions once, so it doesn't chase a target that moves with itself
        worldA = pointA != null ? pointA.position : transform.position;
        worldB = pointB != null ? pointB.position : transform.position;
    }

    private void FixedUpdate()
    {
        Vector3 previousPosition = rb.position;
        DeltaPosition = Vector3.zero;

        if (pointA == null || pointB == null) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 target = headingToB ? worldB : worldA;
        Vector3 newPosition = Vector3.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);
        DeltaPosition = newPosition - previousPosition;

        if (Vector3.Distance(newPosition, target) < 0.01f)
        {
            headingToB = !headingToB;
            waitTimer = waitTime;
        }
    }
}