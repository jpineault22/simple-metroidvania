using Cinemachine;
using UnityEngine;

public class CinemachineManager : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineConfiner confiner;
	private GameObject player;

	private void Awake()
	{
		virtualCamera = GetComponent<CinemachineVirtualCamera>();
		confiner = GetComponent<CinemachineConfiner>();
	}

	private void Start()
	{
		LevelLoader.Instance.FirstMapLoadCompleted += OnFirstLoadCompleted;
		LevelLoader.Instance.TransitionHalfDone += OnTransitionHalfDone;
		GameManager.Instance.GameStarted += OnGameStarted;
	}

	#region Event handlers

	private void OnGameStarted()
	{
		SetFollowPlayer();
		virtualCamera.PreviousStateIsValid = false;
		//transform.position = player.transform.position;
	}

	private void OnFirstLoadCompleted()
	{
		SetMapBoundariesConfiner();
	}

	private void OnTransitionHalfDone()
	{
		ResetCamera();
	}

	#endregion

	#region Setup

	private void SetFollowPlayer()
	{
		player = GameManager.Instance.player;

		if (player != null)
		{
			virtualCamera.Follow = player.transform;
		}
		else
		{
			Debug.LogError("[CinemachineManager] Couldn't find game object with tag [" + Constants.TagPlayer + "].");
		}
	}

	private void ResetCamera()
	{
		SetMapBoundariesConfiner();
		virtualCamera.PreviousStateIsValid = false;
		//transform.position = player.transform.position;
	}

	private void SetMapBoundariesConfiner()
	{
		GameObject mapBoundaries = LevelLoader.Instance.CurrentFunctionalMap.transform.Find(Constants.NameGameObjectMapBoundaries).gameObject;

		if (mapBoundaries != null)
		{
			confiner.m_BoundingShape2D = mapBoundaries.GetComponent<CompositeCollider2D>();
			// The 2D confiner caches the path shape for performance. When changing the path, call the following method to rebuild the cache.
			confiner.InvalidatePathCache();
		}
		else
		{
			Debug.LogError("[CinemachineManager] Couldn't find game object with tag [" + Constants.TagMapBoundaries + "].");
		}
	}

	#endregion
}
