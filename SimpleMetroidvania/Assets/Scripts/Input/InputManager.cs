using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InputManager : Singleton<InputManager>
{
    private PlayerInput playerInput;
    private InputActionMap gameplayMap;
    private InputActionMap UIMap;
    public string CurrentControlScheme { get; private set; }

    public Action<string> ControlSchemeChanged;
    public Action<InputAction.CallbackContext> Moved;
    public Action StartedJump;
    public Action EndedJump;
    public Action StartedAttack;
    public Action StartedInteraction;
    public Action StartedDash;
    public Action StartedBomb;
    public Action Paused;
    public Action Backed;

	protected override void Awake()
	{
		base.Awake();

        playerInput = GetComponent<PlayerInput>();
        gameplayMap = playerInput.actions.FindActionMap(Constants.InputActionMapGameplay);
        UIMap = playerInput.actions.FindActionMap(Constants.InputActionMapUI);
        CurrentControlScheme = playerInput.currentControlScheme;
    }

    private void OnEnable()
    {
        gameplayMap.Disable();
        UIMap.Enable();
        InputUser.onChange += ChangeControlScheme;
    }

    private void OnDisable()
    {
        InputUser.onChange -= ChangeControlScheme;
        gameplayMap.Disable();
        UIMap.Disable();
    }

	private void ChangeControlScheme(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change == InputUserChange.ControlSchemeChanged)
        {
            CurrentControlScheme = playerInput.currentControlScheme;
            ControlSchemeChanged?.Invoke(playerInput.currentControlScheme);

            Cursor.visible = CurrentControlScheme != Constants.InputControlSchemeGamepad;
        }
    }

    // These methods are assigned in Unity, in the Events section of the InputManager's PlayerInput component
    // Gameplay Control Scheme

    public void Move(InputAction.CallbackContext ctx)
    {
        Moved?.Invoke(ctx);
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed) StartedJump?.Invoke();
        else if (ctx.phase == InputActionPhase.Canceled) EndedJump?.Invoke();
    }

    public void Attack(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed) StartedAttack?.Invoke();
    }

    public void Interact(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed) StartedInteraction?.Invoke();
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed) StartedDash?.Invoke();
    }

    public void Bomb(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed) StartedBomb?.Invoke();
    }

    // UI Control Scheme

    public void Pause(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed) Paused?.Invoke();
    }

    public void Back(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed) Backed?.Invoke();
    }

    public void EnableGameplayMap()
	{
        gameplayMap.Enable();
	}
}
