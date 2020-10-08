using UnityEngine;

public class GroundDestructible : HittableObject
{
    [SerializeField] private float maxHits = 3;

    private float hitsLeft;

	private void Awake()
	{
		hitsLeft = maxHits;
	}

	private void DestroyGround()
	{
		// Animate destruction
		Destroy(gameObject);
	}

	public override void GetHit(Direction pAttackDirection, int pDamage)
	{
		hitsLeft--;

		if (hitsLeft <= 0)
		{
			DestroyGround();
		}

		// Ground hit animation
	}
}
