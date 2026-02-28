using UnityEngine;

public class NPCData : MonoBehaviour
{
    [SerializeField] private SO_DialogNPC _dialogData;
    private int _numberOfEncounters = 0;

    public SO_DialogNPC DialogData => _dialogData;
    public int NumberOfEncounters => _numberOfEncounters;

    public void IncrementEncounters()
    {
        _numberOfEncounters++;
    }
}
