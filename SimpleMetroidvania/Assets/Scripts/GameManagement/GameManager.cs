using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
	[SerializeField] private GameObject[] systemPrefabs = default;			// Prefabs to instantiate when launching the game

	public GameState CurrentGameState { get; private set; }
	[HideInInspector] public GameObject player;

	public event Action MenuReloaded;
	
	private List<GameObject> instantiatedSystemPrefabs;

	private void Start()
	{
		DontDestroyOnLoad(gameObject);

		LevelLoader.Instance.FirstMapLoadCompleted += SetPlayerPosition;

		instantiatedSystemPrefabs = new List<GameObject>();
		InstantiateSystemPrefabs();

		LoadMenu();
	}

	#region Game saving and loading, pause
	
	public void SaveGame(Vector2 pSaveSpotPosition)
	{
		int saveSpotMapNumber = int.Parse(LevelLoader.Instance.CurrentMapName.Replace(Constants.NamePrefixSceneMap, string.Empty));
		Debug.Log("Saving in " + Constants.NamePrefixSceneMap + saveSpotMapNumber);

		PlayerData data = new PlayerData(
			pSaveSpotPosition.x,
			pSaveSpotPosition.y,
			saveSpotMapNumber,
			PlayerController.Instance.HasDash,
			PlayerController.Instance.HasWallJump,
			PlayerController.Instance.HasBomb,
			PlayerHealth.Instance.CurrentHP,
			PlayerHealth.Instance.MaxHP,
			PlayerController.Instance.AttackDamage);

		SaveSystem.SavePlayerData(data);
	}
	
	public void LoadGame()
	{
		PlayerData data = SaveSystem.LoadPlayerData();
		int saveSpotMapNumber = Constants.StartingMapNumber;

		if (data != null)
		{
			saveSpotMapNumber = data.SaveSpotMapNumber;
			Spawner.Instance.PlayerSavedPosition = new Vector2(data.SaveSpotPositionX, data.SaveSpotPositionY);
		}

		StartGame(saveSpotMapNumber);

		if (data != null)
		{
			PlayerController.Instance.HasDash = data.HasDash;
			PlayerController.Instance.HasWallJump = data.HasWallJump;
			PlayerController.Instance.HasBomb = data.HasBomb;
			PlayerHealth.Instance.LoadHP(data.CurrentHP, data.MaxHP);
			PlayerController.Instance.AttackDamage = data.AttackDamage;
		}
	}

	public void DeleteSaveFile()
	{
		SaveSystem.DeleteSaveFile();
	}

	public void StartGame(int pSaveSpotMapNumber)
	{
		CurrentGameState = GameState.Playing;

		InstantiatePlayer();
		LevelLoader.Instance.LoadFirstMap(Constants.NamePrefixSceneMap + pSaveSpotMapNumber);
		LevelLoader.Instance.UnloadScene(Constants.NameSceneStartMenu);
	}

	public void LoadMap(MapExit pMapExit)
	{
		LevelLoader.Instance.LoadMapFromMapExit(Constants.NamePrefixSceneMap + pMapExit.GetTargetSceneNumber());
	}

	public void PauseGame()
	{
		Time.timeScale = 0f;
		CurrentGameState = GameState.Paused;
	}

	public void UnpauseGame()
	{
		Time.timeScale = 1f;
		CurrentGameState = GameState.Playing;
	}

	public void QuitToMenu()
	{
		LoadMenu();
		DestroyPlayer();
		LevelLoader.Instance.UnloadMapToMenu();
		MenuReloaded?.Invoke();
	}
	
	public void QuitGame()
	{
	#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
	#else
		Application.Quit();
	#endif
	}

	private void LoadMenu()
	{
		CurrentGameState = GameState.Menu;
		LevelLoader.Instance.LoadScene(Constants.NameSceneStartMenu);
	}

	private void SetPlayerPosition()
	{
		Vector2 playerPosition = Spawner.Instance.GetPlayerPosition();
		player.transform.position = new Vector3(playerPosition.x, playerPosition.y, player.transform.position.z);
	}

	#endregion

	#region Instantiate/Destroy methods

	private void InstantiateSystemPrefabs()
	{
		for (int i = 0; i < systemPrefabs.Length; i++)
		{
			GameObject prefabInstance = Instantiate(systemPrefabs[i]);
			instantiatedSystemPrefabs.Add(prefabInstance);
		}
	}

	private void InstantiatePlayer()
	{
		player = Spawner.Instance.InstantiatePlayer();
	}

	private void DestroyPlayer()
	{
		Spawner.Instance.DestroyPlayer(player);
		player = null;
	}

	#endregion

	#region State setting

	public void StartDialogueState()
	{
		CurrentGameState = GameState.Dialogue;
		PlayerController.Instance.StopMovement();
	}

	public void EndDialogueState()
	{
		CurrentGameState = GameState.Playing;
	}

	#endregion

	protected override void OnDestroy()
	{
		for (int i = 0; i < instantiatedSystemPrefabs.Count; i++)
		{
			Destroy(instantiatedSystemPrefabs[i]);
		}

		instantiatedSystemPrefabs.Clear();
		
		base.OnDestroy();
	}
}

public enum GameState
{
	Menu,
	Playing,
	Paused,
	Dialogue,
	Cutscene
}