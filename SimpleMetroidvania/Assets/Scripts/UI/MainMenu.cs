using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
	{
		GameManager.Instance.LoadGame();
	}

	public void QuitGame()
	{
		GameManager.Instance.QuitGame();
	}
}
