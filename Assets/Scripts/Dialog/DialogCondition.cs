using System;
using UnityEngine;

public enum ConditionType
{
    PhaseEquals,            // Current phase == intParam
    PhaseMin,               // Current phase >= intParam
    GlobalEncountered,      // Talked to this NPC (cross-phase) ?
    NotGlobalEncountered,   // NEVER talked to this NPC (cross-phase) ?
    PhaseEncountered,       // Talked to this NPC ?
    NotPhaseEncountered,    // NEVER talked to this NPC ?
    MetDuringPhase,         // Talked to this NPC during phase intParam ?  (param = npcId)
    NotMetDuringPhase,      // NEVER talked during phase intParam ?
    HasFlag,                // GameFlags.Get(param) == true
    NotHasFlag,             // GameFlags.Get(param) == false
    CardObtained,           // CardSystem.HasCard(param)
    NotCardObtained,        // !CardSystem.HasCard(param)
}

[Serializable]
public class DialogCondition
{
    [Tooltip("Condition type to evaluate")]
    public ConditionType type;

    [Tooltip("Name of the flag, NPC ID or Card ID depending on the type")]
    public string param;

    [Tooltip("Numerical value (ex: phase number)")]
    public int intParam;

    [Tooltip("Card to test")]
    public Card card;
}
