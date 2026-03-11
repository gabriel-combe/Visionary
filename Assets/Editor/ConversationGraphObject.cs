// ConversationGraphObject.cs — Assets/Editor/

using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// GTK asset for the conversation graph (NodeModel API).
/// Double-click in the Project window to open the editor.
/// </summary>
[GraphObjectDefinition(AssetExtension)]
public class ConversationGraphObject : GraphObject
{
    public const string AssetExtension = "convgraph";

    public string conversationId;

    [MenuItem("Assets/Create/Dialog/Conversation Graph")]
    static void CreateAsset()
    {
        GraphObjectCreationHelpers.PromptToCreateGraphObject(
            typeof(ConversationGraphObject),
            new ConversationGraphTemplate());
    }

    [UnityEditor.Callbacks.OnOpenAsset]
    static bool OnOpenAsset(int instanceId, int line)
    {
        var obj = EditorUtility.EntityIdToObject(instanceId);
        if (obj is not ConversationGraphObject graphObj) return false;
        ConversationGraphEditorWindow.OpenGraph(graphObj);
        return true;
    }
}


/// <summary>
/// Graph template that spawns an Entry Node at the center of the canvas on creation.
/// </summary>
public class ConversationGraphTemplate : GraphTemplate<ConversationGraphModel>
{
    public ConversationGraphTemplate()
        : base("Conversation Graph", ConversationGraphObject.AssetExtension) { }

    public override void InitBasicGraph(GraphModel graphModel)
    {
        graphModel.CreateNode<EntryNodeGTK>("Entry", position: UnityEngine.Vector2.zero);
    }
}