using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/NPC", fileName = "DialogNPC")]
public class SO_DialogNPC : ScriptableObject
{   
    public string Name;
    public Sprite Portrait;

    [Serializable]
    public class DialogEntry
    {
        public string playerText;
        public string npcText;
    }

    [Serializable]
    public class DialogPath
    {
        public List<DialogEntry> entries = new List<DialogEntry>();
    }

    public List<DialogPath> DialogPaths = new List<DialogPath>();
}
