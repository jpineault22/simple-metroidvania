using UnityEngine;

public class Bomb : MonoBehaviour
{
	// Serves as a maximum life time in seconds as well as a cooldown used in PlayerController
	public static float MaxLifetime { get; set; } = 2f;
	public static int AttackDamage { get; private set; } = 2;

	[SerializeField] private LayerMask bombableGroundLayerMask = default;
	[SerializeField] private LayerMask enemyLayerMask = default;
	[SerializeField] private float bombRange = 3f;

	private float lifetime;

	private void Update()
	{
		lifetime += Time.deltaTime;

		if (lifetime >= MaxLifetime)
		{
			Explode();
		}
	}

	private void OnCollisionEnter2D(Collision2D pCollision)
	{
		if (((1 << pCollision.gameObject.layer) & (bombableGroundLayerMask | enemyLayerMask)) != 0)
		{
			Explode();
		}
	}

	private void Explode()
	{
		Collider2D[] hitObjects = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), bombRange, (bombableGroundLayerMask | enemyLayerMask));

		foreach (Collider2D collider in hitObjects)
		{
			if (((1 << collider.gameObject.layer) & bombableGroundLayerMask) != 0)
			{
				GroundBombable groundBombable = collider.gameObject.GetComponent<GroundBombable>();
				groundBombable.BombGround();
			}
			else if (((1 << collider.gameObject.layer) & enemyLayerMask) != 0)
			{
				Direction hitDirection = transform.position.x < collider.gameObject.transform.position.x ? Direction.Right : Direction.Left;

				Enemy enemy = collider.gameObject.GetComponentInParent<Enemy>();
				enemy.GetHit(hitDirection, AttackDamage);
			}
		}

		// Animate explosion

		Destroy(gameObject);
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(transform.position, bombRange);
	}
}
