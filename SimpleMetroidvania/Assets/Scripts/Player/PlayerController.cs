using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>
{
    public CharacterState CurrentCharacterState { get; set; }

    #region Persistent variables

    public bool HasDash { get; set; }
    public bool HasWallJump { get; set; }
    public bool HasBomb { get; set; }

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
    private float moveInput;

    // Walking/Jumping
    private bool facingRight;
    private bool isGrounded;
    private float jumpTimeCounter;

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

        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        facingRight = true;
        upTransitionVerticalVelocity = jumpForce;
    }

	private void Start()
	{
        LevelLoader.Instance.TransitionHalfDone += OnTransitionHalfDone;
        LevelLoader.Instance.MapTransitionEnded += OnMapTransitionEnded;
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
                rb.velocity = DashManager.Instance.ProcessDash(facingRight);
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

                if (isGrounded)
                {
                    DashManager.Instance.ResetDashes();
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
        InputManager.Instance.Moved += ctx => UpdateMoveInput(ctx);
        InputManager.Instance.StartedJump += StartJump;
        InputManager.Instance.EndedJump += EndJump;
        InputManager.Instance.StartedAttack += StartAttack;
        InputManager.Instance.StartedInteraction += StartInteraction;
        InputManager.Instance.StartedDash += StartDash;
        InputManager.Instance.StartedBomb += StartBomb;
    }

    private void OnDisable()
    {
        InputManager.Instance.Moved -= ctx => UpdateMoveInput(ctx);
        InputManager.Instance.StartedJump -= StartJump;
        InputManager.Instance.EndedJump -= EndJump;
        InputManager.Instance.StartedAttack -= StartAttack;
        InputManager.Instance.StartedInteraction -= StartInteraction;
        InputManager.Instance.StartedDash -= StartDash;
        InputManager.Instance.StartedBomb -= StartBomb;
    }

    private void UpdateMoveInput(InputAction.CallbackContext ctx)
	{
        if (this != null)
		{
            Vector2 moveVector = ctx.ReadValue<Vector2>();
            moveInput = moveVector.x;

            if (moveVector == Vector2.zero)
			{
                ResetDirection();
			}
            else
			{
                DetermineDirection(moveVector);
			}
        }
    }

    private void StartJump()
	{
        if (this != null && GameManager.Instance.CurrentGameState == GameState.Playing && CurrentCharacterState != CharacterState.MapTransition)
		{
            if (isGrounded)
            {
                CurrentCharacterState = CharacterState.Jumping;
                jumpTimeCounter = jumpTime;
            }
            else if (HasWallJump && (CurrentCharacterState == CharacterState.Jumping || CurrentCharacterState == CharacterState.Falling))
            {
                WallJumpManager.Instance.StartWallJump();
            }
        }
	}

    private void EndJump()
	{
        if (this != null && CurrentCharacterState == CharacterState.Jumping)
		{
            CurrentCharacterState = CharacterState.Falling;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / incompleteJumpFallVelocityDivider);
        }
    }

    private void StartAttack()
	{
        if (this != null && GameManager.Instance.CurrentGameState == GameState.Playing && CurrentCharacterState != CharacterState.MapTransition)
        {
            AttackManager.Instance.StartAttack(facingRight);
        }
	}

    private void StartInteraction()
	{
        if (this != null && GameManager.Instance.CurrentGameState == GameState.Playing && CurrentCharacterState != CharacterState.MapTransition)
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
        if (this != null && GameManager.Instance.CurrentGameState == GameState.Playing && CurrentCharacterState != CharacterState.MapTransition)
        {
            if (HasDash && CurrentCharacterState != CharacterState.Dashing)
            {
                DashManager.Instance.StartDash();
            }
        }
    }

    private void StartBomb()
	{
        if (this != null && GameManager.Instance.CurrentGameState == GameState.Playing && CurrentCharacterState != CharacterState.MapTransition)
        {
            if (HasBomb)
            {
                BombManager.Instance.StartBomb(facingRight);
            }
        }
	}

    #endregion

    #region Action processing methods, called from FixedUpdate

    private void MoveHorizontally()
	{
        Vector2 newVelocity = AttackManager.Instance.CheckRecoil(facingRight, rb.velocity);
        
        if (newVelocity != Vector2.zero)
		{
            rb.velocity = newVelocity;
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
        if (CheckIfCeilingBunk())
        {
            return;
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
        if (CheckIfCeilingBunk())
        {
            return;
        }
        else
        {
            Vector2 newVelocity = WallJumpManager.Instance.ProcessWallJump(jumpForce);

            if (newVelocity != Vector2.zero)
			{
                rb.velocity = newVelocity;
			}
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
                DashManager.Instance.DashDirection = Direction.Right;
            }
            else if (Mathf.Abs(angleRad) < Constants.AngleConstantSecond)
			{
                DashManager.Instance.DashDirection = angleRad > 0 ? Direction.UpperRight : Direction.LowerRight;
            }
            else if (Mathf.Abs(angleRad) < Constants.AngleConstantThird)
			{
                DashManager.Instance.DashDirection = angleRad > 0 ? Direction.Up : Direction.Down;
            }
            else if (Mathf.Abs(angleRad) < Constants.AngleConstantFourth)
			{
                DashManager.Instance.DashDirection = angleRad > 0 ? Direction.UpperLeft : Direction.LowerLeft;
            }
            else
			{
                DashManager.Instance.DashDirection = Direction.Left;
            }

            // Determine attack direction
            if (Mathf.Abs(angleRad) <= Constants.AngleConstant45)
			{
                AttackManager.Instance.AttackDirection = Direction.Right;
			}
            else if (Mathf.Abs(angleRad) < Constants.AngleConstant135)
			{
                if (angleRad > 0)
				{
                    AttackManager.Instance.AttackDirection = Direction.Up;
				}
                else
				{
                    AttackManager.Instance.AttackDirection = Direction.Down;
                }
            }
            else
			{
                AttackManager.Instance.AttackDirection = Direction.Left;
            }
        }
        else
		{
            DashManager.Instance.DashDirection = Direction.None;
		}
	}

    private void ResetDirection()
	{
        DashManager.Instance.DashDirection = Direction.None;
        AttackManager.Instance.AttackDirection = facingRight ? Direction.Right : Direction.Left;
    }

    private void SetIsGrounded()
	{
        Vector3 originLeft = new Vector3(boxCollider.bounds.center.x - boxCollider.bounds.extents.x, boxCollider.bounds.center.y, boxCollider.bounds.center.z);
        Vector3 originRight = new Vector3(boxCollider.bounds.center.x + boxCollider.bounds.extents.x, boxCollider.bounds.center.y, boxCollider.bounds.center.z);
        isGrounded = Physics2D.Raycast(originLeft, Vector2.down, boxCollider.bounds.extents.y + groundedRadius, groundLayerMask)
            || Physics2D.Raycast(originRight, Vector2.down, boxCollider.bounds.extents.y + groundedRadius, groundLayerMask);
    }

    public void SetAnimatorTrigger(string triggerName)
	{
        animator.SetTrigger(triggerName);
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
        else if (Mathf.Abs(moveInput) >= moveInputThreshold && AttackManager.Instance.GetRecoilTimeCounter() <= 0)
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
                WallJumpManager.Instance.SetHorizontalSpeedMultiplier(1);
            }
            else if (Physics2D.OverlapCircle(wallCheckRight.position, groundedRadius, groundLayerMask))
            {
                WallJumpManager.Instance.SetHorizontalSpeedMultiplier(-1);
            }
            else
			{
                WallJumpManager.Instance.SetHorizontalSpeedMultiplier(0);
            }
        }
        else if (CurrentCharacterState != CharacterState.WallJumping)
		{
            WallJumpManager.Instance.SetHorizontalSpeedMultiplier(0);
        }
    }

    private void CheckIfFalling()
	{
        if (CurrentCharacterState != CharacterState.Jumping && !isGrounded && CurrentCharacterState != CharacterState.MapTransition)
		{
            CurrentCharacterState = CharacterState.Falling;
		}
	}

    private bool CheckIfCeilingBunk()
	{
        if (Physics2D.OverlapCircle(ceilingCheckLeft.position, bunkRadius, groundLayerMask) || Physics2D.OverlapCircle(ceilingCheckRight.position, bunkRadius, groundLayerMask))
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            CurrentCharacterState = CharacterState.Falling;
            return true;
        }

        return false;
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
    WallSliding,        // Not implemented
    WallJumping,
    Dashing,
    Hit,
    MapTransition,      // Consider implementing as a GameState instead
    Dying               // Not implemented
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