using System;

namespace Unity.GraphToolkit.Editor
{
    /// <summary>
    /// Model for entry portals.
    /// </summary>
    [Serializable]
    [UnityRestricted]
    public class WirePortalEntryModel : WirePortalModel, ISingleInputPortNodeModel
    {
        /// <inheritdoc />
        public PortModel InputPort { get; protected set; }

        /// <inheritdoc />
        public override bool CanHaveAnotherPortalWithSameDirectionAndDeclaration() => false;

        /// <inheritdoc />
        protected override void OnDefineNode(NodeDefinitionScope scope)
        {
            InputPort = scope.AddInputPort("", GetPortDataTypeHandle(), PortType);
        }
    }
}
