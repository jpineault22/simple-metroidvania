using UnityEngine;

public class WallJumpManager : Singleton<WallJumpManager>
{
    [SerializeField] private float wallJumpTime = 0.25f;
    [SerializeField] private float wallJumpVerticalForceDivider = 1.25f;

    private float wallJumpTimeCounter;
    private int horizontalSpeedMultiplier;

    public void StartWallJump()
	{
        if (horizontalSpeedMultiplier != 0)
		{
            PlayerController.Instance.CurrentCharacterState = CharacterState.WallJumping;
            wallJumpTimeCounter = wallJumpTime;
        }
	}

    public Vector2 ProcessWallJump(float jumpForce)
	{
        if (wallJumpTimeCounter > 0)
        {
            wallJumpTimeCounter -= Time.fixedDeltaTime;
            return new Vector2(jumpForce * horizontalSpeedMultiplier, jumpForce / wallJumpVerticalForceDivider);
        }
        else
        {
            PlayerController.Instance.CurrentCharacterState = CharacterState.Falling;
            horizontalSpeedMultiplier = 0;
            return Vector2.zero;
        }
    }

    public void SetHorizontalSpeedMultiplier(int value)
	{
        horizontalSpeedMultiplier = value;
	}
}
