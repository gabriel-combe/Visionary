using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Unity.GraphToolkit.CSO;
using Unity.GraphToolkit.Editor;
using UnityEngine;

/// <summary>
/// ScriptableObject wrapper that bridges a <see cref="Constant{T}"/> of type
/// List<T> with Unity's PropertyField/SerializedObject system.
/// </summary>
public abstract class ListFieldWrapperBase : ScriptableObject
{
    [NonSerialized] public IReadOnlyList<Constant> Constants;
    [NonSerialized] public ICommandTarget CommandTarget;
    [NonSerialized] public bool SuppressValidation;

    public abstract object ListValue { get; set; }

    void OnValidate()
    {
        if (SuppressValidation || Constants == null || CommandTarget == null)
            return;
        CommandTarget.Dispatch(new UpdateConstantsValueCommand(Constants, ListValue));
    }
}

/// <summary>
/// Holds a [SerializeField] List<T> so Unity renders it as a
/// native ReorderableList via PropertyField.
/// Concrete subclasses are generated via IL emit because ScriptableObject
/// does not support generic types directly.
/// </summary>
public abstract class ListFieldWrapper<T> : ListFieldWrapperBase
{
    [SerializeField]
    protected List<T> m_Value = new List<T>();

    public override object ListValue
    {
        get => m_Value;
        set => m_Value = value is List<T> typed ? new List<T>(typed) : new List<T>();
    }
}

/// <summary>
/// Creates and caches <see cref="ListFieldWrapperBase"/> instances.
/// Lifetime is tied to the owning model via <see cref="ConditionalWeakTable{TKey,TValue}"/>.
/// </summary>
public static class ListFieldWrapperFactory
{
    static readonly Dictionary<Type, Type> s_WrapperTypes = new();

    static readonly ConditionalWeakTable<object, List<(string id, ListFieldWrapperBase wrapper)>>
        s_ObjectWrappers = new();

    static ModuleBuilder s_ModuleBuilder;
    static int s_UniqueId;

    public static ListFieldWrapperBase GetOrCreate(
        IReadOnlyList<Constant> constants,
        ICommandTarget commandTarget)
    {
        if (constants == null || constants.Count == 0) return null;

        var constant = constants[0];
        var owner = constant.OwnerModel;
        if (owner == null) return null;

        var listType = constant.Type;
        if (!listType.IsGenericType || listType.GetGenericTypeDefinition() != typeof(List<>))
            return null;

        string cacheId = owner is PortModel port ? port.UniqueName : constant.ToString();
        object anchor = owner is PortModel pm ? (object)pm.NodeModel : owner;

        var wrappers = s_ObjectWrappers.GetOrCreateValue(anchor);

        foreach (var (id, existing) in wrappers)
        {
            if (id == cacheId && existing.CommandTarget == commandTarget)
            {
                existing.Constants = constants;
                SyncFromConstants(existing, constants);
                return existing;
            }
        }

        var concreteType = GetOrCreateConcreteType(listType);
        var wrapper = ScriptableObject.CreateInstance(concreteType) as ListFieldWrapperBase;
        if (wrapper == null) return null;

        wrapper.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.DontSave;
        wrapper.Constants = constants;
        wrapper.CommandTarget = commandTarget;
        SyncFromConstants(wrapper, constants);

        wrappers.Add((cacheId, wrapper));
        return wrapper;
    }

    public static void SyncFromConstants(ListFieldWrapperBase wrapper, IReadOnlyList<Constant> constants)
    {
        if (constants == null || constants.Count == 0) return;
        wrapper.SuppressValidation = true;
        try
        {
            var value = constants[0].ObjectValue ?? Activator.CreateInstance(constants[0].Type);
            wrapper.ListValue = value;
        }
        finally
        {
            wrapper.SuppressValidation = false;
        }
    }

    public static Type GetOrCreateConcreteType(Type listType)
    {
        if (s_WrapperTypes.TryGetValue(listType, out var cached)) return cached;

        var itemType = listType.GetGenericArguments()[0];
        var genericBase = typeof(ListFieldWrapper<>).MakeGenericType(itemType);
        var typeName = $"ListFieldWrapperConcrete_{itemType.Name}_{s_UniqueId++}";

        var tb = GetOrCreateModuleBuilder().DefineType(
            typeName, TypeAttributes.Public | TypeAttributes.Class, genericBase);
        tb.DefineDefaultConstructor(MethodAttributes.Public);
        var concreteType = tb.CreateType();

        s_WrapperTypes[listType] = concreteType;
        return concreteType;
    }

    static ModuleBuilder GetOrCreateModuleBuilder()
    {
        if (s_ModuleBuilder != null) return s_ModuleBuilder;
        var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(
            new AssemblyName("DialogListWrappersDyn"), AssemblyBuilderAccess.Run);
        s_ModuleBuilder = asm.DefineDynamicModule("DialogListWrappersModule");
        return s_ModuleBuilder;
    }
}