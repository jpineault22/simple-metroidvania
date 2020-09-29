using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    public CharacterState CurrentCharacterState { get; private set; }

    #region Persistent variables

    public bool HasDash { get; set; }
    public bool HasWallJump { get; set; }
    public bool HasBomb { get; set; }
    public int AttackDamage { get; set; }

    #endregion

	#region Components

    public BoxCollider2D boxCollider;
	private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

	#endregion

	#region Game objects assignable in inspector

	[Header("Game objects/Layer masks")]
    [SerializeField] private Transform ceilingCheckLeft = default;
    [SerializeField] private Transform ceilingCheckRight = default;
    [SerializeField] private Transform wallCheckLeft = default;
    [SerializeField] private Transform wallCheckRight = default;
    [SerializeField] private LayerMask groundLayerMask = default;
    [SerializeField] private Transform attackPointRight = default;
    [SerializeField] private Transform attackPointLeft = default;
    [SerializeField] private Transform attackPointUp = default;
    [SerializeField] private Transform attackPointDown = default;
    [SerializeField] private LayerMask enemyLayerMask = default;
    [SerializeField] private GameObject bombPrefab = default;

    // Reference to the interactible game object in range
    private GameObject interactibleGameObject;

    #endregion

    #region Modifiable in inspector

    [Header("Jump settings")]
    [SerializeField] private float moveInputThreshold = 0.2f;
    [SerializeField] private float speed = 15f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float jumpTime = 0.35f;
    [SerializeField] private float bunkRadius = 0.1f;
    [SerializeField] private float groundedRadius = 0.2f;
    [SerializeField] private float minVerticalVelocity = -50f;                  // Minimum vertical velocity that the player can reach when falling
    [SerializeField] private float incompleteJumpFallVelocityDivider = 2f;      // When cancelling jump, divide current vertical velocity by this number

    [Header("Attack settings")]
    [SerializeField] private int baseAttackDamage = 1;
    //[SerializeField] private float attackPointPosition = 1.75f;
    [SerializeField] private float attackRange = 0.75f;
    [SerializeField] private float attackRate = 3f;                             // Times per second
    [SerializeField] private float recoilTime = 0.1f;
    [SerializeField] private float recoilSpeed = 10f;
    [SerializeField] private float pogoSpeed = 25f;

    [Header("Wall jump settings")]
    [SerializeField] private float wallJumpTime = 0.25f;
    [SerializeField] private float wallJumpVerticalForceDivider = 1.25f;

    [Header("Dash settings")]
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float dashTime = 0.15f;
    [SerializeField] private float dashRate = 3f;                               // Times per second
    [SerializeField] private int maxNumberOfDashes = 1;                         // Could add powerup to increase that number

    [Header("Bomb settings")]
    [SerializeField] private float bombVelocityX = 20f;
    [SerializeField] private float bombVelocityY = 20f;

    [Header("Map transition settings")]
    // These variables control the player's velocity changes when coming out of a map exit at the bottom.
    [SerializeField] private float upTransitionHorizontalVelocityIncrement = 60f;
    [SerializeField] private float upTransitionVerticalVelocityDecrement = 15f;

    [Header("Hit/Damage Boosting settings")]
    [SerializeField] private Vector2 hitForce = new Vector2(15f, 10f);
    [SerializeField] private float knockbackTime = 0.3f;                        // Duration of the knockback (Hit state, player does not have control)
    [SerializeField] private float damageBoostTime = 2f;                        // Duration of the Hit state (Other states, player has control)

    #endregion

    #region State variables

    // Input
    private PlayerControls controls;
    private float moveInput;

    // Walking/Jumping
    private bool facingRight;
    private bool isGrounded;
    private float jumpTimeCounter;

    // Attack
    private Direction attackDirection;
    private float nextAttackTime;
    private float recoilTimeCounter;

    // Wall jumping
    private float wallJumpTimeCounter;
    private int horizontalSpeedMultiplier;

    // Dashing
    private Direction dashDirection;
    private int numberOfDashes;
    private float dashTimeCounter;
    private float nextDashTime;

    // Bomb
    private float nextBombTime;

    // Map transition
    private MapTransitionDirection mapTransitionDirection;
    private int targetMapExitNumber;
    private bool transitionHalfDone;
    private float upTransitionHorizontalVelocity;
    private float upTransitionVerticalVelocity;

    // Hit/Damage Boosting
    private float hitCounter;
    private int hitDirection;
    private bool damageBoosting;

	#endregion


	#region MonoBehaviour methods

	protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        // Getting components
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Setting up controls methods
        controls = new PlayerControls();
        controls.Gameplay.Jump.performed += ctx => StartJump();
        controls.Gameplay.Jump.canceled += ctx => EndJump();
        controls.Gameplay.Attack.performed += ctx => StartAttack();
        controls.Gameplay.Interact.performed += ctx => StartInteraction();
        controls.Gameplay.Dash.performed += ctx => StartDash();
        controls.Gameplay.Bomb.performed += ctx => StartBomb();
        controls.Gameplay.Direction.performed += ctx => DetermineDirection(ctx.ReadValue<Vector2>());
        controls.Gameplay.Direction.canceled += ctx => ResetDirection();

        // Setting initial variable values
        AttackDamage = baseAttackDamage;
        facingRight = true;
        dashTimeCounter = dashTime;
        dashDirection = Direction.None;
        attackDirection = Direction.None;
        numberOfDashes = maxNumberOfDashes;
        upTransitionVerticalVelocity = jumpForce;
    }

	private void Start()
	{
        LevelLoader.Instance.TransitionHalfDone += OnTransitionHalfDone;
        LevelLoader.Instance.MapTransitionEnded += OnMapTransitionEnded;

        PlayerHealth.Instance.SetHUBActive(true);
    }

	private void Update()
    {
        if (GameManager.Instance.CurrentGameState == GameState.Playing)
		{
            if (CurrentCharacterState == CharacterState.Hit)
			{
                if (hitCounter <= 0)
				{
                    CurrentCharacterState = CharacterState.Idle;
                    damageBoosting = true;
                    hitCounter = damageBoostTime;
				}

                hitCounter -= Time.deltaTime;
			}
            else
			{
                if (damageBoosting)
				{
                    spriteRenderer.enabled = !spriteRenderer.enabled;

                    if (hitCounter <= 0)
                    {
                        damageBoosting = false;
                        spriteRenderer.enabled = true;
                    }

                    hitCounter -= Time.deltaTime;
                }

                // Get horizontal movement input
                moveInput = controls.Gameplay.Move.ReadValue<float>();

                CheckFlipX();
                CheckWalls();
            }
        }
    }

	private void FixedUpdate()
	{
        if (GameManager.Instance.CurrentGameState == GameState.Playing)
		{
            if (CurrentCharacterState == CharacterState.MapTransition)
            {
                ProcessMapTransition();
            }
            else if (CurrentCharacterState == CharacterState.Dashing)
            {
                ProcessDash();
            }
            else if (CurrentCharacterState == CharacterState.WallJumping)
            {
                ProcessWallJump();
            }
            else if (CurrentCharacterState != CharacterState.Hit)
            {
                CheckIfFalling();
                MoveHorizontally();
                SetIsGrounded();

                // Reset dashes
                if (isGrounded)
                {
                    numberOfDashes = maxNumberOfDashes;
                }

                if (CurrentCharacterState == CharacterState.Jumping)
                {
                    ProcessJump();
                }

                // Minimum cap for Y velocity when falling
                if (rb.velocity.y < minVerticalVelocity)
                {
                    rb.velocity = new Vector2(rb.velocity.x, minVerticalVelocity);
                }
            }
        }
	}

	#endregion

	#region Input system methods

	private void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    private void StartJump()
	{
        if (GameManager.Instance.CurrentGameState == GameState.Playing && CurrentCharacterState != CharacterState.MapTransition)
		{
            if (isGrounded)
            {
                CurrentCharacterState = CharacterState.Jumping;
                jumpTimeCounter = jumpTime;
            }
            else if (HasWallJump && (CurrentCharacterState == CharacterState.Jumping || CurrentCharacterState == CharacterState.Falling) && horizontalSpeedMultiplier != 0)
            {
                CurrentCharacterState = CharacterState.WallJumping;
                wallJumpTimeCounter = wallJumpTime;
            }
        }
	}

    private void EndJump()
	{
        if (CurrentCharacterState == CharacterState.Jumping)
		{
            CurrentCharacterState = CharacterState.Falling;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / incompleteJumpFallVelocityDivider);
        }
    }

    private void StartAttack()
	{
        if (GameManager.Instance.CurrentGameState == GameState.Playing && CurrentCharacterState != CharacterState.MapTransition)
        {
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + 1f / attackRate;

                switch (attackDirection)
                {
                    case Direction.Up:
                        animator.SetTrigger(Constants.AnimatorPlayerAttackUp);
                        break;
                    case Direction.Down:
                        animator.SetTrigger(Constants.AnimatorPlayerAttackDown);
                        break;
                    default:
                        animator.SetTrigger(Constants.AnimatorPlayerAttack);
                        break;
                }

                Vector2 attackPoint = facingRight ? attackPointRight.position : attackPointLeft.position;

                if (attackDirection == Direction.Up)
                {
                    attackPoint = attackPointUp.position;
                }
                else if (attackDirection == Direction.Down)
                {
                    attackPoint = attackPointDown.position;
                }

                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRange, enemyLayerMask);

                if (hitEnemies.Length != 0)
                {
                    recoilTimeCounter = recoilTime;
                }

                foreach (Collider2D enemyCollider in hitEnemies)
                {
                    Enemy enemy = enemyCollider.gameObject.GetComponentInParent<Enemy>();
                    enemy.GetHit(attackDirection, AttackDamage);
                }
            }
        }
	}

    private void StartInteraction()
	{
        if (GameManager.Instance.CurrentGameState == GameState.Playing && CurrentCharacterState != CharacterState.MapTransition)
        {
            if (interactibleGameObject != null)
            {
                InteractibleObject interactibleObject = interactibleGameObject.GetComponent<InteractibleObject>();
                interactibleObject.Interact();
            }
        }
	}

    private void StartDash()
	{
        if (GameManager.Instance.CurrentGameState == GameState.Playing && CurrentCharacterState != CharacterState.MapTransition)
        {
            if (HasDash && Time.time >= nextDashTime && CurrentCharacterState != CharacterState.Dashing)
            {
                if (numberOfDashes != 0)
                {
                    nextDashTime = Time.time + 1f / dashRate;
                    dashTimeCounter = dashTime;
                    CurrentCharacterState = CharacterState.Dashing;
                    numberOfDashes--;
                }
            }
        }
    }

    private void StartBomb()
	{
        if (GameManager.Instance.CurrentGameState == GameState.Playing && CurrentCharacterState != CharacterState.MapTransition)
        {
            if (HasBomb && Time.time >= nextBombTime)
            {
                nextBombTime = Time.time + Bomb.MaxLifetime;

                Transform spawnPoint;
                float velocityX;

                if (facingRight)
                {
                    spawnPoint = attackPointRight.transform;
                    velocityX = bombVelocityX;
                }
                else
                {
                    spawnPoint = attackPointLeft.transform;
                    velocityX = -bombVelocityX;
                }

                GameObject spawnedBomb = Instantiate(bombPrefab, spawnPoint);
                spawnedBomb.GetComponent<Rigidbody2D>().velocity = new Vector2(velocityX, bombVelocityY);
            }
        }
	}

    #endregion

    #region Action processing methods, called from FixedUpdate

    private void MoveHorizontally()
	{
        // Check if recoiling from player's attack
        if (recoilTimeCounter > 0)
		{
            recoilTimeCounter -= Time.fixedDeltaTime;

            if (attackDirection == Direction.Down)
			{
                rb.velocity = new Vector2(rb.velocity.x, pogoSpeed);
            }
            else if (attackDirection != Direction.Up)
			{
                int recoilSpeedMultiplier = facingRight ? -1 : 1;
                rb.velocity = new Vector2(recoilSpeed * recoilSpeedMultiplier, rb.velocity.y);
            }
		}
		else
		{
            // Process horizontal movement
            if (Mathf.Abs(moveInput) >= moveInputThreshold)
            {
                int direction = facingRight ? 1 : -1;
                rb.velocity = new Vector2(direction * speed, rb.velocity.y);

                if (isGrounded && CurrentCharacterState != CharacterState.Jumping)
                {
                    CurrentCharacterState = CharacterState.Walking;
                }
            }
            else
            {
                StopMovement();
            }
        }
	}

	private void ProcessJump()
	{
        // Check if player bunks on ceiling
        if (Physics2D.OverlapCircle(ceilingCheckLeft.position, bunkRadius, groundLayerMask) || Physics2D.OverlapCircle(ceilingCheckRight.position, bunkRadius, groundLayerMask))
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            CurrentCharacterState = CharacterState.Falling;
        }
        else if (jumpTimeCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpTimeCounter -= Time.fixedDeltaTime;
        }
        else
        {
            CurrentCharacterState = CharacterState.Falling;
        }
    }

    private void ProcessWallJump()
	{
        if (wallJumpTimeCounter > 0)
        {
            rb.velocity = new Vector2(jumpForce * horizontalSpeedMultiplier, jumpForce / wallJumpVerticalForceDivider);
            wallJumpTimeCounter -= Time.fixedDeltaTime;
        }
        else
        {
            CurrentCharacterState = CharacterState.Falling;
            horizontalSpeedMultiplier = 0;
        }
    }

	private void ProcessDash()
	{
		if (dashTimeCounter > 0)
		{
			dashTimeCounter -= Time.fixedDeltaTime;
			Vector2 dashVector = CreateDashVector();
			rb.velocity = dashVector;

			// Possibly add camera shake later
		}
		else
		{
			rb.velocity = Vector2.zero;
			CurrentCharacterState = CharacterState.Falling;
		}
	}

    private void ProcessMapTransition()
	{
        // Checking map transition direction
        if (mapTransitionDirection == MapTransitionDirection.Left || mapTransitionDirection == MapTransitionDirection.Right)
		{
            int direction = mapTransitionDirection == MapTransitionDirection.Right ? 1 : -1;
            rb.velocity = new Vector2(direction * speed, rb.velocity.y);
        }
        else if (mapTransitionDirection == MapTransitionDirection.Up)
		{
            // This would need some polishing so that the player's movement arc is more fluid
            if (transitionHalfDone)
            {
                upTransitionHorizontalVelocity += upTransitionHorizontalVelocityIncrement * Time.fixedDeltaTime;
                upTransitionVerticalVelocity -= upTransitionVerticalVelocityDecrement * Time.fixedDeltaTime;
            }

            int direction = facingRight ? 1 : -1;
            rb.velocity = new Vector2(upTransitionHorizontalVelocity * direction, upTransitionVerticalVelocity);
		}
        // Do nothing if going down, gravity will do the rest yay
	}

    private void MoveToTargetMapExit()
	{
        Transform targetMapExit = FindTargetMapExit();

        if (targetMapExit == null)
        {
            Debug.LogError("[PlayerController] Cannot find target map exit.");
            return;
        }

        transform.position = targetMapExit.position;
    }

    #endregion

    #region State update methods

    private void DetermineDirection(Vector2 pDirectionVector)
	{
        // Is there a better way to do this?
        if (Mathf.Abs(pDirectionVector.x) >= moveInputThreshold || Mathf.Abs(pDirectionVector.y) >= moveInputThreshold)
		{
            float angleRad = Mathf.Atan2(pDirectionVector.y, pDirectionVector.x);

            // Determine dash direction
            if (Mathf.Abs(angleRad) < Constants.AngleConstantFirst)
			{
                dashDirection = Direction.Right;
            }
            else if (Mathf.Abs(angleRad) < Constants.AngleConstantSecond)
			{
                dashDirection = angleRad > 0 ? Direction.UpperRight : Direction.LowerRight;
            }
            else if (Mathf.Abs(angleRad) < Constants.AngleConstantThird)
			{
                dashDirection = angleRad > 0 ? Direction.Up : Direction.Down;
            }
            else if (Mathf.Abs(angleRad) < Constants.AngleConstantFourth)
			{
                dashDirection = angleRad > 0 ? Direction.UpperLeft : Direction.LowerLeft;
            }
            else
			{
                dashDirection = Direction.Left;
            }

            // Determine attack direction
            if (Mathf.Abs(angleRad) <= Constants.AngleConstant45)
			{
                attackDirection = Direction.Right;
			}
            else if (Mathf.Abs(angleRad) < Constants.AngleConstant135)
			{
                if (angleRad > 0)
				{
                    attackDirection = Direction.Up;
				}
                else
				{
                    attackDirection = Direction.Down;
                }
            }
            else
			{
                attackDirection = Direction.Left;
            }
        }
        else
		{
            dashDirection = Direction.None;
		}
	}

    private void ResetDirection()
	{
        dashDirection = Direction.None;
        attackDirection = facingRight ? Direction.Right : Direction.Left;
    }

    private void SetIsGrounded()
	{
        Vector3 originLeft = new Vector3(boxCollider.bounds.center.x - boxCollider.bounds.extents.x, boxCollider.bounds.center.y, boxCollider.bounds.center.z);
        Vector3 originRight = new Vector3(boxCollider.bounds.center.x + boxCollider.bounds.extents.x, boxCollider.bounds.center.y, boxCollider.bounds.center.z);
        isGrounded = Physics2D.Raycast(originLeft, Vector2.down, boxCollider.bounds.extents.y + groundedRadius, groundLayerMask)
            || Physics2D.Raycast(originRight, Vector2.down, boxCollider.bounds.extents.y + groundedRadius, groundLayerMask);
    }

    private void CheckFlipX()
	{
        if (CurrentCharacterState == CharacterState.MapTransition)
		{
            if (mapTransitionDirection == MapTransitionDirection.Right)
			{
                facingRight = true;
			}
            else if (mapTransitionDirection == MapTransitionDirection.Left)
			{
                facingRight = false;
			}

            spriteRenderer.flipX = !facingRight;
		}
        else if (Mathf.Abs(moveInput) >= moveInputThreshold && recoilTimeCounter <= 0)
		{
            if (moveInput > 0)
            {
                spriteRenderer.flipX = false;
                facingRight = true;
            }
            else if (moveInput < 0)
            {
                spriteRenderer.flipX = true;
                facingRight = false;
            }
        }
	}

    private void CheckWalls()
	{
        // Check if there are walls to the sides and set the horizontal speed multiplier that will determine the direction of the wall jump
        if ((CurrentCharacterState == CharacterState.Falling || CurrentCharacterState == CharacterState.Jumping) && !isGrounded)
		{
            if (Physics2D.OverlapCircle(wallCheckLeft.position, groundedRadius, groundLayerMask))
            {
                horizontalSpeedMultiplier = 1;
            }
            else if (Physics2D.OverlapCircle(wallCheckRight.position, groundedRadius, groundLayerMask))
            {
                horizontalSpeedMultiplier = -1;
            }
            else
			{
                horizontalSpeedMultiplier = 0;
            }
        }
        else if (CurrentCharacterState != CharacterState.WallJumping)
		{
            horizontalSpeedMultiplier = 0;
		}
    }

    private void CheckIfFalling()
	{
        if (CurrentCharacterState != CharacterState.Jumping && !isGrounded && CurrentCharacterState != CharacterState.MapTransition)
		{
            CurrentCharacterState = CharacterState.Falling;
		}
	}

    private Vector2 CreateDashVector()
	{
        Vector2 dashVector = Vector2.zero;

        // Redesign this section, it's much more redundant than with the initial direction system
        switch(dashDirection)
		{
            case Direction.Up:
                dashVector.y = dashSpeed;
                break;
            case Direction.Right:
                dashVector.x = dashSpeed;
                break;
            case Direction.Down:
                dashVector.y = -dashSpeed;
                break;
            case Direction.Left:
                dashVector.x = -dashSpeed;
                break;
            case Direction.UpperRight:
                dashVector.y = dashSpeed;
                dashVector.x = dashSpeed;
                break;
            case Direction.LowerRight:
                dashVector.x = dashSpeed;
                dashVector.y = -dashSpeed;
                break;
            case Direction.LowerLeft:
                dashVector.y = -dashSpeed;
                dashVector.x = -dashSpeed;
                break;
            case Direction.UpperLeft:
                dashVector.x = -dashSpeed;
                dashVector.y = dashSpeed;
                break;
            case Direction.None:
                if (facingRight)
                {
                    dashVector.x = dashSpeed;
                }
                else
                {
                    dashVector.x = -dashSpeed;
                }
                break;
        }

        return dashVector;
	}

    private Transform FindTargetMapExit()
	{
        Transform targetMapExit = LevelLoader.Instance.CurrentFunctionalMap.transform.Find(Constants.NamePrefixMapExit + targetMapExitNumber);

        return targetMapExit;
    }

	#endregion

	#region Collisions

	private void OnTriggerEnter2D(Collider2D pCollision)
	{
        // Check if interactible object to display message
        if (pCollision.gameObject.layer == LayerMask.NameToLayer(Constants.LayerInteractibleObject))
        {
            interactibleGameObject = pCollision.gameObject;
        }

        // Check if powerup and collect
        if (pCollision.gameObject.CompareTag(Constants.TagDashPowerup))
		{
            HasDash = true;
            CollectPowerup(pCollision.gameObject);
		}
        else if (pCollision.gameObject.CompareTag(Constants.TagWallJumpPowerup))
		{
            HasWallJump = true;
            CollectPowerup(pCollision.gameObject);
        }
        else if (pCollision.gameObject.CompareTag(Constants.TagBombPowerup))
		{
            HasBomb = true;
            CollectPowerup(pCollision.gameObject);
        }
        // Check if heart and collect
        else if (pCollision.gameObject.CompareTag(Constants.TagHeart))
		{
            PlayerHealth.Instance.IncrementHearts();
            CollectPowerup(pCollision.gameObject);
		}
    }

	private void OnTriggerStay2D(Collider2D pCollision)
	{
        // If the collision is an enemy and the character is not already hit or damage boosting, get hit
        if (pCollision.gameObject.layer == LayerMask.NameToLayer(Constants.LayerFunctionalEnemy)
            && CurrentCharacterState != CharacterState.Hit && !damageBoosting)
		{
            GetHit(pCollision.gameObject);
		}

    }

	private void OnTriggerExit2D(Collider2D pCollision)
	{
        if (pCollision.gameObject.layer == LayerMask.NameToLayer(Constants.LayerInteractibleObject))
        {
            interactibleGameObject = null;
        }
    }

    private void GetHit(GameObject pEnemyGameObject)
	{
        CurrentCharacterState = CharacterState.Hit;
        Enemy enemy = pEnemyGameObject.GetComponentInParent<Enemy>();
        PlayerHealth.Instance.TakeDamage(enemy.enemyData.attackDamage);

        hitDirection = pEnemyGameObject.transform.position.x >= transform.position.x ? -1 : 1;
        rb.velocity = new Vector2(hitDirection * hitForce.x, hitForce.y);
        hitCounter = knockbackTime;
	}

	#endregion

	#region Methods called by external objects

	public void SetStateToMapTransition(MapTransitionDirection pMapTransitionDirection, int pTargetMapExitNumber)
	{
        CurrentCharacterState = CharacterState.MapTransition;
        mapTransitionDirection = pMapTransitionDirection;
        targetMapExitNumber = pTargetMapExitNumber;
	}

    public void SetStateToIdle()
	{
        CurrentCharacterState = CharacterState.Idle;
	}

    public void StopMovement()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);

        if (isGrounded && CurrentCharacterState != CharacterState.Jumping)
        {
            CurrentCharacterState = CharacterState.Idle;
        }
    }

    #endregion

    #region Event handlers

    private void OnTransitionHalfDone()
	{
        MoveToTargetMapExit();
        transitionHalfDone = true;
    }

    private void OnMapTransitionEnded()
	{
        CurrentCharacterState = CharacterState.Idle;
        transitionHalfDone = false;
        upTransitionHorizontalVelocity = 0;
        upTransitionVerticalVelocity = jumpForce;
    }

	#endregion

	#region Gizmos

	private void OnDrawGizmosSelected()
	{
        if (attackPointRight == null)
		{
            return;
        }

        // Drawing only the right attack point to get an idea of the range without cluttering the space around the player
        Gizmos.DrawWireSphere(attackPointRight.position, attackRange);
	}

	#endregion

	#region To be moved in other class

    // This method could be in some kind of item manager
    private void CollectPowerup(GameObject pGameObject)
	{
        // Play "powerup collected" animation
        
        Destroy(pGameObject);
	}

	#endregion

	protected override void OnDestroy()
	{
        if (PlayerHealth.IsInitialized)
		{
            PlayerHealth.Instance.SetHUBActive(false);
        }
        
        if (LevelLoader.IsInitialized)
		{
            LevelLoader.Instance.TransitionHalfDone -= OnTransitionHalfDone;
            LevelLoader.Instance.MapTransitionEnded -= OnMapTransitionEnded;
        }
		
        base.OnDestroy();
    }
}

public enum CharacterState
{
    Idle,
    Walking,
    Jumping,
    Falling,
    WallSliding,        // Not implemented (for animation)
    WallJumping,
    Dashing,
    Hit,
    MapTransition       // Consider implementing as a GameState instead
}

public enum Direction
{
    None,
    Up,
    Right,
    Down,
    Left,
    UpperRight,
    LowerRight,
    LowerLeft,
    UpperLeft
}