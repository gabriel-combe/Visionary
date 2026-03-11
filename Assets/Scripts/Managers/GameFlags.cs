using System.Collections.Generic;
using UnityEngine;

public class GameFlags : SingletonMono<GameFlags>
{
    private Dictionary<string, bool> _flags = new();

    public bool Get(string key) => _flags.TryGetValue(key, out var v) && v;
    public void Set(string key, bool value) => _flags[key] = value;
    public void Reset() => _flags.Clear();
}