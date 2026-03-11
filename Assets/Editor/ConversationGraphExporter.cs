// ConversationGraphExporter.cs — Assets/Editor/

using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

public static class ConversationGraphExporter
{
    [MenuItem("Assets/Dialog/Export to ConversationGraph SO", true)]
    static bool ValidateExport()
        => AssetDatabase.GetAssetPath(Selection.activeObject)
            .EndsWith("." + ConversationGraphObject.AssetExtension);

    [MenuItem("Assets/Dialog/Export to ConversationGraph SO")]
    public static void ExportSelected()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        var graphObject = GraphObject.LoadGraphObjectAtPath<ConversationGraphObject>(path);
        if (graphObject == null) { Debug.LogWarning("[Exporter] Not found: " + path); return; }
        Export(graphObject, path);
    }

    public static ConversationGraph Export(ConversationGraphObject graphObject, string assetPath)
    {
        string soPath = assetPath.Replace("." + ConversationGraphObject.AssetExtension, ".asset");

        var so = AssetDatabase.LoadAssetAtPath<ConversationGraph>(soPath)
                 ?? ScriptableObject.CreateInstance<ConversationGraph>();
        if (!AssetDatabase.Contains(so))
            AssetDatabase.CreateAsset(so, soPath);

        so.startNodeIds.Clear();
        so.nodes.Clear();

        var graphModel = graphObject.GraphModel;

        // ── Collect nodes — use node.Guid (Hash128 persisted by GTK) ──────────
        var gtkNodes = new Dictionary<string, DialogNodeGTK>();
        var resolvedNextIds = new Dictionary<string, List<string>>();
        var resolvedChoiceIds = new Dictionary<string, List<List<string>>>();
        EntryNodeGTK entryNode = null;

        foreach (var abstractNode in graphModel.NodeModels)
        {
            if (abstractNode is EntryNodeGTK entry) { entryNode = entry; continue; }
            if (abstractNode is not DialogNodeGTK n) continue;
            string id = n.Guid.ToString();
            gtkNodes[id] = n;
            resolvedNextIds[id] = new List<string>();
            resolvedChoiceIds[id] = new List<List<string>>();
        }

        if (entryNode == null)
            Debug.LogWarning("[Exporter] No Entry Node found in graph — startNodeIds will be empty.");


        // ── Resolve connections ───────────────────────────────────────────────
        foreach (var (id, gtkNode) in gtkNodes)
        {
            var choices = gtkNode.GetListOption<PlayerChoiceGTK>(DialogNodeGTK.k_Choices);
            var choiceLists = resolvedChoiceIds[id];
            while (choiceLists.Count < choices.Count)
                choiceLists.Add(new List<string>());

            foreach (var port in gtkNode.OutputsById.Values)
            {
                var wires = port.GetConnectedWires();
                if (wires.Count == 0) continue;

                // Use OutputsByDisplayOrder so matching is purely positional — immune to text collisions.
                // Port order: [In] on inputs, [Out] then [choice_0, choice_1, ...] on outputs.
                int choicePortIndex = -1;
                if (port.Title != "Out")
                {
                    int ci = 0;
                    foreach (var p in gtkNode.OutputsByDisplayOrder)
                    {
                        if (p.Title == "Out") continue;
                        if (p == port) { choicePortIndex = ci; break; }
                        ci++;
                    }
                }

                if (port.Title == "Out")
                {
                    foreach (var wire in wires)
                        if (wire.ToPort?.NodeModel is DialogNodeGTK t)
                            resolvedNextIds[id].Add(NodeId(t));
                }
                else if (choicePortIndex >= 0 && choicePortIndex < choiceLists.Count)
                {
                    foreach (var wire in wires)
                        if (wire.ToPort?.NodeModel is DialogNodeGTK t)
                            choiceLists[choicePortIndex].Add(NodeId(t));
                }
            }
        }

        // ── Resolve startNodeIds from Entry Node ──────────────────────────────
        if (entryNode != null)
        {
            foreach (var port in entryNode.OutputsById.Values)
                foreach (var wire in port.GetConnectedWires())
                    if (wire.ToPort?.NodeModel is DialogNodeGTK target)
                        so.startNodeIds.Add(NodeId(target));
        }

        // Read conversationId from the Entry Node option
        so.conversationId = entryNode != null
            ? entryNode.GetOption<string>(EntryNodeGTK.k_ConversationId, "")
            : string.Empty;

        if (so.startNodeIds.Count == 0)
            Debug.LogWarning("[Exporter] Entry Node has no connections — startNodeIds is empty.");

        // ── Convert to runtime ────────────────────────────────────────────────
        foreach (var (id, gtkNode) in gtkNodes)
        {
            var choices = gtkNode.GetListOption<PlayerChoiceGTK>(DialogNodeGTK.k_Choices);
            var conditions = gtkNode.GetListOption<DialogCondition>(DialogNodeGTK.k_Conditions);
            var onEnterOutcomes = gtkNode.GetListOption<DialogOutcome>(DialogNodeGTK.k_OnEnterOutcomes);
            var choiceLists = resolvedChoiceIds[id];

            var runtimeNode = new DialogNode
            {
                nodeId = id,
                npcText = gtkNode.GetOption<string>(DialogNodeGTK.k_NpcText, ""),
                speaker = gtkNode.GetOption<Speaker>(DialogNodeGTK.k_Speaker),
                conditions = new List<DialogCondition>(conditions),
                onEnterOutcomes = new List<DialogOutcome>(onEnterOutcomes),
                nextNodeIds = new List<string>(resolvedNextIds[id]),
                choices = new List<PlayerChoice>()
            };

            for (int i = 0; i < choices.Count; i++)
            {
                runtimeNode.choices.Add(new PlayerChoice
                {
                    playerText = choices[i].playerText,
                    nextNodeIds = i < choiceLists.Count ? new List<string>(choiceLists[i]) : new List<string>(),
                    outcomes = new List<DialogOutcome>(choices[i].outcomes ?? new())
                });
            }

            so.nodes.Add(runtimeNode);
        }

        EditorUtility.SetDirty(so);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Exporter] {so.nodes.Count} nodes → {soPath}");
        return so;
    }

    static string NodeId(DialogNodeGTK n) => n.Guid.ToString();
}