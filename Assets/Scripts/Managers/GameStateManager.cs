using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : SingletonMono<GameStateManager>
{
    private Dictionary<GameStateId, GameState> _states = new();
    private GameState _current;
    private GameContext _ctx;
    private int _currentPhase = 0;

    public int CurrentPhase => _currentPhase;
    public GameStateId CurrentStateId => _current?.Id ?? GameStateId.StartMenu;

    public void Init(GameContext ctx)
    {
        _ctx = ctx;
        RegisterState(new StartMenuState());
        RegisterState(new IntroState());
        RegisterState(new Phase1State());
        RegisterState(new Phase2State());
        RegisterState(new EndingPhaseState());
        RegisterState(new CreditsState());
    }

    public void RegisterState(GameState state)
    {
        _states[state.Id] = state;
    }

    public IEnumerator TransitionTo(GameStateId next)
    {
        if (_current != null)
            yield return StartCoroutine(_current.OnExit(_ctx));

        if (!_states.TryGetValue(next, out _current))
        {
            Debug.LogWarning($"[GameStateManager] State {next} not registered.");
            yield break;
        }

        _currentPhase = next switch
        {
            GameStateId.Phase1 => 1,
            GameStateId.Phase2 => 2,
            GameStateId.EndingPhase => 3,
            _ => 0
        };

        EventBus.Emit(new GameStateChangedEvent(next));
        yield return StartCoroutine(_current.OnEnter(_ctx));
    }
}
