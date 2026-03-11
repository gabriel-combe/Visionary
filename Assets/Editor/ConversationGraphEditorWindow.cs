// ConversationGraphEditorWindow.cs — Assets/Editor/

using Unity.GraphToolkit.Editor;
using UnityEditor;

[GraphEditorWindowDefinition(typeof(ConversationGraphObject))]
public class ConversationGraphEditorWindow : GraphViewEditorWindow
{
    protected override GraphTool CreateGraphTool()
        => GraphTool.Create<SimpleGraphTool<ConversationGraphModel, ConversationGraphObject>>(WindowID);

    public static void OpenGraph(ConversationGraphObject graphObject)
        => ShowGraphInExistingOrNewWindow<ConversationGraphEditorWindow>(graphObject);
}
