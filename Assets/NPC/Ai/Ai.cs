using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class Ai : MonoBehaviour, IInteracble
{
    public AIDialogue DialogueData;
    public GameObject DialoguePanel;


    public TMP_Text dialogueText;
    public GameObject interactIcon;



    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    public bool Caninteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if(isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;
        interactIcon.SetActive(false);
        DialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
  
    }

        void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(DialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex <  DialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach(char letter in DialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(DialogueData.TypingSpeed);
            NextLine();


        }

        isTyping = false;
    }



    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        DialoguePanel.SetActive(false);
    }
}
