using UnityEngine;

public class SaveSpot : InteractibleObject
{
    public override void Interact()
	{
		Vector2 saveSpotPosition = new Vector2(transform.position.x, transform.position.y);
		GameManager.Instance.SaveGame(saveSpotPosition);
	}
}
