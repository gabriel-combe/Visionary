using System.Collections;

public class CreditsState : GameState
{
    public override GameStateId Id => GameStateId.Credits;

    public override IEnumerator OnEnter(GameContext ctx)
    {
        ctx.uiDirector.ShowEndMenu();
        yield break;
    }
}
