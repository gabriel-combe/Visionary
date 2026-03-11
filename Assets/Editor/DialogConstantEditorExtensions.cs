using System;
using System.Collections.Generic;
using Unity.GraphToolkit.CSO;
using Unity.GraphToolkit.Editor;
using UnityEngine;
using UnityEngine.UIElements;

[GraphElementsExtensionMethodsCache(
    typeof(RootView),
    GraphElementsExtensionMethodsCacheAttribute.toolDefaultPriority)]
public static class DialogConstantEditorExtensions
{
    public static BaseModelPropertyField BuildDialogConstantEditor(
        this ConstantEditorBuilder builder,
        IReadOnlyList<Constant> constants)
    {
        if (constants == null || constants.Count == 0)
            return new ConstantField(constants, builder.ConstantOwners,
                                     builder.CommandTarget, builder.Label);

        var valueType = constants[0].Type;

        // List<T> → GtkListPropertyField<T>
        if (valueType.IsGenericType &&
            valueType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var itemType = valueType.GetGenericArguments()[0];
            var fieldType = typeof(GtkListPropertyField<>).MakeGenericType(itemType);
            try
            {
                return (BaseModelPropertyField)Activator.CreateInstance(
                    fieldType,
                    builder.CommandTarget,
                    builder.ConstantOwners,
                    constants,
                    builder.Label);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DialogConstantEditorExtensions] Failed to create GtkListPropertyField<{itemType.Name}>.\n{ex}");
                return new MissingFieldEditor(builder.CommandTarget, builder.Label ?? valueType.Name);
            }
        }

        // string with label "Conversation ID" or "NPC Text" → multiline text field
        if (valueType == typeof(string) &&
            (builder.Label == "Conversation ID" || builder.Label == "NPC Text"))
            return new MultilineStringConstantField(builder.CommandTarget, constants, builder.Label, minLines: 2);

        // All other types → GTK default
        return new ConstantField(constants, builder.ConstantOwners,
                                 builder.CommandTarget, builder.Label);
    }
}

/// <summary>
/// Renders a string constant as a multiline, word-wrapped TextField with a minimum width.
/// </summary>
public class MultilineStringConstantField : BaseModelPropertyField
{
    const float k_LineHeight = 9;
    const float k_MaxWidth = 500f;

    readonly IReadOnlyList<Constant> m_Constants;
    readonly TextField m_TextField;

    public MultilineStringConstantField(
        ICommandTarget commandTarget,
        IReadOnlyList<Constant> constants,
        string label,
        int minLines = 2)
        : base(commandTarget)
    {
        m_Constants = constants;

        m_TextField = new TextField(label)
        {
            multiline = true,
            isDelayed = true,
            value = constants.Count > 0 ? constants[0].ObjectValue as string ?? "" : "",
        };

        m_TextField.style.minHeight = minLines * k_LineHeight;
        m_TextField.style.whiteSpace = WhiteSpace.Normal;

        var textInput = m_TextField.Q<VisualElement>("unity-text-input");
        if (textInput != null)
        {
            textInput.style.minHeight = minLines * k_LineHeight;
            textInput.style.whiteSpace = WhiteSpace.Normal;
            textInput.style.overflow = Overflow.Hidden;
            textInput.style.flexShrink = 1;
        }

        // Apply maxWidth once after attachment, outside the layout pass via schedule.
        m_TextField.RegisterCallback<AttachToPanelEvent>(_ =>
        {
            m_TextField.schedule.Execute(() =>
            {
                var parent = m_TextField.parent;
                if (parent == null) return;
                float w = Mathf.Max(parent.resolvedStyle.width, k_MaxWidth);
                if (w <= 0) return;
                m_TextField.style.maxWidth = w;
                if (textInput != null)
                    textInput.style.maxWidth = w;
            });
        });

        m_TextField.RegisterValueChangedCallback(evt =>
            CommandTarget.Dispatch(new UpdateConstantsValueCommand(m_Constants, evt.newValue)));

        hierarchy.Add(m_TextField);
    }

    public override void UpdateDisplayedValue()
    {
        if (m_Constants.Count > 0)
            m_TextField.SetValueWithoutNotify(m_Constants[0].ObjectValue as string ?? "");
    }
}