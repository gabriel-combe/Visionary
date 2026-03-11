// DialogNodeGTK.cs — Assets/Editor/

using System;
using System.Collections.Generic;
using Unity.GraphToolkit;
using Unity.GraphToolkit.Editor;
using UnityEngine;

[Serializable]
public class PlayerChoiceGTK
{
    [TextArea(1, 2)]
    public string playerText = string.Empty;
    public List<DialogOutcome> outcomes = new List<DialogOutcome>();
}

[Serializable]
[LibraryItem(typeof(ConversationGraphModel), "Dialog/Dialog Node")]
public class DialogNodeGTK : NodeModel
{
    public const string k_NpcText = "npcText";
    public const string k_Speaker = "speaker";
    public const string k_Conditions = "conditions";
    public const string k_SpeakerSide = "speakerSide";
    public const string k_OnEnterOutcomes = "onEnterOutcomes";
    public const string k_Choices = "choices";

    // ── Read helpers (used by the exporter) ───────────────────────────────────

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

    public List<T> GetListOption<T>(string id)
        => GetOption<List<T>>(id) ?? new List<T>();

    // ── Node definition ───────────────────────────────────────────────────────

    protected override void OnDefineNode(NodeDefinitionScope scope)
    {
        // Input port — Multi so multiple nodes can point to this one
        var inPort = scope.AddInputPort<DialogFlow>("In");
        inPort.Capacity = PortCapacity.Multi;

        var choices = GetListOption<PlayerChoiceGTK>(k_Choices);

        if (choices == null || choices.Count == 0)
        {
            // Linear node: single Out port, Multi so it can branch to several targets
            var outPort = scope.AddOutputPort<DialogFlow>("Out");
            outPort.Capacity = PortCapacity.Multi;
        }
        else
        {
            // One port per choice, each Multi so a choice can also branch
            for (int i = 0; i < choices.Count; i++)
            {
                // Port label = playerText with GTK-forbidden chars stripped. portId stays index-based
                // so renaming a choice never breaks existing wires.
                string label = string.IsNullOrWhiteSpace(choices[i]?.playerText)
                    ? $"Choice {i + 1}"
                    : System.Text.RegularExpressions.Regex.Replace(choices[i].playerText, @"[./\\]", "");
                var port = scope.AddOutputPort<DialogFlow>(label, portId: $"choice_{i}");
                port.Capacity = PortCapacity.Multi;
            }
        }

        scope.AddNodeOption(k_NpcText, typeof(string), "NPC Text", defaultValue: string.Empty);
        scope.AddNodeOption(k_Speaker, typeof(Speaker), "Speaker");
        scope.AddNodeOption(k_SpeakerSide, typeof(SpeakerSide), "Speaker Side");
        scope.AddNodeOption(k_Conditions, typeof(List<DialogCondition>), "Conditions");
        scope.AddNodeOption(k_OnEnterOutcomes, typeof(List<DialogOutcome>), "On Enter Outcomes");
        scope.AddNodeOption(k_Choices, typeof(List<PlayerChoiceGTK>), "Choices");
    }
}

// Marker type for dialog port connections.
public class DialogFlow { }