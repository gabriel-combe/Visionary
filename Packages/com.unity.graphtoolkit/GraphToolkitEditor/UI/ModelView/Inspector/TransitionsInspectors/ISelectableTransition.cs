using System;

namespace Unity.GraphToolkit.Editor
{
    public interface ISelectableTransition : ISelectableElement
    {
        /// <summary>
        /// Starts text editing: focus is given to the title text field.
        /// </summary>
        void BeginEditing();
    }
}
