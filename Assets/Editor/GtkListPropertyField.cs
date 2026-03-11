// ─────────────────────────────────────────────────────────────────────────────
// GtkListPropertyField.cs
// Place in: Assets/Editor/
//
// A BaseModelPropertyField that renders a List<T> constant using Unity's native
// PropertyField + SerializedObject binding pipeline.
//
// Layout:
//   GtkListPropertyField<T>  (BaseModelPropertyField / VisualElement)
//     └── PropertyField  (bound to ListFieldWrapper<T>.m_Value via SerializedObject)
//           └── (Unity renders this as a ReorderableList for any List<T>)
//
// Data flow (write):
//   User edits list → ApplyModifiedProperties → OnValidate on wrapper →
//   UpdateConstantsValueCommand dispatched → GTK updates Constant → calls
//   UpdateDisplayedValue → wrapper synced from Constant → binding auto-refreshes
//
// Data flow (read/refresh):
//   GTK calls UpdateDisplayedValue → SyncFromConstants → wrapper.m_Value = copy →
//   next binding frame: SerializedObject.Update → PropertyField re-renders
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

/// <summary>
/// A GTK <see cref="BaseModelPropertyField"/> that renders a
/// <c>List<<typeparamref name="T"/>></c> node-option constant using Unity's
/// native PropertyField / SerializedObject binding infrastructure.
/// </summary>
/// <remarks>
/// Instances are created by
/// <see cref="DialogConstantEditorExtensions.BuildDialogConstantEditor"/> via
/// reflection, so the constructor signature must remain stable.
/// </remarks>
/// <typeparam name="T">The element type of the list constant.</typeparam>
public sealed class GtkListPropertyField<T> : BaseModelPropertyField
{
    readonly IReadOnlyList<Constant> m_Constants;
    readonly ListFieldWrapperBase    m_Wrapper;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Initialises the field, creates the ScriptableObject wrapper, and binds
    /// a <see cref="PropertyField"/> to its serialised list.
    /// </summary>
    /// <param name="commandTarget">GTK command dispatcher (the RootView).</param>
    /// <param name="owners">The graph-element models that own the constants.</param>
    /// <param name="constants">The constants that back this list option.</param>
    /// <param name="label">Display label (may be null).</param>
    public GtkListPropertyField(
        RootView                       commandTarget,
        IReadOnlyList<GraphElementModel> owners,
        IReadOnlyList<Constant>          constants,
        string                           label)
        : base(commandTarget)
    {
        m_Constants = constants;

        // ── Wrapper ───────────────────────────────────────────────────────────
        m_Wrapper = ListFieldWrapperFactory.GetOrCreate(constants, commandTarget);

        if (m_Wrapper == null)
        {
            // Orphan / preview node — render empty, no crash.
            return;
        }

        // ── SerializedObject + PropertyField ──────────────────────────────────
        var serializedObject = new SerializedObject(m_Wrapper);
        var property         = serializedObject.FindProperty("m_Value");

        if (property == null)
        {
            // Should not happen if IL emit succeeded; fail silently.
            return;
        }

        var propertyField = new PropertyField(property, label ?? string.Empty);
        propertyField.Bind(serializedObject);

        // Register with GTK's BaseModelPropertyField infrastructure (sets Field).
        // Pass null label: PropertyField handles its own label rendering.
        Setup(null, propertyField, null);

        // Add to the visual hierarchy.
        hierarchy.Add(propertyField);
    }

    // ── UpdateDisplayedValue ──────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Syncs the ScriptableObject wrapper from the current constant value.
    /// The bound PropertyField picks up the change automatically on the next
    /// binding update frame — no manual SerializedObject.Update() call needed.
    /// </remarks>
    public override void UpdateDisplayedValue()
    {
        if (m_Wrapper == null || m_Constants == null || m_Constants.Count == 0)
            return;

        ListFieldWrapperFactory.SyncFromConstants(m_Wrapper, m_Constants);
    }
}
