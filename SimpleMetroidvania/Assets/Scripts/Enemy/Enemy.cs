using UnityEngine;

// This class has not been fully implemented yet
public abstract class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    protected abstract void Attack();
}
