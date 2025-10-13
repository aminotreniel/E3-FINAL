using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public Transform player;
    private PlayerHealth playerHealth;

    [Header("Boss Settings")]
    public float detectionRadius = 10f;   // How far boss can "see" the player
    public float attackRange = 2f;        // How close boss must be to attack
    public float moveSpeed = 2f;          // Walking speed
    public float attackDamage = 20f;      // Damage dealt to player per attack
    public float attackCooldown = 2f;     // Time between attacks

    [Header("Health Settings")]
    public float maxHealth = 200f;
    public float currentHealth;

    [Header("UI Settings")]
    public Canvas healthBarCanvas;   // World Space Canvas
    public Slider healthSlider;      // Assign Slider component
    public Vector3 healthBarOffset = new Vector3(0, 3f, 0); // Position above boss

    private bool isDead = false;
    private float lastAttackTime = 0f;
    private Camera mainCamera;

    void Start()
    {
        currentHealth = maxHealth;

        if (anim == null) anim = GetComponent<Animator>();
        if (player != null) playerHealth = player.GetComponent<PlayerHealth>();

        mainCamera = Camera.main;

        Debug.Log(gameObject.name + " spawned with " + currentHealth + " health.");

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius)
        {
            if (distance <= attackRange)
            {
                StopMoving();
                TryAttack();
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
        else
        {
            StopMoving();
        }

        // Keep healthbar facing camera
        if (healthBarCanvas != null && mainCamera != null)
        {
            healthBarCanvas.transform.position = transform.position + healthBarOffset;
            healthBarCanvas.transform.LookAt(mainCamera.transform);
            healthBarCanvas.transform.Rotate(0, 180f, 0); // prevent flipping
        }
    }

    // --------------------
    // MOVEMENT + ATTACK
    // --------------------
    private void MoveTowardsPlayer()
    {
        anim.SetBool("IsWalking", true);

        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(targetPos);

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    private void StopMoving()
    {
        anim.SetBool("IsWalking", false);
    }

    private void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            anim.ResetTrigger("Attack");
            anim.SetTrigger("Attack");

            if (playerHealth != null && !playerHealth.isDead)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log("Boss attacked! Dealt " + attackDamage + " damage.");
            }

            lastAttackTime = Time.time;
        }
    }

    // --------------------
    // HEALTH + DAMAGE
    // --------------------
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log(">>> " + gameObject.name + " took " + damageAmount + " damage! Health: " + currentHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            int hitType = Random.Range(1, 3);
            PlayHitAnimation(hitType);
        }
    }

    private void PlayHitAnimation(int hitType)
    {
        if (isDead) return;

        if (hitType == 1)
        {
            anim.ResetTrigger("Hit1");
            anim.SetTrigger("Hit1");
        }
        else
        {
            anim.ResetTrigger("Hit2");
            anim.SetTrigger("Hit2");
        }
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("!!! " + gameObject.name + " died!");

        anim.SetBool("IsDead", true);
        anim.SetTrigger("Death");
        StopMoving();

        if (healthBarCanvas != null)
            healthBarCanvas.gameObject.SetActive(false);

        float deathAnimTime = 3f;
        Destroy(gameObject, deathAnimTime);
    }

    // --------------------
    // DEBUG VISUALS
    // --------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
