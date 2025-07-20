using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a UI element that manages a grid layout for its children.
/// </summary>
public class NP_GridLayout : NP_UIElements
{
    protected HorizontalLayoutGroup horizontalLayoutGroup;
    protected VerticalLayoutGroup verticalLayoutGroup;
    protected UnityEngine.UI.GridLayoutGroup gridLayoutGroup; // Note: full namespace to avoid conflict with enum

    protected override void Awake()
    {
        base.Awake();
        // Get references to potential layout groups
        horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
        verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
        gridLayoutGroup = GetComponent<UnityEngine.UI.GridLayoutGroup>();
    }

    public void SetDirection(LayoutDirection layoutDirection, GridLayoutGroup gridLayoutGroupType)
    {
        // This method implies controlling which layout group component is active or
        // how it's configured based on the desired direction.
        // Example: Only one layout group should be active at a time.
        DisableAllLayoutGroups();

        switch (layoutDirection)
        {
            case LayoutDirection.Horizontal:
                if (horizontalLayoutGroup == null) horizontalLayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
                horizontalLayoutGroup.enabled = true;
                break;
            case LayoutDirection.Vertical:
                if (verticalLayoutGroup == null) verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
                verticalLayoutGroup.enabled = true;
                break;
                // If you had a 'Grid' layoutDirection, you'd enable gridLayoutGroup here.
        }
        // You might use gridLayoutGroupType to further configure settings,
        // e.g., spacing, padding, child alignment based on the type.
        // This part would require more specific logic based on your design.
    }

    public void SetNumberOfChildren(int count, GridLayoutGroup gridLayoutGroupType)
    {
        // This method typically involves instantiating or managing a fixed number of child elements
        // within the layout. It's not directly related to the layout group components themselves,
        // but rather the *content* of the grid.
        // You'd typically iterate through existing children, create new ones, or destroy excess ones.
        Debug.Log($"Setting number of children for grid type {gridLayoutGroupType} to: {count}");

        // Example: Clear existing children and instantiate new ones (very simplified)
        // For actual implementation, you'd use object pooling or more robust management.
        // foreach (Transform child in transform)
        // {
        //     Destroy(child.gameObject);
        // }
        // for (int i = 0; i < count; i++)
        // {
        //     // Instantiate a prefab or create a new GameObject as a child
        //     // GameObject newChild = Instantiate(yourChildPrefab, transform);
        // }
    }

    private void DisableAllLayoutGroups()
    {
        if (horizontalLayoutGroup != null) horizontalLayoutGroup.enabled = false;
        if (verticalLayoutGroup != null) verticalLayoutGroup.enabled = false;
        if (gridLayoutGroup != null) gridLayoutGroup.enabled = false;
    }
}
