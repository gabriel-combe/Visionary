// ListConstant.cs — Assets/Editor/

using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

/// <summary>
/// Specialisation of <see cref="Constant{T}"/> for <see cref="List{T}"/> values.
/// Guarantees non-null initialisation and element-copy cloning.
/// </summary>
[Serializable]
public class ListConstant<T> : Constant<List<T>>
{
    // Always returns a new empty list — never null.
    public override object DefaultValue => new List<T>();

    // Shallow element copy: new list with the same element references.
    // For structs (DialogCondition, DialogOutcome, PlayerChoiceGTK) this is a true deep copy.
    public override Constant Clone()
    {
        var copy = (ListConstant<T>)Activator.CreateInstance(GetType());
        copy.m_Value = m_Value != null ? new List<T>(m_Value) : new List<T>();
        return copy;
    }
}
