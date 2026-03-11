using System.Collections;
using UnityEngine;

public class Phase1State : GameState
{
    public override GameStateId Id => GameStateId.Phase1;

    public override IEnumerator OnEnter(GameContext ctx)
    {
        CardSystem.Instance.ResetCards();
        GameFlags.Instance.Reset();

        yield return ctx.uiDirector.ShowDayScreen("Day One");
        yield return ctx.uiDirector.RunOssiclePhase(1);
        yield return ctx.uiDirector.ToggleOssicleView(false);

        if (ctx.handOfTheKing != null)
            yield return ctx.uiDirector.StartCoroutine(ctx.uiDirector.HandleNPCDialog(ctx.handOfTheKing));

        // Free roam - wait until we obtain a card
        ctx.uiDirector.EnableInteraction = true;
        yield return new WaitUntil(() => CardSystem.Instance.Card1 !=  null
                                      && !ctx.uiDirector.IsInEncounter);
        ctx.uiDirector.EnableInteraction = false;

        yield return ctx.uiDirector.ToggleOssicleView(true);
        yield return ctx.cardDeckManager.DealCard(CardSystem.Instance.Card1);

        yield return GameStateManager.Instance.TransitionTo(GameStateId.Phase2);
    }
}
