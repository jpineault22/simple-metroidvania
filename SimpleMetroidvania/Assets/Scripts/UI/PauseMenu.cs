using UnityEngine;

public class PauseMenu : Singleton<PauseMenu>
{
    [SerializeField] private GameObject pauseMenuPanel = default;

	public GameObject pauseFirstButton = default;

	public void Pause()
	{
		if (GameManager.Instance.CurrentGameState == GameState.Playing && PlayerController.Instance.CurrentCharacterState != CharacterState.Dying)
		{
			pauseMenuPanel.SetActive(true);
			GameManager.Instance.PauseGame();
		}
	}

	public void Resume()
	{
		if (GameManager.Instance.CurrentGameState == GameState.Paused)
		{
			pauseMenuPanel.SetActive(false);
			GameManager.Instance.UnpauseGame();
		}
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
