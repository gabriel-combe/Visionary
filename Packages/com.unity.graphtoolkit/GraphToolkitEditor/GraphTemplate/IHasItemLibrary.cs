using System;

namespace Unity.GraphToolkit.Editor
{
    [UnityRestricted]
    public interface IHasItemLibrary
    {
        ItemLibraryHelper GetItemLibraryHelper();
    }
}
