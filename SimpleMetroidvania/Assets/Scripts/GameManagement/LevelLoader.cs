using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : Singleton<LevelLoader>
{
    [SerializeField] private Animator crossfadeAnimator = default;
    [SerializeField] private float fadeTime = 1/3;                          // Time for fade out/in, total transition animation time is twice that amount

    public string CurrentMapName { get; private set; } = string.Empty;
    public GameObject CurrentFunctionalMap { get; private set; }

    public event Action FirstMapLoadCompleted;
    public event Action TransitionHalfDone;                                 // This event is invoked if the CrossfadeStart animation has ended AND the next map has been loaded
    public event Action MapTransitionEnded;

    private AsyncOperation currentMapLoadOperation;
    private string previousMapName = string.Empty;
    private bool crossfadeStartEnded;

	private void Start()
	{
        DontDestroyOnLoad(gameObject);
	}

	private void Update()
	{
		if (crossfadeStartEnded && currentMapLoadOperation.isDone)
		{
            crossfadeStartEnded = false;
            TransitionHalfDone?.Invoke();
		}
	}

	public void LoadFirstMap(string pSceneName)
	{
        LoadMap(pSceneName);
        // Invoke an event when first map after menu is loaded
        currentMapLoadOperation.completed += ctx => FirstMapLoadCompleted?.Invoke();
    }

    public void LoadMapFromMapExit(string pSceneName)
	{
        previousMapName = CurrentMapName;
        LoadMap(pSceneName);
        currentMapLoadOperation.allowSceneActivation = false;

        // Start Crossfade animation
        StartCoroutine(CrossfadeStartTransition());
    }

	private void LoadMap(string pSceneName)
	{
        CurrentMapName = pSceneName;
        currentMapLoadOperation = LoadScene(pSceneName);
    }

    public void UnloadMapToMenu()
	{
        if (CurrentMapName != string.Empty)
		{
            UnloadScene(CurrentMapName);
            CurrentMapName = previousMapName = string.Empty;
        }
        else
		{
            Debug.LogError("[LevelLoader] Current map not set, cannot unload scene.");
		}
	}

    public AsyncOperation LoadScene(string pSceneName)
	{
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(pSceneName, LoadSceneMode.Additive);

        if (asyncOperation == null)
        {
            Debug.LogError("[GameManager] Unable to load scene " + pSceneName);
            return null;
        }

        asyncOperation.completed += OnLoadOperationComplete;

        return asyncOperation;
    }

    public void UnloadScene(string pSceneName)
    {
        AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(pSceneName);

        if (asyncOperation == null)
        {
            Debug.LogError("[GameManager] Unable to unload scene " + pSceneName);
            return;
        }

        asyncOperation.completed += OnUnloadOperationComplete;
    }

    private void OnLoadOperationComplete(AsyncOperation pAsyncOperation)
    {
        Debug.Log("Load Complete");

        if (GameManager.Instance.CurrentGameState == GameState.Playing)
		{
            Scene currentMap = SceneManager.GetSceneByName(CurrentMapName);
            GameObject[] gameObjects = currentMap.GetRootGameObjects();

            FindFunctionalMap(gameObjects);
            Spawner.Instance.FindSpawnPoints(gameObjects);
        }
    }

    private void OnUnloadOperationComplete(AsyncOperation pAsyncOperation)
    {
        Debug.Log("Unload Complete.");
    }

    IEnumerator CrossfadeStartTransition()
	{
        crossfadeAnimator.SetTrigger(Constants.AnimatorCrossfadeStart);

        yield return new WaitForSeconds(fadeTime);

        currentMapLoadOperation.allowSceneActivation = true;

        if (previousMapName != string.Empty)
        {
            UnloadScene(previousMapName);
            previousMapName = string.Empty;
        }

        crossfadeStartEnded = true;

        StartCoroutine(CrossfadeEndTransition());
    }

    IEnumerator CrossfadeEndTransition()
	{
        crossfadeAnimator.SetTrigger(Constants.AnimatorCrossfadeEnd);

        yield return new WaitForSeconds(fadeTime);

        MapTransitionEnded?.Invoke();
    }

    private void FindFunctionalMap(GameObject[] pGameObjects)
	{
        foreach (GameObject obj in pGameObjects)
        {
            if (obj.CompareTag(Constants.TagFunctionalMap))
            {
                CurrentFunctionalMap = obj;
            }
        }
    }
}
