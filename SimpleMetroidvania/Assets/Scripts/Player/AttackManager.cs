using UnityEngine;

public class AttackManager : Singleton<AttackManager>
{
    public int AttackDamage { get; set; }
    public Direction AttackDirection { get; set; }

    [Header("Game objects/Layer masks")]
    [SerializeField] public Transform attackPointRight = default;
    [SerializeField] public Transform attackPointLeft = default;
    [SerializeField] private Transform attackPointUp = default;
    [SerializeField] private Transform attackPointDown = default;
    [SerializeField] private LayerMask hittableObjectsLayerMask = default;

    [Header("Attack settings")]
    [SerializeField] private int baseAttackDamage = 1;
    //[SerializeField] private float attackPointPosition = 1.75f;
    [SerializeField] private float attackRange = 0.75f;
    [SerializeField] private float attackRate = 3f;                             // Times per second
    [SerializeField] private float recoilTime = 0.1f;
    [SerializeField] private float recoilSpeed = 10f;
    [SerializeField] private float pogoSpeed = 25f;

    private float nextAttackTime;
    private float recoilTimeCounter;

	protected override void Awake()
	{
		base.Awake();

        AttackDamage = baseAttackDamage;
        AttackDirection = Direction.None;
    }

    public void StartAttack(bool facingRight)
	{
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + 1f / attackRate;

            switch (AttackDirection)
            {
                case Direction.Up:
                    PlayerController.Instance.SetAnimatorTrigger(Constants.AnimatorPlayerAttackUp);
                    break;
                case Direction.Down:
                    PlayerController.Instance.SetAnimatorTrigger(Constants.AnimatorPlayerAttackDown);
                    break;
                default:
                    PlayerController.Instance.SetAnimatorTrigger(Constants.AnimatorPlayerAttack);
                    break;
            }

            Vector2 attackPoint = facingRight ? attackPointRight.position : attackPointLeft.position;

            if (AttackDirection == Direction.Up)
            {
                attackPoint = attackPointUp.position;
            }
            else if (AttackDirection == Direction.Down)
            {
                attackPoint = attackPointDown.position;
            }

            Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint, attackRange, hittableObjectsLayerMask);

            if (hitObjects.Length != 0)
            {
                recoilTimeCounter = recoilTime;
            }

            foreach (Collider2D collider in hitObjects)
            {
                HittableObject hittableObject = collider.gameObject.CompareTag(Constants.TagEnemy)
                    ? collider.gameObject.GetComponentInParent<Enemy>()
                    : collider.gameObject.GetComponent<HittableObject>();

                hittableObject.GetHit(AttackDirection, AttackDamage);
            }
        }
    }

    public Vector2 CheckRecoil(bool facingRight, Vector2 velocity)
	{
        if (recoilTimeCounter > 0)
        {
            recoilTimeCounter -= Time.fixedDeltaTime;

            if (AttackDirection == Direction.Down)
            {
                return new Vector2(velocity.x, pogoSpeed);
            }
            else if (AttackDirection != Direction.Up)
            {
                int recoilSpeedMultiplier = facingRight ? -1 : 1;
                return new Vector2(recoilSpeed * recoilSpeedMultiplier, velocity.y);
            }
        }

        return Vector2.zero;
    }

    public float GetRecoilTimeCounter()
	{
        return recoilTimeCounter;
	}

    private void OnDrawGizmosSelected()
    {
        if (attackPointRight == null)
        {
            return;
        }

        // Drawing only the right attack point to get an idea of the range without cluttering the space around the player
        Gizmos.DrawWireSphere(attackPointRight.position, attackRange);
    }
}
