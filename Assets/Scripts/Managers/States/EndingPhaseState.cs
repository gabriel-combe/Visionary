using System.Collections;
using UnityEngine;

public class EndingPhaseState : GameState
{
    public override GameStateId Id => GameStateId.EndingPhase;

    public override IEnumerator OnEnter(GameContext ctx)
    {
        var endingCard = CardSystem.Instance.FindEnding();
        CardSystem.Instance.GrantCard(endingCard);

        yield return ctx.uiDirector.ShowDayScreen("Wedding Day");
        yield return ctx.uiDirector.RunOssiclePhase(3);
        yield return ctx.uiDirector.ToggleOssicleView(false);

        // Free roam - wait until we obtain a card
        ctx.uiDirector.EnableInteraction = true;
        yield return new WaitUntil(() => GameFlags.Instance.Get("EndingShown")
                                      && !ctx.uiDirector.IsInEncounter);
        ctx.uiDirector.EnableInteraction = false;

        yield return ctx.uiDirector.ToggleOssicleView(true);
        yield return ctx.cardDeckManager.DealCard(CardSystem.Instance.EndingCard);

        yield return GameStateManager.Instance.TransitionTo(GameStateId.Credits);
    }
}
