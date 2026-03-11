using System;
using UnityEngine.UIElements;

namespace Unity.GraphToolkit.Editor
{
    /// <summary>
    /// Base class for model views.
    /// </summary>
    [UnityRestricted]
    public abstract class View : VisualElement
    {
        /// <summary>
        /// Instantiates the VisualElements that makes the UI.
        /// </summary>
        public abstract void BuildUITree();
    }
}
