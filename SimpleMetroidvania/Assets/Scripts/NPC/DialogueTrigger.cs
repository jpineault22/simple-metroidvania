using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue[] dialogue = default;

	// Find a way to make this variable persist through scene changes
	private int currentDialogueIndex;

	public void TriggerDialogue(string pNpcName)
	{
		DialogueManager.Instance.StartDialogue(pNpcName, dialogue[currentDialogueIndex]);

		if (currentDialogueIndex < dialogue.Length - 1)
		{
			currentDialogueIndex++;
		}
	}
}
