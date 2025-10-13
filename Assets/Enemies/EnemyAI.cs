using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 3f;
    public float damage = 15f;
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float attackRate = 1f;

    private Animator animator;
    private Transform playerTarget;
    private NavMeshAgent agent;
    private float nextAttackTime = 0f;
    private bool isAlerted = false;

    // Reference to health so we can check death state
    private EnemyHealth enemyHealth;

    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange;
            agent.updateRotation = false; // we handle rotation manually
        }
    }

    void Update()
    {
        // Stop AI completely if enemy is dead
        if (enemyHealth != null && enemyHealth.IsDead)
            return;

        if (playerTarget == null) return;

        // Make sure agent is usable before giving commands
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        animator.SetFloat("Speed", agent.velocity.magnitude);

        if (distanceToPlayer <= detectionRange)
        {
            if (!isAlerted)
            {
                isAlerted = true;
                agent.isStopped = true;
                animator.SetTrigger("Alert");
                return;
            }

            // If within attack range → stop and face player
            if (distanceToPlayer <= attackRange)
            {
                agent.isStopped = true;
                FacePlayer();

                if (Time.time >= nextAttackTime)
                {
                    animator.SetBool("IsAttacking", true);
                    nextAttackTime = Time.time + attackRate;
                }
            }
            else
            {
                // Chase player (only if navmesh is valid)
                agent.isStopped = false;
                agent.SetDestination(playerTarget.position);
                animator.SetBool("IsAttacking", false);
            }
        }
        else
        {
            // Out of detection range → idle
            agent.isStopped = true;
            agent.ResetPath();
            isAlerted = false;
            animator.SetBool("IsAttacking", false);
        }
    }

    private void FacePlayer()
    {
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;
        if (direction.magnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 8f);
        }
    }

    // Animation event
    public void TriggerAttackDamage()
    {
        if (enemyHealth != null && enemyHealth.IsDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerHealth playerHealth = playerTarget.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
