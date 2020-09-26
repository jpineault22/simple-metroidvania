using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel = default;

	// Create a UIManager class to gather the input when you'll have multiple menus (inventory, map, etc.)
	private UIControls controls;

	private void Awake()
	{
		controls = new UIControls();
		controls.InGameUI.Pause.performed += ctx => PauseButtonPressed();
	}

	private void OnEnable()
	{
		controls.Enable();
	}

	private void OnDisable()
	{
		controls.Disable();
	}

	private void PauseButtonPressed()
	{
		if (GameManager.Instance.CurrentGameState == GameState.Paused)
		{
			Resume();
		}
		else if (GameManager.Instance.CurrentGameState == GameState.Playing && PlayerController.Instance.CurrentCharacterState != CharacterState.MapTransition)
		{
			Pause();
		}
	}

	public void Pause()
	{
		pauseMenuPanel.SetActive(true);
		GameManager.Instance.PauseGame();
	}

	public void Resume()
	{
		pauseMenuPanel.SetActive(false);
		GameManager.Instance.UnpauseGame();
	}

	public void LoadMenu()
	{
		Resume();
		GameManager.Instance.QuitToMenu();
	}

	public void QuitGame()
	{
		GameManager.Instance.QuitGame();
	}
}
