using UnityEngine;
using UnityEngine.AI;

public class AIDogFollow : MonoBehaviour
{
    public Transform player;
    public float followDistance = 2f;
    public float runSpeed = 6f;
    public float walkSpeed = 3f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance > followDistance)
        {
            agent.SetDestination(player.position);
            agent.speed = (distance > 5f) ? runSpeed : walkSpeed;
        }
        else
        {
            agent.ResetPath(); // Stop moving when close
        }
    }
}
