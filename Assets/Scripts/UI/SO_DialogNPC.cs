using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/NPC", fileName = "DialogNPC")]
public class SO_DialogNPC : ScriptableObject
{   
    [SerializeField] private string _name;
    [SerializeField] private Sprite _portrait;
    [SerializeField] private Card _card;
    [SerializeField]private List<DialogPath> _dialogPaths = new List<DialogPath>();

    public string Name => _name;
    public Sprite Portrait => _portrait;
    public Card Card => _card;
    public List<DialogPath> DialogPaths => _dialogPaths;

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

}
