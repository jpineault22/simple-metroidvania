using UnityEngine;

// This class defines objects that have a trigger collider displaying a short message when in range, and with which the player can interact (such as NPCs, save spots and objects to pick up)
public abstract class InteractibleObject : MonoBehaviour
{
    [SerializeField] private GameObject interactMessageCanvas = default;

	public bool InRange { get; private set; }

	protected virtual void Awake()
	{
		interactMessageCanvas.SetActive(false);
	}

	private void OnTriggerEnter2D(Collider2D pCollision)
	{
		interactMessageCanvas.SetActive(true);
		InRange = true;
	}

	private void OnTriggerExit2D(Collider2D pCollision)
	{
		interactMessageCanvas.SetActive(false);
		InRange = false;
	}

	public abstract void Interact();
}
