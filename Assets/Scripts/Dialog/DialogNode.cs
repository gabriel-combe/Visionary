using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpeakerSide { Left, Right }

[Serializable]
public class DialogNode
{
    [Tooltip("Unique ID of this node in the graph")]
    public string nodeId;

    [Header("Speaker")]
    [Tooltip("Who speaks on this node.")]
    public Speaker speaker;

    [Tooltip("Which side the speaker portrait is displayed on. Right flips the sprite.")]
    public SpeakerSide speakerSide = SpeakerSide.Left;

    [Header("Display Conditions")]
    [Tooltip("ALL conditions must be true for this node to be selected as entry")]
    public List<DialogCondition> conditions = new();

    [Header("NPC Text")]
    [TextArea(2, 8)]
    public string npcText;

    [Header("Player choice")]
    [Tooltip("If empty: linear dialog, no dice, click to continue")]
    public List<PlayerChoice> choices = new();

    [Header("Effects of this node")]
    [Tooltip("Effects applied when the node is entered (ex: GiveCard)")]
    public List<DialogOutcome> onEnterOutcomes = new();

    [Tooltip("Used when there are no choices. Points to the next node(s). Random pick if multiple.")]
    public List<string> nextNodeIds;
}