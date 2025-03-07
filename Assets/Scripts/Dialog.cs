using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialog
{
    // datastruktur för dialogen
    [SerializeField] List<DialogLine> lines = new List<DialogLine>();

    public List<DialogLine> Lines => lines;
}

[System.Serializable]
public class DialogLine
{
    // text som visas
    [SerializeField] private string text = "";
    // spelarens valmöjligheter
    [SerializeField] private List<DialogChoice> choices = new List<DialogChoice>();
    // karaktären som pratar
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
    // text för valalternativ
    [SerializeField] private string text = "";
    // nästa dialog som följer
    [SerializeField] private Dialog nextDialog;
    // uppdrag kopplat till val
    [SerializeField] private Quest quest;

    public string Text => text;
    public Dialog NextDialog => nextDialog;
    public Quest Quest => quest;
}