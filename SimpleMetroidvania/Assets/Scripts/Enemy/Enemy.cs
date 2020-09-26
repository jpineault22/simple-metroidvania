using UnityEngine;

// This class has not been fully implemented yet
public abstract class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData = default;

    protected abstract void Attack();

    protected abstract void Die();
}
