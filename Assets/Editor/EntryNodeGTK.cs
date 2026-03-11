// EntryNodeGTK.cs — Assets/Editor/
// Defines the entry point of a conversation graph.
// Connect its Out port to one or more Dialog Nodes.
// The exporter reads conversationId and startNodeIds from this node.
// Cannot be deleted from the graph.

using System;
using Unity.GraphToolkit;
using Unity.GraphToolkit.Editor;
using UnityEngine;

[Serializable]
public class EntryNodeGTK : NodeModel
{
    public const string k_ConversationId = "conversationId";

    public EntryNodeGTK()
    {
        // Prevent the Entry Node from being deleted
        SetCapability(Unity.GraphToolkit.Editor.Capabilities.Deletable, false);
        SetCapability(Unity.GraphToolkit.Editor.Capabilities.Copiable, false);
    }

    public bool TryGetOption<T>(string id, out T value)
    {
        string key = $"{NodeOption.k_OptionIdPrefix}{id}";
        if (InputConstantsById.TryGetValue(key, out var constant) && constant.TryGetValue(out value))
            return true;
        value = default;
        return false;
    }

    public T GetOption<T>(string id, T fallback = default)
        => TryGetOption<T>(id, out var v) ? v : fallback;

    protected override void OnDefineNode(NodeDefinitionScope scope)
    {
        // Multi: can point to several candidate start nodes (random among those with valid conditions)
        var outPort = scope.AddOutputPort<DialogFlow>("Out");
        outPort.Capacity = PortCapacity.Multi;

        // TextArea attribute makes the string field taller in the inspector
        scope.AddNodeOption(
            k_ConversationId,
            typeof(string),
            "Conversation ID",
            defaultValue: string.Empty);
    }
}