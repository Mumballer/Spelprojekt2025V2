using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialog
{
    [SerializeField] List<DialogLine> lines = new List<DialogLine>();

    public List<DialogLine> Lines => lines;
}

[System.Serializable]
public class DialogLine
{
    [SerializeField] private string text = "";
    [SerializeField] private List<DialogChoice> choices = new List<DialogChoice>();
    [SerializeField] private DialogCharacter character;

    public string Text => text;
    public List<DialogChoice> Choices => choices;
    public bool HasChoices => choices != null && choices.Count > 0;
    public DialogCharacter Character
    {
        get => character;
        set => character = value;
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