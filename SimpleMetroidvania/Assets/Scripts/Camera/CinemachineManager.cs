using Cinemachine;
using UnityEngine;

public class CinemachineManager : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineConfiner confiner;

	private void Awake()
	{
		virtualCamera = GetComponent<CinemachineVirtualCamera>();
		confiner = GetComponent<CinemachineConfiner>();
	}

	private void Start()
	{
		LevelLoader.Instance.FirstMapLoadCompleted += OnMapLoaded;
		LevelLoader.Instance.TransitionHalfDone += OnMapLoaded;
		GameManager.Instance.MenuReloaded += OnMenuReloaded;
	}

	#region Event handlers

	private void OnMapLoaded()
	{
		SetMapBoundariesConfiner();
		SetFollowPlayer();
	}

	private void OnMenuReloaded()
	{
		ResetVirtualCameraProperties();
	}

	#endregion

	#region Setup

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

	private void SetFollowPlayer()
	{
		if (GameManager.Instance.player != null)
		{
			virtualCamera.PreviousStateIsValid = false;
			transform.position = GameManager.Instance.player.transform.position;
			virtualCamera.Follow = GameManager.Instance.player.transform;
		}
		else
		{
			Debug.LogError("[CinemachineManager] Couldn't find game object with tag [" + Constants.TagPlayer + "].");
		}
	}

	private void ResetVirtualCameraProperties()
	{
		// It works, but check warnings on virtual camera component at runtime
		confiner.m_BoundingShape2D = null;
		virtualCamera.Follow = null;
	}

	#endregion
}
