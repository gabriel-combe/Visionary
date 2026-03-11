// ConversationGraphModel.cs — Assets/Editor/

using System;
using System.Collections.Generic;
using Unity.GraphToolkit;
using Unity.GraphToolkit.Editor;

[Serializable]
public class ConversationGraphModel : GraphModel
{
    // Only DialogNodeGTK and EntryNodeGTK can be pasted/duplicated.
    public override bool CanPasteNode(AbstractNodeModel originalModel)
        => originalModel is DialogNodeGTK || originalModel is EntryNodeGTK;

    // No variables in this graph.
    public override bool CanPasteVariable(VariableDeclarationModelBase originalModel)
        => false;

    // Route List<T> to ListConstant<T> for non-null init and correct cloning.
    public override Type GetConstantType(TypeHandle typeHandle)
    {
        if (typeHandle.IsCustomTypeHandle())
            return null;

        var resolvedType = typeHandle.Resolve();

        if (resolvedType.IsGenericType &&
            resolvedType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var itemType = resolvedType.GetGenericArguments()[0];
            return typeof(ListConstant<>).MakeGenericType(itemType);
        }

        return base.GetConstantType(typeHandle);
    }
}
