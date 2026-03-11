using System.Collections;

public class StartMenuState : GameState
{
    public override GameStateId Id => GameStateId.StartMenu;

    public override IEnumerator OnEnter(GameContext ctx)
    {
        yield return ctx.uiDirector.ShowStartMenuAndWait();
        yield return GameStateManager.Instance.TransitionTo(GameStateId.Intro);
    }
}
