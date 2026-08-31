using UnityEngine;

/// <summary>
/// Simple patrol/chase enemy. Patrols between two points when the player is
/// out of sight; switches to chasing once the player enters both the
/// detection radius and the view cone in front of it (so the "face" you set
/// up visually actually matters - stand behind it and it won't see you).
/// Loses interest and returns to patrol after a few seconds without seeing
/// the player. Killing the player happens on direct contact.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EnemyChaser : MonoBehaviour
{
    private enum State { Patrol, Chase }

    [Header("Detection")]
    [SerializeField] private float viewDistance = 8f;
    [Tooltip("Full view cone angle in degrees, centred on this object's forward (blue) axis.")]
    [SerializeField] private float viewAngle = 100f;
    [SerializeField] private float loseSightAfterSeconds = 2f;
    [Tooltip("Optional: walls on these layers block line of sight. Leave as Nothing to ignore obstacles.")]
    [SerializeField] private LayerMask obstacleLayers;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4.5f;
    [SerializeField] private float rotationSpeed = 540f;

    [Header("Patrol")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float waypointTolerance = 0.2f;

    [Header("Dependencies")]
    [Tooltip("This object's own Rewindable, so its AI pauses while it's being rewound by the player.")]
    [SerializeField] private Rewindable rewindable;

    private Transform player;
    private State state = State.Patrol;
    private Vector3 waypointA;
    private Vector3 waypointB;
    private bool headingToB;
    private float timeSinceLastSeen;
    private bool wasRewinding;
    private bool wasPlayerDead;

    private void Awake()
    {
        // Safety net: must be a trigger, or it'll push the player instead of killing them
        GetComponent<Collider>().isTrigger = true;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;

        // Freeze patrol points as world positions once, so the route doesn't chase itself if pointA/B are children
        if (pointA != null) waypointA = pointA.position;
        if (pointB != null) waypointB = pointB.position;
    }

    private void Update()
    {
        bool isRewinding = rewindable != null && rewindable.IsRewinding;

        if (isRewinding)
        {
            // Rewindable is driving this object's transform directly
            wasRewinding = true;
            return;
        }

        if (wasRewinding)
        {
            // Re-evaluate fresh instead of carrying over stale state
            wasRewinding = false;
            timeSinceLastSeen = 0f;
            state = CanSeePlayer() ? State.Chase : State.Patrol;
        }

        bool playerIsDead = PlayerDeathHandler.Instance != null && PlayerDeathHandler.Instance.IsDead;

        if (playerIsDead)
        {
            if (!wasPlayerDead)
            {
                // Player just got caught, stand down instead of hovering over them
                wasPlayerDead = true;
                state = State.Patrol;
                timeSinceLastSeen = 0f;
                Debug.Log($"{name}: player is down -> standing down", this);
            }

            Patrol();
            return;
        }

        wasPlayerDead = false;

        bool canSeePlayer = CanSeePlayer();

        if (canSeePlayer)
        {
            if (state != State.Chase)
            {
                Debug.Log($"{name}: spotted the player -> switching to Chase", this);
            }
            state = State.Chase;
            timeSinceLastSeen = 0f;
        }
        else if (state == State.Chase)
        {
            timeSinceLastSeen += Time.deltaTime;
            if (timeSinceLastSeen >= loseSightAfterSeconds)
            {
                state = State.Patrol;
                Debug.Log($"{name}: lost the player -> back to Patrol", this);
            }
        }

        if (state == State.Chase && player != null)
        {
            MoveTowards(player.position, chaseSpeed);
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        if (pointA == null || pointB == null) return;

        Vector3 target = headingToB ? waypointB : waypointA;
        MoveTowards(target, patrolSpeed);

        // Horizontal-only check, since movement ignores height too
        Vector2 flatPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 flatTarget = new Vector2(target.x, target.z);

        if (Vector2.Distance(flatPos, flatTarget) <= waypointTolerance)
        {
            headingToB = !headingToB;
            Debug.Log($"{name}: reached waypoint, now heading to {(headingToB ? "B" : "A")} at {(headingToB ? waypointB : waypointA)}", this);
        }
    }

    private void MoveTowards(Vector3 targetPosition, float speed)
    {
        Vector3 flatTarget = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        Vector3 direction = flatTarget - transform.position;

        if (direction.sqrMagnitude < 0.0001f) return;
        direction.Normalize();

        transform.position += direction * speed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        if (distance > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > viewAngle * 0.5f) return false;

        if (obstacleLayers.value != 0 &&
            Physics.Raycast(transform.position, toPlayer.normalized, distance, obstacleLayers))
        {
            return false; // something blocks the line of sight
        }

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerDeathHandler deathHandler = other.GetComponentInParent<PlayerDeathHandler>();
        if (deathHandler != null)
        {
            deathHandler.Die(DeathCause.Enemy);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Green = patrolling, red = chasing, yellow = not playing
        Gizmos.color = Application.isPlaying
            ? (state == State.Chase ? new Color(1f, 0f, 0f, 0.3f) : new Color(0f, 1f, 0f, 0.3f))
            : new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // Patrol waypoints, Cyan = point A, magenta = point B
        Vector3 posA = Application.isPlaying ? waypointA : (pointA ? pointA.position : transform.position);
        Vector3 posB = Application.isPlaying ? waypointB : (pointB ? pointB.position : transform.position);

        if (pointA != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(posA, waypointTolerance);
            Gizmos.DrawLine(transform.position, posA);
        }

        if (pointB != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(posB, waypointTolerance);
            Gizmos.DrawLine(transform.position, posB);
        }
    }
}