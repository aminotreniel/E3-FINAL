using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private EnemyAI enemyAI;

    void Start()
    {
        enemyAI = GetComponentInParent<EnemyAI>(); // find the parent script
    }

    public void TriggerAttackDamage()
    {
        if (enemyAI != null)
            enemyAI.TriggerAttackDamage();
    }
}
