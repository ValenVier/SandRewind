using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum DeathCause { Enemy, Projectile }

/// <summary>On death: player falls flat, gets a grace window to rewind and escape, or the level restarts</summary>
public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private float graceWindowSeconds = 4f;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RewindController rewindController;
    [SerializeField] private SandMeter sandMeter;

    [Header("Fall over on death")]
    [SerializeField] private float fallDuration = 0.3f;
    [Tooltip("How high off the ground the player's centre sits once lying down (roughly the capsule radius).")]
    [SerializeField] private float lyingHeight = 0.5f;

    [Header("Events (hook up a health bar HUD)")]
    public UnityEvent<float, float> OnGraceTimeChanged; // (remaining, total)

    public static PlayerDeathHandler Instance { get; private set; }
    public bool IsDead { get; private set; }

    private Rigidbody rb;

    private void Awake()
    {
        Instance = this;
    }

    public void Die(DeathCause cause)
    {
        if (IsDead) return;
        IsDead = true;

        if (playerController)
        {
            playerController.enabled = false;
            if (rb == null) rb = playerController.GetComponent<Rigidbody>();
        }

        StartCoroutine(FallOver());
        OnGraceTimeChanged?.Invoke(graceWindowSeconds, graceWindowSeconds);

        if (sandMeter != null && sandMeter.HasSand)
        {
            StartCoroutine(GraceWindow());
        }
        else
        {
            OnGraceTimeChanged?.Invoke(0f, graceWindowSeconds);
            LevelManager.Instance.RestartLevel();
        }
    }

    /// <summary>Tips the player onto the ground via kinematic MovePosition/MoveRotation</summary>
    private IEnumerator FallOver()
    {
        if (rb == null) yield break;

        rb.isKinematic = true;

        Transform t = rb.transform;
        Quaternion startRot = t.rotation;
        Quaternion endRot = Quaternion.Euler(90f, t.eulerAngles.y, 0f);
        Vector3 startPos = t.position;
        Vector3 endPos = new Vector3(startPos.x, lyingHeight, startPos.z);

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float lerp = elapsed / fallDuration;
            rb.MoveRotation(Quaternion.Slerp(startRot, endRot, lerp));
            rb.MovePosition(Vector3.Lerp(startPos, endPos, lerp));
            yield return null;
        }

        rb.MoveRotation(endRot);
        rb.MovePosition(endPos);
    }

    private IEnumerator GraceWindow()
    {
        float elapsed = 0f;
        bool rewindWasUsed = false;

        while (elapsed < graceWindowSeconds)
        {
            if (rewindController != null && rewindController.IsRewinding)
            {
                rewindWasUsed = true;
                break;
            }

            elapsed += Time.deltaTime;
            OnGraceTimeChanged?.Invoke(Mathf.Max(0f, graceWindowSeconds - elapsed), graceWindowSeconds);
            yield return null;
        }

        if (!rewindWasUsed)
        {
            OnGraceTimeChanged?.Invoke(0f, graceWindowSeconds);
            LevelManager.Instance.RestartLevel();
            yield break;
        }

        // Rewindable already restores the pre-death position/rotation as a side effect
        while (rewindController.IsRewinding)
        {
            yield return null;
        }

        Revive();
    }

    private void Revive()
    {
        IsDead = false;
        if (playerController) playerController.enabled = true;
        OnGraceTimeChanged?.Invoke(graceWindowSeconds, graceWindowSeconds);
    }
}