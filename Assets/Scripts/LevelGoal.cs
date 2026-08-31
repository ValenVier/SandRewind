using UnityEngine;

/// <summary>End-of-level trigger: only completes the level if the coin was already collected</summary>
[RequireComponent(typeof(Collider))]
public class LevelGoal : MonoBehaviour
{
    private bool coinCollected;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    public void SetCoinCollected(bool collected)
    {
        coinCollected = collected;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (coinCollected)
        {
            LevelManager.Instance.CompleteLevel();
        }
        else
        {
            Debug.Log("You need the gold coin before you can finish the level!");
        }
    }
}