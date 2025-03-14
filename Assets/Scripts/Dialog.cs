using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog", menuName = "RPG/Dialog")]
public class Dialog : ScriptableObject
{
    // Use the standalone DialogLine class
    [SerializeField] private List<DialogLine> lines = new List<DialogLine>();
    
    // Public accessor
    public List<DialogLine> Lines => lines;
    
    // Utility to quickly add lines
    public void AddLine(string text, DialogCharacter character = null)
    {
        // lägger till ny rad
        DialogLine newLine = new DialogLine();
        newLine.SetText(text);
        newLine.Character = character;
        lines.Add(newLine);
    }
}

// Keep only these standalone classes
[System.Serializable]
public class DialogLine
{
    [SerializeField] [TextArea(2, 5)] private string text = "";
    [SerializeField] private List<DialogChoice> choices = new List<DialogChoice>();
    [SerializeField] private DialogCharacter character;
    [SerializeField] private Dialog nextDialog;

    public string Text => text;
    public List<DialogChoice> Choices => choices;
    public bool HasChoices => choices != null && choices.Count > 0;
    public DialogCharacter Character
    {
        get => character;
        set => character = value;
    }
    public Dialog NextDialog => nextDialog;
    
    // Helper method to set text through reflection
    public void SetText(string newText)
    {
        text = newText;
    }
}

[System.Serializable]
public class DialogChoice
{
    [SerializeField] private string text = "";
    [SerializeField] private Dialog nextDialog;
    [SerializeField] private Quest quest;

    public string Text => text;
    public Dialog NextDialog => nextDialog;
    public Quest Quest => quest;
}