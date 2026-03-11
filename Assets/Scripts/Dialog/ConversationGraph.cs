using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/Conversation Graph SO", fileName = "ConversationGraph")]
public class ConversationGraph : ScriptableObject
{
    [Tooltip("Unique identifier for this conversation.")]
    public string conversationId;

    [Tooltip("Populated automatically on export from the Entry Node connections. " +
             "If multiple IDs: one is picked at random among those whose conditions are met.")]
    public List<string> startNodeIds = new();

    public List<DialogNode> nodes = new();

    public DialogNode GetNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) return null;
        return nodes.Find(n => n.nodeId == nodeId);
    }
}
