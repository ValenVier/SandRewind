using UnityEngine;

/// <summary>Trap turret: fires a Projectile prefab forward at a fixed interval</summary>
public class ProjectileLauncher : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [Tooltip("Optional. Defaults to this object's own position/rotation if left empty.")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireInterval = 2f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fireInterval)
        {
            timer = 0f;
            Fire();
        }
    }

    private void Fire()
    {
        if (projectilePrefab == null) return;

        Transform origin = firePoint != null ? firePoint : transform;
        Projectile instance = Instantiate(projectilePrefab, origin.position, origin.rotation);
        instance.Launch(origin.forward);
    }
}