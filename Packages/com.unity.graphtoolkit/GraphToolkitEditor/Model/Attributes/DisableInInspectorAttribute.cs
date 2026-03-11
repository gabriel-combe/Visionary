using System;

namespace Unity.GraphToolkit.Editor
{
    /// <summary>
    /// Attribute to disable a field in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [UnityRestricted]
    public class DisableInInspectorAttribute : Attribute
    {
    }
}
