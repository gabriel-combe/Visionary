using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Register<T>(T service) => _services[typeof(T)] = service;
    public static T Get<T>() => (T)_services[typeof(T)];
    public static bool TryGet<T>(out T service)
    {
        service = default;
        if (!_services.TryGetValue(typeof(T), out object obj)) return false;
        service = (T)obj;
        return true;
    }
}
