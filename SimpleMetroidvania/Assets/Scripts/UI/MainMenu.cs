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

	public void DeleteSaveFile()
	{
		GameManager.Instance.DeleteSaveFile();
	}
}
