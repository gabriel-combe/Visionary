using UnityEngine;

public class NPCData : MonoBehaviour
{
    [SerializeField] private SO_DialogNPC _dialogData;
    private bool _hasEncountered = false;

    public SO_DialogNPC DialogData => _dialogData;
    public bool HasEncountered => _hasEncountered;

    public void Encountered()
    {
        _hasEncountered = true;
    }

    public void ResetEncounters()
    {
        _hasEncountered = false;
    }
}
