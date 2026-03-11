using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    private static readonly Dictionary<Type, List<object>> _handlers = new();

    public static void Subscribe<T>(Action<T> handler)
    {
        var key = typeof(T);
        if (!_handlers.ContainsKey(key)) _handlers[key] = new();
        _handlers[key].Add(handler);
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        var key = typeof(T);
        if (!_handlers.ContainsKey(key)) return;
        _handlers[key].Remove(handler);
    }

    public static void Emit<T>(T evt)
    {
        if (!_handlers.TryGetValue(typeof(T), out var list)) return;
        foreach (var handler in list) ((Action<T>)handler)(evt);
    }
}

// Typed event (light structs)
public struct GameStateChangedEvent
{
    public GameStateId State;
    public GameStateChangedEvent(GameStateId state) { State = state; }
}

public struct CardUpdatedEvent
{
    public IReadOnlyList<Card> Cards;
    public CardUpdatedEvent(IReadOnlyList<Card> cards) { Cards = cards; }
}

public struct DiceResultEvent
{
    public int Value;
    public DiceResultEvent(int value) { Value = value; }
}

public struct OssicleResultEvent
{
    public int UpCount;
    public OssicleResultEvent(int upCount) { UpCount = upCount; }
}

public struct DialogNodeStartedEvent
{
    public string NpcId;
    public DialogNode Node;
    public DialogNodeStartedEvent(string npcId, DialogNode node) { NpcId = npcId; Node = node; }
}

public struct EnvironmentChangedEvent
{
    public int Index;
    public EnvironmentChangedEvent(int index) { Index = index; }
}
public struct OssicleViewChangedEvent
{
    public bool IsOssicleView;
    public OssicleViewChangedEvent(bool isOssicleView) { IsOssicleView = isOssicleView; }
}