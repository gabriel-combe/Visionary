using System.Collections;

public class IntroState : GameState
{
    public override GameStateId Id => GameStateId.Intro;

    public override IEnumerator OnEnter(GameContext ctx)
    {
        yield return ctx.uiDirector.ShowDebutTextAndWait();
        yield return GameStateManager.Instance.TransitionTo(GameStateId.Phase1);
    }
}
