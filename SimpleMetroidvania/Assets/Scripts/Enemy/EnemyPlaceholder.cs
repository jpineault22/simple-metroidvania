using JetBrains.Annotations;
using UnityEngine;

// Refine to make behavior more fluid (ex: reaction time)
// See if it would be better to add another superclass for enemy types that would have a different overall behavior (Ground, Air, etc.)
public class EnemyPlaceholder : Enemy
{
	#region SerializeFields

	[Header("Game objects/Layer masks")]
	[SerializeField] private Transform playerDetection = default;
	[SerializeField] private Transform groundDetection = default;
	[SerializeField] private LayerMask groundLayerMask = default;

	[Header("Walking/Idle settings")]
	[SerializeField] private float walkingSpeed = 3f;
	[SerializeField] private float walkingTimeMin = 3f;                         // The minimum time the enemy will walk
	[SerializeField] private float walkingTimeMax = 8f;							// The maximum time the enemy will walk
	[SerializeField] private float additionalWalkingTimeChance = 0.5f;			// The chance [0, 1] that additional time will be added to the walking timer when getting a high timer
	[SerializeField] private float idleTimeMin = 2f;							// The minimum time the enemy will be idle
	[SerializeField] private float idleTimeMax = 8f;							// The maximum time the enemy will be idle
	[SerializeField] private float flipUponWalkChance = 0.35f;                  // The chance [0, 1] that the enemy will change direction when starting to walk
	[SerializeField] private float walkUponStartChance = 0.5f;                  // The chance [0, 1] that the enemy will start walking when spawned
	[SerializeField] private float groundDetectionRaycastDistance = 1f;			// The distance of the raycast detecting the ground
	[SerializeField] private float wallDetectionRaycastDistance = 0.1f;         // The distance of the raycast detecting walls
	[SerializeField] private float groundedRadius = 0.2f;

	[Header("Chasing settings")]
	[SerializeField] private float chasingSpeed = 7f;
	[SerializeField] private float playerDetectionRaycastDistance = 10f;        // The distance of the "line of sight" raycast detecting the player
	[SerializeField] private float exitChasingDistance = 20f;					// The distance the player needs to be from the enemy for it to start chasing them
	[SerializeField] private float verticalExitChasingDistance = 3f;            // The vertical distance the player needs to be from the enemy for it to start chasing them
	[SerializeField] private float chasingIdleMinTime = 1f;						// The minimum time the enemy will be ChasingIdle
	[SerializeField] private float chasingIdleMaxTime = 5f;                     // The maximum time the enemy will be ChasingIdle
	[SerializeField] private float chasingFlipTimeMin = 1f;						// The minimum time between each flip when chasing

	[Header("Hit settings")]
	[SerializeField] private Vector2 hitForce = new Vector2(15f, 0f);
	[SerializeField] private float hitTime = 0.5f;

	#endregion

	#region Components

	private Transform player;
	private Rigidbody2D rb;

	#endregion

	#region Other variables

	private bool isGrounded;
	private float chasingFlipTimer;
	private int hitDirection;

	#endregion

	#region MonoBehaviour methods

	protected override void Awake()
	{
		base.Awake();

		rb = GetComponent<Rigidbody2D>();
		externalCollider = GetComponent<BoxCollider2D>();
	}

	private void Start()
	{
		// Change all Player game object getters for this for efficiency and consistency?
		player = PlayerController.Instance.gameObject.transform;

		if (Random.Range(0f, 1f) <= walkUponStartChance)
		{
			StartWalking();
		}
		else
		{
			BecomeIdle();
		}
	}

	private void Update()
	{
		// Basic states (Walking, Idle)
		if (currentEnemyState == EnemyState.Walking || currentEnemyState == EnemyState.Idle)
		{
			// If the enemy sees the player, it starts chasing them
			Vector2 raycastDirection = facingRight ? Vector2.right : Vector2.left;
			RaycastHit2D lineOfSight = Physics2D.Raycast(playerDetection.position, raycastDirection, playerDetectionRaycastDistance, LayerMask.GetMask(Constants.LayerPlayer));

			if (lineOfSight.collider)
			{
				StartChasing();
			}

			// When the state timer reaches 0, follow the corresponding behavior
			if (currentStateTimer <= 0)
			{
				if (currentEnemyState == EnemyState.Idle)
				{
					StartWalking();
				}
				else if (currentEnemyState == EnemyState.Walking)
				{
					BecomeIdle();
				}
			}
		}
		// Hit state
		else if (currentEnemyState == EnemyState.Hit)
		{
			if (currentStateTimer <= 0)
			{
				currentEnemyState = EnemyState.Chasing;
				CheckChasingDirection();
			}
		}
		// Chasing states
		else if (currentEnemyState == EnemyState.Chasing || currentEnemyState == EnemyState.ChasingIdle)
		{
			// If the distance between the player and the enemy is big enough, the enemy stops chasing the player
			if (Vector2.Distance(transform.position, player.position) > exitChasingDistance)
			{
				BecomeIdle();
			}

			// If the the enemy is not already ChasingIdle
			// AND the vertical distance between the player and the enemy is big enough
			// AND the enemy is almost directly aligned vertically with the player
			// In other words, if the player is above or below the enemy => the enemy becomes ChasingIdle
			if (currentEnemyState != EnemyState.ChasingIdle
				&& Mathf.Abs(transform.position.y - player.position.y) > verticalExitChasingDistance
				&& Mathf.Abs(transform.position.x - player.position.x) < 0.1f)
			{
				BecomeChasingIdle(chasingIdleMinTime);
			}

			if (currentEnemyState == EnemyState.Chasing)
			{
				chasingFlipTimer -= Time.deltaTime;
			}
			else if (currentEnemyState == EnemyState.ChasingIdle)
			{
				if (currentStateTimer <= 0)
				{
					StartWalking();
				}
			}
		}

		currentStateTimer -= Time.deltaTime;
	}

