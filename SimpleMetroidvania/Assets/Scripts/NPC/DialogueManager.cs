using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueManager : Singleton<DialogueManager>
{
    [SerializeField] private TMP_Text nameText = default;
    [SerializeField] private TMP_Text dialogueText = default;
    [SerializeField] private GameObject dialogueFirstButton = default;
    [SerializeField] private Animator animator = default;
    
    private Queue<string> sentences;
    private string currentSentence;
    private bool coroutineRunning;

    public bool dialogueActive;

    protected override void Awake()
    {
        base.Awake();
        
        sentences = new Queue<string>();
    }

    public void StartDialogue(string pNpcName, Dialogue pDialogue)
	{
        GameManager.Instance.StartDialogueState();
        dialogueActive = true;
        animator.SetBool(Constants.AnimatorDialogueIsOpen, true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(dialogueFirstButton);

        nameText.text = pNpcName;
        sentences.Clear();

        foreach(string sentence in pDialogue.sentences)
		{
            sentences.Enqueue(sentence);
		}

        DisplayNextSentence();
	}

    public void DisplayNextSentence()
	{
        if (coroutineRunning)
		{
            StopAllCoroutines();
            coroutineRunning = false;
            dialogueText.text = currentSentence;
		}
        else
		{
            if (sentences.Count == 0)
            {
                EndDialogue();
                return;
            }

            string sentence = sentences.Dequeue();
            currentSentence = sentence;
            StartCoroutine(TypeSentence(sentence));
        }
	}

    IEnumerator TypeSentence (string pSentence)
	{
        coroutineRunning = true;
        dialogueText.text = string.Empty;

        foreach (char letter in pSentence.ToCharArray())
		{
            dialogueText.text += letter;
            yield return null;
		}

        coroutineRunning = false;
	}

    private void EndDialogue()
	{
        animator.SetBool(Constants.AnimatorDialogueIsOpen, false);
        dialogueActive = false;
        GameManager.Instance.EndDialogueState();
    }
}
