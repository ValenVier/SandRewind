using UnityEngine;
using UnityEngine.Events;

/// <summary>The "sand in the dagger" resource: drains while rewinding, refills via SandPickup</summary>
public class SandMeter : MonoBehaviour
{
    [Header("Sand Settings")]
    [SerializeField] private float maxSand = 10f;
    [SerializeField] private float startingSand = 10f;
    [Tooltip("Units of sand consumed per second while rewinding is active.")]
    [SerializeField] private float sandDrainPerSecond = 1f;

    public float CurrentSand { get; private set; }
    public float MaxSand => maxSand;
    public float NormalizedSand => maxSand > 0f ? CurrentSand / maxSand : 0f;
    public bool HasSand => CurrentSand > 0f;

    [Header("Events (hook these up to a UI bar / SFX)")]
    public UnityEvent<float, float> OnSandChanged; // (current, max)
    public UnityEvent OnSandDepleted;

    private void Awake()
    {
        CurrentSand = Mathf.Clamp(startingSand, 0f, maxSand);
    }

    /// <summary>Call every frame while rewinding; returns false once the meter hits zero</summary>
    public bool TryDrain(float deltaTime)
    {
        if (CurrentSand <= 0f) return false;

        CurrentSand = Mathf.Max(0f, CurrentSand - sandDrainPerSecond * deltaTime);
        OnSandChanged?.Invoke(CurrentSand, maxSand);

        if (CurrentSand <= 0f)
        {
            OnSandDepleted?.Invoke();
            return false;
        }

        return true;
    }

    public void AddSand(float amount)
    {
        CurrentSand = Mathf.Clamp(CurrentSand + amount, 0f, maxSand);
        OnSandChanged?.Invoke(CurrentSand, maxSand);
    }

    public void IncreaseMaxSand(float amount)
    {
        maxSand += amount;
        OnSandChanged?.Invoke(CurrentSand, maxSand);
    }
}