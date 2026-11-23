using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AIDialogue", menuName = "Scriptable Objects/AIDialogue")]
public class AIDialogue : ScriptableObject
{
    public string[] dialogueLines;

    public float TypingSpeed;
    internal static void SetText(string name)
    {
        throw new NotImplementedException();
    }
}
