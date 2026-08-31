using UnityEngine;

/// <summary>Straight-line projectile: kills the player on contact, destroys itself on any hit or timeout</summary>
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;

    private Vector3 direction;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    /// <summary>Call right after Instantiate to set the travel direction</summary>
    public void Launch(Vector3 launchDirection)
    {
        direction = launchDirection.normalized;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerDeathHandler deathHandler = other.GetComponentInParent<PlayerDeathHandler>();
            if (deathHandler != null)
            {
                deathHandler.Die(DeathCause.Projectile);
            }
        }

        Destroy(gameObject);
    }
}