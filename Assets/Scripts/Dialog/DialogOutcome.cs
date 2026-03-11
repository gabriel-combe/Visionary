using System;
using UnityEngine;

public enum OutcomeType
{
    GiveCard,               // Give a card to the player (param = ID of the card in CardsPool)
    SetFlag,                // GameFlags.Set(param, true)
    ClearFlag,              // GameFlags.Set(param, false)
    EndDialog,              // Close the dialog immediatly
}

[Serializable]
public class DialogOutcome
{
    public OutcomeType type;

    [Tooltip("Name of the flag or NpcId")]
    public string param;

    [Tooltip("Card to give")]
    public Card card;
}
