using UnityEngine;

public class NPC : InteractibleObject
{
	[SerializeField] private string npcName = default;
	
	private DialogueTrigger dialogueTrigger;

	protected override void Awake()
	{
		base.Awake();

		dialogueTrigger = GetComponent<DialogueTrigger>();
	}

	public override void Interact()
	{
		if (InRange && !DialogueManager.Instance.dialogueActive)
		{
			dialogueTrigger.TriggerDialogue(npcName);
		}
	}
}
