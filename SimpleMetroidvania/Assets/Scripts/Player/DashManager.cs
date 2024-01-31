using UnityEngine;

public class DashManager : Singleton<DashManager>
{
    public Direction DashDirection { get; set; }

    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float dashTime = 0.15f;
    [SerializeField] private float dashRate = 3f;                               // Times per second
    [SerializeField] private int maxNumberOfDashes = 1;                         // Could add powerup to increase that number

    private int numberOfDashes;
    private float dashTimeCounter;
    private float nextDashTime;

	protected override void Awake()
	{
		base.Awake();

        dashTimeCounter = dashTime;
        DashDirection = Direction.None;
        numberOfDashes = maxNumberOfDashes;
    }

    public void StartDash()
    {
        if (Time.time >= nextDashTime && numberOfDashes != 0)
        {
            nextDashTime = Time.time + 1f / dashRate;
            dashTimeCounter = dashTime;
            numberOfDashes--;
            PlayerController.Instance.CurrentCharacterState = CharacterState.Dashing;
        }
    }

    public Vector2 ProcessDash(bool facingRight)
    {
        if (dashTimeCounter > 0)
        {
            dashTimeCounter -= Time.fixedDeltaTime;
            Vector2 dashVector = CreateDashVector(facingRight);

            // Possibly add camera shake later

            return dashVector;
        }
        else
        {
            PlayerController.Instance.CurrentCharacterState = CharacterState.Falling;
            return Vector2.zero;
        }
    }

    public void ResetDashes()
	{
        numberOfDashes = maxNumberOfDashes;
    }

    private Vector2 CreateDashVector(bool facingRight)
    {
        Vector2 dashVector = Vector2.zero;

        // Redesign this section, it's much more redundant than with the initial direction system
        switch (DashDirection)
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
}
