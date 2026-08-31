using UnityEngine;

/// <summary>Collectible coin: flags LevelGoal as collected on pickup, then removes itself</summary>
[RequireComponent(typeof(Collider))]
public class CoinPickup : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 90f; // degrees per second
    [SerializeField] private LevelGoal levelGoal;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        // Just a visual spin, no gameplay effect
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (levelGoal != null) levelGoal.SetCoinCollected(true);
        Destroy(gameObject);
    }
}