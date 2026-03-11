using UnityEngine;

public class GameContext : MonoBehaviour
{
    [Header("UI")]
    public UIDirector uiDirector;
    public DialogBoxUI dialogBox;
    public PoemUi poemManager;
    public CardDeck cardDeckManager;

    [Header("Gameplay")]
    public throwableManager throwManager;
    public DialogRunner dialogRunner;

    [Header("Scripted NPCs")]
    [Tooltip("Triggered automatically after the ossicle vision in Phase 1 and 2")]
    public NPCData handOfTheKing;
}