using UnityEngine.EventSystems;

public class UIManager : Singleton<UIManager>
{
	private void Start()
	{
        InputManager.Instance.Paused += PauseButtonPressed;
		InputManager.Instance.Backed += BackButtonPressed;
    }

	private void PauseButtonPressed()
	{
		if (GameManager.Instance.CurrentGameState == GameState.Paused)
		{
			PauseMenu.Instance.Resume();
		}
		else if (GameManager.Instance.CurrentGameState == GameState.Playing && PlayerController.Instance.CurrentCharacterState != CharacterState.MapTransition)
		{
			PauseMenu.Instance.Pause();
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(PauseMenu.Instance.pauseFirstButton);
		}
	}

	private void BackButtonPressed()
	{
		switch(GameManager.Instance.CurrentGameState)
		{
			case GameState.Paused:

				PauseMenu.Instance.Resume();
				break;

			case GameState.Menu:

				if (MainMenu.Instance.GetCurrentMenu() == Menu.Controls)
				{
					MainMenu.Instance.CloseControlsMenu();
				}
				else if (MainMenu.Instance.GetCurrentMenu() == Menu.Settings)
				{
					MainMenu.Instance.CloseSettingsMenu();
				}
				
				break;

			default:
				break;
		}
	}
}
