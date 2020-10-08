using UnityEngine;

public abstract class Enemy : HittableObject
{
    [SerializeField] public EnemyData enemyData = default;

    protected EnemyState currentEnemyState;
    protected float currentStateTimer;
    protected bool facingRight;
    protected int currentHealth;

    protected Collider2D externalCollider;

	protected virtual void Awake()
	{
        currentHealth = enemyData.maxHealth;
	}

	protected abstract void Move();

    protected abstract void Attack();

    protected abstract void Die();

    protected abstract void Flip();

    protected void TakeDamage(int pDamage)
	{
        currentHealth -= pDamage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }
}

// When implementing more enemy types, see if I split this enum and move it into derived classes
public enum EnemyState
{
    Idle,
    Walking,
    Chasing,
    ChasingIdle,         // When the enemy is chasing the player and is unable to keep chasing them (ex: they reach the edge of a platform), they become ChasingIdle
    Hit,
    Falling
}