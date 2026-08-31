using UnityEngine;

/// <summary>Collectible sand pickup: adds sand to the player's SandMeter on contact</summary>
[RequireComponent(typeof(Collider))]
public class SandPickup : MonoBehaviour
{
    [SerializeField] private float sandAmount = 3f;
    [SerializeField] private bool destroyOnPickup = true;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        SandMeter sandMeter = other.GetComponentInParent<SandMeter>();
        if (sandMeter == null) return;

        sandMeter.AddSand(sandAmount);

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }
}