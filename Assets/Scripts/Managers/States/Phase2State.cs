using System.Collections;
using UnityEngine;

public class Phase2State : GameState
{
    public override GameStateId Id => GameStateId.Phase2;

    public override IEnumerator OnEnter(GameContext ctx)
    {
        yield return ctx.uiDirector.ShowDayScreen("Day Two");
        yield return ctx.uiDirector.RunOssiclePhase(2);
        yield return ctx.uiDirector.ToggleOssicleView(false);

        if (ctx.handOfTheKing != null)
            yield return ctx.uiDirector.StartCoroutine(ctx.uiDirector.HandleNPCDialog(ctx.handOfTheKing));

        // Free roam - wait until we obtain a card
        ctx.uiDirector.EnableInteraction = true;
        yield return new WaitUntil(() => CardSystem.Instance.Card2 != null
                                      && !ctx.uiDirector.IsInEncounter);
        ctx.uiDirector.EnableInteraction = false;

        yield return ctx.uiDirector.ToggleOssicleView(true);
        yield return ctx.cardDeckManager.DealCard(CardSystem.Instance.Card2);

        yield return GameStateManager.Instance.TransitionTo(GameStateId.EndingPhase);
    }
}
