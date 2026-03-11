using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.GraphToolkit.Editor.ContextualMenuItems
{
    /// <summary>
    /// Helpers for defining the menu items to show in a contextual menu.
    /// </summary>
    static class ContextualMenuHelpers
    {
        public static Dictionary<ContextualMenuCategory, List<ContextualMenuItem>> GetMenuItemsForSelection(List<GraphElementModel> selection)
        {
            // If the selection is null or empty, return null.
            if (selection == null || selection.Count == 0)
                return null;

            // Combine the contextual menu items from all selected elements, keeping only the same items between types of selected elements.
            var menuItems = new List<ContextualMenuItem>();
            var uniqueSelectedTypes = new List<Type>();

            foreach (var elementModel in selection)
            {
                // If there are wires in the selection, we don't want to show contextual menu items for them.
                if (elementModel is WireModel)
                    continue;

                var type = elementModel.GetType();
                if (elementModel is IHasContextualMenuItems hasContextualMenuItems && !uniqueSelectedTypes.Contains(type))
                {
                    uniqueSelectedTypes.Add(type);
                    IntersectMenuItems(menuItems, hasContextualMenuItems.ContextualMenuItems);
                }
            }

            return CategorizeMenuItems(menuItems);
        }

        /// <summary>
        /// Organizes the <see cref="ContextualMenuItem"/>s from a given list by <see cref="ContextualMenuCategory"/>.
        /// </summary>
        /// <param name="itemsList">The graph view.</param>
        /// <returns>A dictionary containing lists of <see cref="ContextualMenuItem"/>s paired with their category.</returns>
        public static Dictionary<ContextualMenuCategory, List<ContextualMenuItem>> CategorizeMenuItems(IReadOnlyList<ContextualMenuItem> itemsList)
        {
            var categoryGroups = new Dictionary<ContextualMenuCategory, List<ContextualMenuItem>>();
            foreach (var contextualMenuItem in itemsList)
            {
                // If the category is not already in the dictionary, create a new list for it.
                if (!categoryGroups.ContainsKey(contextualMenuItem.Category))
                    categoryGroups[contextualMenuItem.Category] = new List<ContextualMenuItem>();

                // If the index is negative or out of bounds, add the item to the end of the list.
                if (contextualMenuItem.IndexInCategory < 0 || contextualMenuItem.IndexInCategory >= categoryGroups[contextualMenuItem.Category].Count)
                    categoryGroups[contextualMenuItem.Category].Add(contextualMenuItem);
                else
                    categoryGroups[contextualMenuItem.Category].Insert(contextualMenuItem.IndexInCategory, contextualMenuItem);
            }

            return categoryGroups;
        }

        /// <summary>
        /// Combines the provided list of <see cref="ContextualMenuItem"/>s with the existing ones in the provided list.
        /// </summary>
        /// <param name="menuItems">The current list of items.</param>
        /// <param name="otherMenuItems">The list of items to combine with the current list.</param>
        static void IntersectMenuItems(List<ContextualMenuItem> menuItems, IReadOnlyList<ContextualMenuItem> otherMenuItems)
        {
            if (menuItems.Count == 0)
            {
                // If the menuItems list is empty, add all items from the provided list.
                menuItems.AddRange(otherMenuItems);
            }
            else
            {
                // Only keep items that are also in the provided list.
                for (var i = menuItems.Count - 1; i >= 0; i--)
                {
                    if (!otherMenuItems.Contains(menuItems[i]))
                        menuItems.RemoveAt(i);
                }
            }
        }

        // Predefined menu items:

        // ViewSelection menu items:
        public static ContextualMenuItem cutItem = new(ContextualMenuCategory.CutCopyPaste, "Cut");
        public static ContextualMenuItem copyItem = new(ContextualMenuCategory.CutCopyPaste, "Copy");
        public static ContextualMenuItem pasteItem = new(ContextualMenuCategory.CutCopyPaste, "Paste");
        public static ContextualMenuItem renameItem = new(ContextualMenuCategory.RenameDuplicateDelete, "Rename");
        public static ContextualMenuItem duplicateItem = new(ContextualMenuCategory.RenameDuplicateDelete, "Duplicate");
        public static ContextualMenuItem deleteItem = new(ContextualMenuCategory.RenameDuplicateDelete, "Delete");
        public static ContextualMenuItem selectUnusedItem = new(ContextualMenuCategory.Organization, "Select Unused");
        public static ContextualMenuItem pasteAsNewMenuItem = new(ContextualMenuCategory.CutCopyPaste, "Paste as New");

        // Common graph element menu items:
        public static ContextualMenuItem createPlacematItem = new(ContextualMenuCategory.OrganizationalElements, "Create Placemat");
        public static ContextualMenuItem createLocalSubgraphFromSelectionItem = new(ContextualMenuCategory.Conversions, "Create Local Subgraph from Selection");
        public static ContextualMenuItem frameSelectionItem = new(ContextualMenuCategory.Modifications, "Frame Selection");
        public static ContextualMenuItem colorItem = new(ContextualMenuCategory.Modifications, "Color");
        public static ContextualMenuItem alignAndDistributeElementsItem = new(ContextualMenuCategory.Organization, "Align and Distribute Elements");

        // GraphView menu items:
        public static ContextualMenuItem addNodeItem = new(ContextualMenuCategory.FunctionalElements, "Add Node");
        public static ContextualMenuItem createStickyNoteItem = new(ContextualMenuCategory.OrganizationalElements, "Create Sticky Note");
        public static ContextualMenuItem createEmptyLocalSubgraphItem = new(ContextualMenuCategory.OrganizationalElements, "Create Empty Local Subgraph");
        public static ContextualMenuItem selectAllItem = new(ContextualMenuCategory.Organization, "Select All");
        public static ContextualMenuItem showOverlayMenuItem = new(ContextualMenuCategory.External, "Show Overlay Menu");

        // Node menu items:
        public static ContextualMenuItem editSubtitleItem = new(ContextualMenuCategory.Modifications, "Edit Subtitle");
        public static ContextualMenuItem bypassNodeItem = new(ContextualMenuCategory.Modifications, "Bypass Node");
        public static ContextualMenuItem disableNodeItem = new(ContextualMenuCategory.Modifications, "Disable Node");
        public static ContextualMenuItem disconnectAllWiresItem = new(ContextualMenuCategory.Modifications, "Disconnect All Wires");
        public static ContextualMenuItem toggleCollapseItem = new(ContextualMenuCategory.Modifications, "Toggle Collapse");
        public static ContextualMenuItem deleteAndReconnectItem = new(ContextualMenuCategory.RenameDuplicateDelete, "Delete and reconnect");

        // State menu items:
        public static ContextualMenuItem createTransitionMenuItem = new(ContextualMenuCategory.FunctionalElements, "Create Transition");
        public static ContextualMenuItem createLocalTransitionMenuItem = new(ContextualMenuCategory.FunctionalElements, "Create Local Transition");
        public static ContextualMenuItem createOnEnterTransitionMenuItem = new(ContextualMenuCategory.FunctionalElements, "Create OnEnter Transition");
        public static ContextualMenuItem createSelfTransitionMenuItem = new(ContextualMenuCategory.FunctionalElements, "Create Self Transition");
        public static ContextualMenuItem setAsDefaultStateMenuItem = new(ContextualMenuCategory.Modifications, "Set Default State");

        // Subgraph menu items:
        public static ContextualMenuItem extractContentsToPlacematItem = new(ContextualMenuCategory.Conversions, "Extract Contents to Placemat");
        public static ContextualMenuItem openLocalSubgraphItem = new(ContextualMenuCategory.AssetManagement, "Open Local Subgraph");
        public static ContextualMenuItem openAssetSubgraphItem = new(ContextualMenuCategory.AssetManagement, "Open Asset Subgraph");
        public static ContextualMenuItem unpackToLocalSubgraphItem = new(ContextualMenuCategory.AssetManagement, "Unpack to Local Subgraph");
        public static ContextualMenuItem findAssetInProjectItem = new(ContextualMenuCategory.AssetManagement, "Find Asset in Project");
        public static ContextualMenuItem convertToAssetSubgraphItem = new(ContextualMenuCategory.AssetManagement, "Convert to Asset Subgraph");

        // Variable and constant menu items:
        public static ContextualMenuItem itemizeItem = new(ContextualMenuCategory.Modifications, "Itemize");
        public static ContextualMenuItem convertToConstantItem = new(ContextualMenuCategory.Conversions, "Convert to Constant");
        public static ContextualMenuItem convertToVariableItem = new(ContextualMenuCategory.Conversions, "Convert to Variable");

        // Blackboard menu items:
        public static ContextualMenuItem createVariableItem = new(ContextualMenuCategory.FunctionalElements, "Create Variable");
        public static ContextualMenuItem createGroupItem = new(ContextualMenuCategory.FunctionalElements, "Create Group");

        // Ports menu items:
        public static ContextualMenuItem addNodeFromPortItem = new(ContextualMenuCategory.FunctionalElements, "Add Node from port");
        public static ContextualMenuItem createVariableFromPortItem = new(ContextualMenuCategory.FunctionalElements, "Create Variable from port");
        public static ContextualMenuItem copyValueItem = new(ContextualMenuCategory.CutCopyPaste, "Copy Value");
        public static ContextualMenuItem pasteValueItem = new(ContextualMenuCategory.CutCopyPaste, "Paste Value");
        public static ContextualMenuItem expandPortItem = new(ContextualMenuCategory.Modifications, "Expand Port");
        public static ContextualMenuItem collapsePortItem = new(ContextualMenuCategory.Modifications, "Collapse Port");

        // Wire menu items:
        public static ContextualMenuItem insertNodeItem = new(ContextualMenuCategory.FunctionalElements, "Insert Node");
        public static ContextualMenuItem insertJunctionPointItem = new(ContextualMenuCategory.FunctionalElements, "Insert Junction Point");
        public static ContextualMenuItem convertToPortalsItem = new(ContextualMenuCategory.Conversions, "Convert to Portals");
        public static ContextualMenuItem reorderWireItem = new(ContextualMenuCategory.Modifications, "Reorder Wire");

        // Context and block menu items:
        public static ContextualMenuItem addBlockItem = new(ContextualMenuCategory.FunctionalElements, "Add Block");
        public static ContextualMenuItem insertBlockAboveItem = new(ContextualMenuCategory.FunctionalElements, "Insert Block Above");
        public static ContextualMenuItem insertBlockBelowItem = new(ContextualMenuCategory.FunctionalElements, "Insert Block Below");
        public static ContextualMenuItem convertToBlockSubgraphItem = new(ContextualMenuCategory.Conversions, "Convert to Block Subgraph");

        // Sticky Note menu items:
        public static ContextualMenuItem fitToTextItem = new(ContextualMenuCategory.Modifications, "Fit to Text");
        public static ContextualMenuItem fontSizeAndThemeItem = new(ContextualMenuCategory.Modifications, "Font Size");

        // Placemat menu items:
        public static ContextualMenuItem deleteAndSelectContentsItem = new(ContextualMenuCategory.RenameDuplicateDelete, "Delete and Select Contents");
        public static ContextualMenuItem smartResizeItem = new(ContextualMenuCategory.Modifications, "Smart Resize");
        public static ContextualMenuItem reorderPlacematItem = new(ContextualMenuCategory.Modifications, "Reorder Placemat");
        public static ContextualMenuItem selectAllPlacematContentsItem = new(ContextualMenuCategory.Organization, "Select All Placemat Contents");

        // Portals menu items:
        public static ContextualMenuItem createOppositePortalItem = new(ContextualMenuCategory.Conversions, "Create Opposite Portal");
        public static ContextualMenuItem revertToWireItem = new(ContextualMenuCategory.Conversions, "Revert to Wire");
        public static ContextualMenuItem revertAllToWiresItem = new(ContextualMenuCategory.Conversions, "Revert All to Wire");
    }
}
