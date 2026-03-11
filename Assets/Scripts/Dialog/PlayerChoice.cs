using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerChoice
{
    [TextArea(1, 2)]
    public string playerText;

    [Tooltip("ID of the NPC's DialogNode to play after this choice")]
    public List<string> nextNodeIds;

    [Tooltip("Effects triggered when this choice is selected")]
    public List<DialogOutcome> outcomes = new();
}
