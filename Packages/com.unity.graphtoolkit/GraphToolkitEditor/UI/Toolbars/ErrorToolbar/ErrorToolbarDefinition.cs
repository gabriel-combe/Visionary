using System;
using System.Collections.Generic;

namespace Unity.GraphToolkit.Editor
{
    /// <summary>
    /// Class that defines the content of the error toolbar.
    /// </summary>
    [UnityRestricted]
    public class ErrorToolbarDefinition : ToolbarDefinition
    {
        /// <inheritdoc />
        public override IEnumerable<string> ElementIds => new[] { ErrorCountLabel.id, PreviousErrorButton.id, NextErrorButton.id };
    }
}
