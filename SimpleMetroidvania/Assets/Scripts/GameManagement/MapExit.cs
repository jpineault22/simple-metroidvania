using UnityEngine;

public class MapExit : MonoBehaviour
{
    [SerializeField] private int targetSceneNumber = default;
    [SerializeField] private int targetMapExitNumber = default;
	[SerializeField] private MapTransitionDirection mapTransitionDirection = default;

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
			if (PlayerController.Instance.CurrentCharacterState != CharacterState.MapTransition)
			{
				PlayerController.Instance.SetStateToMapTransition(mapTransitionDirection, targetMapExitNumber);
				GameManager.Instance.LoadMap(this);
			}
		}
	}
}

public enum MapTransitionDirection
{
	Up,
	Right,
	Down,
	Left
}