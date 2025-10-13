using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public float bulletDamage = 10f;

    void OnCollisionEnter(Collision collision)
    {
        // Check if it's an enemy
        EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(bulletDamage);
            Debug.Log("Bullet hit enemy: " + collision.gameObject.name);
        }

        // Check if it's the boss (on parent if needed)
        BossController boss = collision.gameObject.GetComponent<BossController>();
        if (boss == null) // if not on child, check parent
            boss = collision.gameObject.GetComponentInParent<BossController>();

        if (boss != null)
        {
            boss.TakeDamage(bulletDamage);
            Debug.Log("Bullet hit boss: " + collision.gameObject.name);
        }

        if (enemyHealth == null && boss == null)
        {
            Debug.Log("Bullet hit something else: " + collision.gameObject.name);
        }

        Destroy(gameObject);
    }
}
