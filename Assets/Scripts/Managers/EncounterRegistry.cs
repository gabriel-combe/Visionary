using System.Collections.Generic;
using UnityEngine;

public class EncounterRegistry : SingletonMono<EncounterRegistry>
{
    // Persitant all game
    private readonly HashSet<string> _globalMet = new();

    // Reset each new phase
    private readonly Dictionary<int, HashSet<string>> _perPhaseMet = new();

    public bool EverMet(string NpcId) => _globalMet.Contains(NpcId);
    public bool MetThisPhase(string NpcId) => MetDuringPhase(NpcId, GameStateManager.Instance.CurrentPhase);
    public bool MetDuringPhase(string npcId, int phase) => _perPhaseMet.TryGetValue(phase, out var s) && s.Contains(npcId);

    public void RecordMeeting(string NpcId)
    {
        _globalMet.Add(NpcId);
        int currentPhase = GameStateManager.Instance.CurrentPhase;
        if (!_perPhaseMet.ContainsKey(currentPhase))
            _perPhaseMet[currentPhase] = new HashSet<string>();
        _perPhaseMet[currentPhase].Add(NpcId);
    }

    public void ResetAll() { _globalMet.Clear(); _perPhaseMet.Clear(); }
}
