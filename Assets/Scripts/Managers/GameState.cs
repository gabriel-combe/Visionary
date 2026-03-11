using System.Collections;
using UnityEngine;

public enum GameStateId
{
    StartMenu,
    Intro,
    Phase1,
    Phase2,
    EndingPhase,
    Credits
}

public abstract class GameState
{
    public abstract GameStateId Id { get; }
    public virtual IEnumerator OnEnter(GameContext ctx) {  yield break; }
    public virtual IEnumerator OnExit(GameContext ctx) { yield break; }
}
