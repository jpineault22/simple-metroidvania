using UnityEngine;

public class GroundBombable : MonoBehaviour
{
    public void BombGround()
	{
		// Animate bombabilization
		Destroy(gameObject);
	}
}