	private void FixedUpdate()
	{
		SetIsGrounded();
		Move();
		CheckIfFalling();
	}

	#endregion

	#region Action processing methods, (called from FixedUpdate)

	protected override void Move()
	{
		if (currentEnemyState == EnemyState.Walking || currentEnemyState == EnemyState.Chasing)
		{
			int direction = facingRight ? 1 : -1;

			RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, groundDetectionRaycastDistance, LayerMask.GetMask(Constants.LayerGround));
			RaycastHit2D wallInfo = Physics2D.Raycast(groundDetection.position, new Vector2(direction, 0), wallDetectionRaycastDistance, LayerMask.GetMask(Constants.LayerGround));

			switch (currentEnemyState)
			{
				case EnemyState.Walking:

					rb.velocity = new Vector2(direction * walkingSpeed, rb.velocity.y);

					if (!groundInfo.collider || wallInfo.collider)
					{
						Flip();
					}

					break;

				case EnemyState.Chasing:

					int chasingDirection = CheckChasingDirection();

					if (!groundInfo.collider || wallInfo.collider)
					{
						BecomeChasingIdle(chasingIdleMaxTime);
						break;
					}

					rb.velocity = new Vector2(chasingDirection * chasingSpeed, rb.velocity.y);

					break;
			}
		}
	}

	protected override void Attack()
	{
		// This enemy does not attack
	}

	protected override void Die()
	{
		// Play death animation

		Destroy(gameObject);
	}

	public override void GetHit(Direction pAttackDirection, int pDamage)
	{
		currentEnemyState = EnemyState.Hit;
		TakeDamage(pDamage);

		if (pAttackDirection == Direction.Right)
		{
			hitDirection = 1;
		}
		else if (pAttackDirection == Direction.Left)
		{
			hitDirection = -1;
		}
		else
		{
			hitDirection = 0;
		}

		rb.velocity = new Vector2(hitDirection * hitForce.x, hitForce.y);
		currentStateTimer = hitTime;
	}

	#endregion

	#region State update methods

	private void SetIsGrounded()
	{
		Vector3 originLeft = new Vector3(externalCollider.bounds.center.x - externalCollider.bounds.extents.x, externalCollider.bounds.center.y, externalCollider.bounds.center.z);
		Vector3 originRight = new Vector3(externalCollider.bounds.center.x + externalCollider.bounds.extents.x, externalCollider.bounds.center.y, externalCollider.bounds.center.z);
		isGrounded = Physics2D.Raycast(originLeft, Vector2.down, externalCollider.bounds.extents.y + groundedRadius, groundLayerMask)
			|| Physics2D.Raycast(originRight, Vector2.down, externalCollider.bounds.extents.y + groundedRadius, groundLayerMask);
	}

	private void BecomeIdle()
	{
		currentEnemyState = EnemyState.Idle;
		currentStateTimer = Random.Range(idleTimeMin, idleTimeMax);
	}

	private void StartWalking()
	{
		currentEnemyState = EnemyState.Walking;
		currentStateTimer = Random.Range(walkingTimeMin, walkingTimeMax);

		// If the time is high, there is a chance that additional time will be added
		if (currentStateTimer >= walkingTimeMax - 1 && Random.Range(0f, 1f) <= additionalWalkingTimeChance)
		{
			currentStateTimer += Random.Range(0f, walkingTimeMax);
		}

		if (Random.Range(0f, 1f) <= flipUponWalkChance)
		{
			Flip();
		}
	}
	
	private void StartChasing()
	{
		currentEnemyState = EnemyState.Chasing;
	}

	private void BecomeChasingIdle(float pChasingIdleTime)
	{
		currentEnemyState = EnemyState.ChasingIdle;
		currentStateTimer = pChasingIdleTime;
		rb.velocity = Vector2.zero;
	}

	private int CheckChasingDirection()
	{
		int chasingDirection = facingRight ? 1 : -1;

		if (chasingFlipTimer <= 0)
		{
			chasingDirection = transform.position.x <= player.position.x ? 1 : -1;

			if ((chasingDirection == 1 && !facingRight || chasingDirection == -1 && facingRight))
			{
				Flip();
				chasingFlipTimer = chasingFlipTimeMin;
			}
		}

		return chasingDirection;
	}

	private void CheckIfFalling()
	{
		if (!isGrounded)
		{
			currentEnemyState = EnemyState.Falling;
		}
		else if (currentEnemyState == EnemyState.Falling)
		{
			currentEnemyState = EnemyState.Idle;
		}
	}

	protected override void Flip()
	{
		if (facingRight)
		{
			transform.eulerAngles = new Vector3(0, 0, 0);
			facingRight = false;
		}
		else
		{
			transform.eulerAngles = new Vector3(0, 180, 0);
			facingRight = true;
		}
	}

	#endregion
}
