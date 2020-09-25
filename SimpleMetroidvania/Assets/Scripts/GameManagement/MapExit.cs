using UnityEngine;

public class MapExit : MonoBehaviour
{
    [SerializeField] private int targetSceneNumber;
    [SerializeField] private int targetMapExitNumber;
	[SerializeField] private MapTransitionDirection mapTransitionDirection;

    public int GetTargetSceneNumber()
	{
        return targetSceneNumber;
	}

    public int GetTargetMapExitNumber()
    {
        return targetMapExitNumber;
    }

	public MapTransitionDirection GetMapTransitionDirection()
	{
		return mapTransitionDirection;
	}

	// Consider moving that logic to PlayerController
	private void OnTriggerEnter2D(Collider2D pCollision)
	{
		if (pCollision.gameObject.CompareTag(Constants.TagPlayer))
		{
			PlayerController playerController = pCollision.gameObject.GetComponent<PlayerController>();
			if (!playerController.IsMapTransitioning())
			{
				playerController.SetStateToMapTransition(mapTransitionDirection, targetMapExitNumber);
				GameManager.Instance.LoadMap(this);
			}
		}
	}
}

// Could merge with the Direction enum in PlayerController.cs
public enum MapTransitionDirection
{
	Up,
	Right,
	Down,
	Left
}