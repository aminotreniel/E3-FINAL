using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    public float currentHealth;
    public int pointsOnDeath = 100; // 👈 score reward

    [Header("Hit Flash")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.15f;

    private Animator animator;
    private NavMeshAgent agent;
    private Collider mainCollider;
    private bool isDead = false;
    private Coroutine flashRoutine;

    public bool IsDead => isDead;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        mainCollider = GetComponent<Collider>();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitFlash()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color[][] originalColors = new Color[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = new Color[renderers[i].materials.Length];
            for (int j = 0; j < renderers[i].materials.Length; j++)
            {
                if (renderers[i].materials[j].HasProperty("_Color"))
                    originalColors[i][j] = renderers[i].materials[j].color;

                renderers[i].materials[j].color = hitColor;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            for (int j = 0; j < renderers[i].materials.Length; j++)
            {
                if (renderers[i].materials[j].HasProperty("_Color"))
                    renderers[i].materials[j].color = originalColors[i][j];
            }
        }

        flashRoutine = null;
    }

    private void Die()
    {
        isDead = true;

        if (agent != null && agent.enabled) agent.enabled = false;
        if (mainCollider != null) mainCollider.enabled = false;

        if (animator != null)
        {
            animator.applyRootMotion = true;
            animator.SetTrigger("Die");
        }

        // 🟢 Award points here
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(pointsOnDeath);
        }

        Destroy(gameObject, 3f);
    }
}
