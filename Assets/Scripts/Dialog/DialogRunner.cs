using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogRunner : MonoBehaviour
{
    private string _nextNodeId = null;
    private bool _stopDialog = false;

    #region Entry Node

    /// <summary>
    /// Iterates through startNodeIds (set by the Entry Node in the GTK canvas),
    /// filters by conditions, and picks one at random among the valid candidates.
    /// </summary>
    public DialogNode FindEntryNode(ConversationGraph graph)
    {
        if (graph.startNodeIds != null && graph.startNodeIds.Count > 0)
        {
            var valid = new List<DialogNode>();
            foreach (var id in graph.startNodeIds)
            {
                var node = graph.GetNode(id);
                if (node != null && EvaluateAll(node.conditions))
                    valid.Add(node);
            }
            if (valid.Count > 0)
                return valid[UnityEngine.Random.Range(0, valid.Count)];
        }

        Debug.LogWarning($"[DialogRunner] No valid entry node found in {graph.conversationId}");
        return null;
    }

    #endregion

    #region Condition Evaluation

    private bool EvaluateAll(List<DialogCondition> conditions)
    {
        foreach (var c in conditions)
            if (!Evaluate(c)) return false;
        return true;
    }

    private bool Evaluate(DialogCondition c)
    {
        var enc = EncounterRegistry.Instance;
        var flags = GameFlags.Instance;
        var cards = CardSystem.Instance;
        var gsm = GameStateManager.Instance;

        return c.type switch
        {
            ConditionType.PhaseEquals => gsm.CurrentPhase == c.intParam,
            ConditionType.PhaseMin => gsm.CurrentPhase >= c.intParam,
            ConditionType.GlobalEncountered => enc.EverMet(c.param),
            ConditionType.NotGlobalEncountered => !enc.EverMet(c.param),
            ConditionType.PhaseEncountered => enc.MetThisPhase(c.param),
            ConditionType.NotPhaseEncountered => !enc.MetThisPhase(c.param),
            ConditionType.MetDuringPhase => enc.MetDuringPhase(c.param, c.intParam),
            ConditionType.NotMetDuringPhase => !enc.MetDuringPhase(c.param, c.intParam),
            ConditionType.HasFlag => flags.Get(c.param),
            ConditionType.NotHasFlag => !flags.Get(c.param),
            ConditionType.CardObtained => cards.HasCard(c.card),
            ConditionType.NotCardObtained => !cards.HasCard(c.card),
            _ => true
        };
    }

    #endregion

    #region Apply Outcomes

    /// <summary>
    /// Applies a list of outcomes. Returns true if an EndDialog outcome was encountered.
    /// </summary>
    public bool ApplyOutcomes(List<DialogOutcome> outcomes)
    {
        foreach (var o in outcomes)
        {
            switch (o.type)
            {
                case OutcomeType.GiveCard:
                    if (o.card != null) CardSystem.Instance.GrantCard(o.card);
                    else Debug.LogWarning("[DialogRunner] GiveCard outcome has no card assigned.");
                    break;
                case OutcomeType.SetFlag:
                    GameFlags.Instance.Set(o.param, true);
                    break;
                case OutcomeType.ClearFlag:
                    GameFlags.Instance.Set(o.param, false);
                    break;
                case OutcomeType.EndDialog:
                    return true;
            }
        }
        return false;
    }

    #endregion

    #region Main Coroutine

    /// <summary>
    /// Runs a full dialog for an NPC. Call via StartCoroutine.
    /// </summary>
    public IEnumerator RunDialog(ConversationGraph graph, DialogBoxUI dialogBox, throwableManager throwManager)
    {
        _stopDialog = false;

        var currentNode = FindEntryNode(graph);
        if (currentNode == null) yield break;

        if (currentNode.speaker != null)
            EncounterRegistry.Instance.RecordMeeting(currentNode.speaker.NpcId);

        dialogBox.OpenDialog();

        while (currentNode != null)
        {
            if (ApplyOutcomes(currentNode.onEnterOutcomes)) break;

            dialogBox.SetSpeaker(currentNode.speaker, currentNode.speakerSide);
            yield return StartCoroutine(dialogBox.ShowTextAndWaitForClick(currentNode.npcText));

            // No choices: advance via nextNodeIds (random pick if more than one)
            if (currentNode.choices == null || currentNode.choices.Count == 0)
            {
                if (currentNode.nextNodeIds == null || currentNode.nextNodeIds.Count == 0) break;

                string nextId = currentNode.nextNodeIds.Count == 1
                    ? currentNode.nextNodeIds[0]
                    : currentNode.nextNodeIds[UnityEngine.Random.Range(0, currentNode.nextNodeIds.Count)];

                currentNode = graph.GetNode(nextId);
                continue;
            }

            // Choices: throw dice, display options, wait for selection
            yield return StartCoroutine(RunChoices(currentNode, dialogBox, throwManager));
            if (_stopDialog) break;

            currentNode = graph.GetNode(_nextNodeId);
        }

        dialogBox.HideDialog();
    }

    /// <summary>
    /// Handles the dice roll, choice display, and selection for a node with choices.
    /// Sets _nextNodeId and _stopDialog on completion.
    /// </summary>
    public IEnumerator RunChoices(DialogNode currentNode, DialogBoxUI dialogBox, throwableManager throwManager)
    {
        // Wait for dice result
        int diceResult = -1;
        Action<DiceResultEvent> onDice = e => diceResult = e.Value;
        EventBus.Subscribe<DiceResultEvent>(onDice);
        dialogBox.ChoicesContainer.SetActive(true);
        throwManager.ThrowDice();
        yield return new WaitUntil(() => diceResult >= 0);
        dialogBox.ChoicesContainer.SetActive(false);
        EventBus.Unsubscribe<DiceResultEvent>(onDice);

        // How many choices are revealed based on dice roll (at least 1)
        int revealCount = Mathf.Clamp(
            Mathf.CeilToInt((diceResult / 20f) * currentNode.choices.Count),
            1, currentNode.choices.Count);

        Debug.Log($"Dice: {diceResult}  Choices: {currentNode.choices.Count}  Revealed: {revealCount}");

        // Split into revealed / hidden, shuffle the revealed portion
        var revealed = new List<(PlayerChoice, bool)>();
        var hidden = new List<(PlayerChoice, bool)>();
        for (int i = 0; i < currentNode.choices.Count; i++)
        {
            if (i < revealCount) revealed.Add((currentNode.choices[i], true));
            else hidden.Add((currentNode.choices[i], false));
        }
        revealed.Shuffle();

        var displayChoices = new List<(PlayerChoice, bool)>();
        displayChoices.AddRange(revealed);
        displayChoices.AddRange(hidden);

        yield return StartCoroutine(dialogBox.ShowChoicesAndWait(displayChoices));

        int selectedIndex = dialogBox.LastSelectedChoice;
        if (selectedIndex < 0 || selectedIndex >= displayChoices.Count) { _stopDialog = true; yield break; }

        var selectedChoice = displayChoices[selectedIndex].Item1;

        // Random pick if the choice has multiple next nodes
        _nextNodeId = selectedChoice.nextNodeIds != null && selectedChoice.nextNodeIds.Count > 0
            ? selectedChoice.nextNodeIds[UnityEngine.Random.Range(0, selectedChoice.nextNodeIds.Count)]
            : null;

        if (ApplyOutcomes(selectedChoice.outcomes)) { _stopDialog = true; yield break; }

        dialogBox.ClearChoices();
    }

    #endregion
}

public static class IListExtensions
{
    /// <summary>Fisher-Yates shuffle.</summary>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}