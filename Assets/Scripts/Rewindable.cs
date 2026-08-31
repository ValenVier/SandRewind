using UnityEngine;

/// <summary>Attach to any object that should rewind in time. Delegates buffer logic to RewindBuffer</summary>
public class Rewindable : MonoBehaviour
{
    [Header("Recording")]
    [Tooltip("Physics ticks between recorded keyframes. 1 = record every tick " +
             "(smoothest, most memory). 5 = record every 5th tick (recommended).")]
    [SerializeField] private int ticksPerKeyframe = 5;

    [Tooltip("Max keyframes kept in the buffer. With FixedUpdate at 50/s and " +
             "ticksPerKeyframe = 5, 128 keyframes is roughly 12-13 seconds of history.")]
    [SerializeField] private int maxKeyframes = 128;

    [Header("Physics (optional)")]
    [Tooltip("If set, this Rigidbody's velocity is also recorded/restored so the " +
             "object doesn't keep its old momentum after a rewind. Leave empty for " +
             "purely kinematic/animated objects.")]
    [SerializeField] private Rigidbody rb;

    public bool IsRewinding { get; private set; }
    public bool HasHistory => buffer.HasHistory;
    public int StoredKeyframeCount => buffer.Count;

    private RewindBuffer buffer;

    private int frameCounter;
    private int reverseCounter;
    private bool firstRewindTick = true;

    private RewindKeyframe currentFrame;
    private RewindKeyframe previousFrame;

    // Kinematic bodies ignore velocity entirely, so we can't apply this every
    // tick while rewinding - just remember it and hand it back once in StopRewinding()
    private Vector3 lastVelocity;
    private Vector3 lastAngularVelocity;

    private void Awake()
    {
        buffer = new RewindBuffer(maxKeyframes);
    }

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!IsRewinding)
        {
            Record();
        }
        else
        {
            Rewind();
        }
    }

    private void Record()
    {
        if (frameCounter < ticksPerKeyframe)
        {
            frameCounter++;
            return;
        }

        frameCounter = 0;

        Vector3 velocity = rb ? rb.linearVelocity : Vector3.zero;
        Vector3 angularVelocity = rb ? rb.angularVelocity : Vector3.zero;

        buffer.Record(new RewindKeyframe(transform.position, transform.rotation, velocity, angularVelocity));
    }

    private void Rewind()
    {
        if (!buffer.HasHistory)
        {
            StopRewinding();
            return;
        }

        if (reverseCounter > 0)
        {
            reverseCounter--;
        }
        else
        {
            reverseCounter = ticksPerKeyframe;

            if (!buffer.PopPair(out currentFrame, out previousFrame))
            {
                StopRewinding();
                return;
            }
        }

        if (firstRewindTick)
        {
            firstRewindTick = false;

            if (!buffer.PopPair(out currentFrame, out previousFrame))
            {
                StopRewinding();
                return;
            }
        }

        float t = ticksPerKeyframe > 0 ? (float)reverseCounter / ticksPerKeyframe : 1f;
        Vector3 newPosition = Vector3.Lerp(previousFrame.position, currentFrame.position, t);
        Quaternion newRotation = Quaternion.Slerp(previousFrame.rotation, currentFrame.rotation, t);

        if (rb)
        {
            // MovePosition/MoveRotation smooth via Rigidbody interpolation; direct transform assignment snaps
            rb.MovePosition(newPosition);
            rb.MoveRotation(newRotation);
        }
        else
        {
            transform.position = newPosition;
            transform.rotation = newRotation;
        }

        lastVelocity = Vector3.Lerp(previousFrame.velocity, currentFrame.velocity, t);
        lastAngularVelocity = Vector3.Lerp(previousFrame.angularVelocity, currentFrame.angularVelocity, t);
    }

    public void StartRewinding()
    {
        if (IsRewinding) return;

        IsRewinding = true;
        firstRewindTick = true;
        reverseCounter = 0;

        if (rb)
        {
            // Only zero velocity if still dynamic - setting it while already kinematic just warns
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            rb.isKinematic = true; // we drive the transform by hand while rewinding
        }
    }

    public void StopRewinding()
    {
        if (!IsRewinding) return;

        IsRewinding = false;
        frameCounter = 0;

        if (rb)
        {
            rb.isKinematic = false;
            rb.linearVelocity = lastVelocity;
            rb.angularVelocity = lastAngularVelocity;
        }
    }
}