using UnityEngine;
using UnityEngine.UI;
using System;
using NP_UI;

// This is a static class, meaning you can call its methods directly without creating an instance.
public static class UIMenuGenerator
{
    // --- Public Enums (can be placed outside this class if desired, but kept here for self-containment) ---
    public enum MenuAlignment
    {
        TopLeft, TopCenter, TopRight,
        BottomLeft, BottomCenter, BottomRight,
        LeftSide, RightSide, TopPanel, BottomPanel,
        Center,
        Stretch,
        Left,
        Right
    }

    public enum GridLayoutType
    {
        Horizontal, // Items lay out horizontally, content scrolls horizontally
        Vertical,   // Items lay out vertically, content scrolls vertically
        Grid,        // Items lay out in a grid, content scrolls vertically
        Stretch
    }

    /// <summary>
    /// Generates a scrollable UI menu with a GridLayoutGroup purely from code,
    /// using a MenuData struct/class for all parameters.
    /// </summary>
    /// <param name="config">The MenuData struct/class containing all necessary parameters.</param>
    /// <returns>The root GameObject of the generated menu, or null if creation fails.</returns>
    public static GameObject CreateScrollableGridMenu(MenuData config)
    {
        // --- 1. Validate parameters ---
        float actualScreenCoveragePercent = config.ScreenCoveragePercent; // Use a local variable to allow modification
        if (!ValidateParameters(config.ParentCanvas, ref actualScreenCoveragePercent)) return null;

        // --- 2. Create Root Menu GameObject ---
        GameObject rootMenuGO = CreateRootMenu(config);

        if (rootMenuGO == null) return null;
        RectTransform menuRootRect = rootMenuGO.GetComponent<RectTransform>();

        // --- 3. Create Viewport and Content GameObjects ---
        RectTransform viewportRect = CreateViewport(rootMenuGO, config.ViewportBackgroundColor);
        RectTransform contentRect = CreateContent(rootMenuGO.GetComponent<NP_Menu>(), viewportRect);

        // --- 4. Add and Configure ScrollRect ---
        ScrollRect
            scrollRect =
                rootMenuGO
                    .GetComponentInChildren<
                        ScrollRect>(); //AddScrollRect(rootMenuGO, viewportRect, contentRect, config.LayoutType);

        NP_Menu npMenu = rootMenuGO.GetComponent<NP_Menu>();

        // --- 5. Add and Configure ContentSizeFitter ---
        AddContentSizeFitter(contentRect, config.LayoutType);

        // --- 6. Add and Configure GridLayoutGroup ---
        AddGridLayoutGroup(config, contentRect);

        //Just For Example!
        // --- 7. Populate with example items ---
        //PopulateMenuItems(contentRect, config.ItemCount, config.ItemBackgroundColor, config.ItemTextColor);
        SetFields(config, npMenu);
        ApplyComponent(config, rootMenuGO);
        SetColors(config, npMenu);
        // --- 8. Force Layout Rebuilds ---
        ForceRebuildLayouts(menuRootRect, viewportRect, contentRect);

        rootMenuGO.SetActive(config.IsAlwaysOn);
        return rootMenuGO;
    }

    private static void SetColors(MenuData config, NP_Menu menu)
    {
        if (menu == null)
        {
            return;
        }

        Color npColor = config.MenuBackgroundColor;
        menu.backgroundImage.color = new Color(npColor.r, npColor.g, npColor.b, npColor.a);
        
    }
    private static void SetFields(MenuData config, NP_Menu menu)
    {
        if (menu == null)
        {
            return;
        }
        menu.headLineText.SetText(config.MenuName);
        menu.menuData = config;
    }

    private static void ApplyComponent(MenuData config, GameObject rootMenuGo)
    {
        rootMenuGo.AddComponent(config.ItemType);
    }
    
    // =================================================================================================
    // --- PRIVATE STATIC HELPER FUNCTIONS
    // =================================================================================================

    /// <summary>
    /// Helper: Validates essential parameters and clamps values if necessary.
    /// </summary>
    private static bool ValidateParameters(RectTransform parentCanvas, ref float screenCoveragePercent)
    {
        if (parentCanvas == null)
        {
            Debug.LogError("UIMenuGenerator: 'parentCanvas' is null. Cannot create menu.");
            return false;
        }
        if (screenCoveragePercent <= 0 || screenCoveragePercent > 1)
        {
            Debug.LogWarning($"UIMenuGenerator: 'screenCoveragePercent' should be between 0 and 1. Clamping to 0.3. Current: {screenCoveragePercent}");
            screenCoveragePercent = Mathf.Clamp(screenCoveragePercent, 0.01f, 1f);
        }
        return true;
    }

    /// <summary>
    /// Helper: Creates the root GameObject for the menu and sets its basic properties.
    /// </summary>
    private static GameObject CreateRootMenu(MenuData menuData)
    {
        GameObject rootGO = CreateNewRootMenuGameObject(menuData); 
        
        rootGO.transform.SetParent(menuData.ParentCanvas, false);

        RectTransform rootRect = rootGO.GetComponent<RectTransform>();

        SetRectTransformProperties(rootRect, menuData.Alignment, menuData.ScreenCoveragePercent, menuData.ParentCanvas);
        return rootGO;
    }

    private static GameObject CreateNewRootMenuGameObject(MenuData config)
    {
        bool isSubclassOfNPGeneric = config.ItemType.Type.IsSubclassOf(typeof(NpGenericMenu));
        bool isSubclassOfFormMenu = config.ItemType.Type.IsSubclassOf(typeof(FormMenu));

        MenuType menuType = MenuType.Regular;
        
        if (isSubclassOfFormMenu)
        {
            menuType = MenuType.Form;
            //if (config.MenuType.)
            {
                
            }
        }

        if (isSubclassOfNPGeneric  && !isSubclassOfFormMenu)
        {
            menuType = MenuType.Regular;
        }
        
        
        return NP_MenusManager.Instance.GetNewNpMenu(config, menuType).gameObject;
    }
    
    /// <summary>
    /// Helper: Creates and configures the ScrollRect's Viewport GameObject.
    /// </summary>
    private static RectTransform CreateViewport(GameObject parentGO, Color bgColor)
    {
        NP_Menu npMenu = parentGO.GetComponent<NP_Menu>();
        GameObject viewportGO = npMenu.viewPort;

        RectTransform viewportRect = viewportGO.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;

        Image viewportImage = viewportGO.GetComponent<Image>();
        //viewportImage.color = bgColor;
        viewportImage.raycastTarget = false;

        Mask viewportMask = viewportGO.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        LayoutElement viewportLayoutElement = viewportGO.AddComponent<LayoutElement>();
        viewportLayoutElement.flexibleHeight = 1;
        viewportLayoutElement.flexibleWidth = 1; // Crucial for Viewport to fill parent

        return viewportRect;
    }

    /// <summary>
    /// Helper: Creates and configures the ScrollRect's Content GameObject.
    /// </summary>
    private static RectTransform CreateContent(NP_Menu npMenu, RectTransform viewportRect)
    {
        GameObject contentGO = npMenu.scrollRect.content.gameObject;
        contentGO.transform.SetParent(viewportRect.transform, false);

        RectTransform contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;

        return contentRect;
    }

    /// <summary>
    /// Helper: Adds and configures the ScrollRect component.
    /// </summary>
    private static ScrollRect AddScrollRect(
        GameObject rootGO, RectTransform viewportRect, RectTransform contentRect,
        GridLayoutType layoutType)
    {
        ScrollRect scrollRect = rootGO.AddComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = (layoutType == GridLayoutType.Horizontal);
        scrollRect.vertical = (layoutType == GridLayoutType.Vertical || layoutType == GridLayoutType.Grid);
        scrollRect.elasticity = 0.1f;
        scrollRect.scrollSensitivity = 10f;
        return scrollRect;
    }

    /// <summary>
    /// Helper: Adds and configures the ContentSizeFitter component.
    /// </summary>
    private static void AddContentSizeFitter(RectTransform targetRect, GridLayoutType layoutType)
    {
        ContentSizeFitter contentSizeFitter = targetRect.gameObject.GetComponent<ContentSizeFitter>();
        contentSizeFitter.horizontalFit = (layoutType == GridLayoutType.Horizontal || layoutType == GridLayoutType.Grid) ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        contentSizeFitter.verticalFit = (layoutType == GridLayoutType.Vertical || layoutType == GridLayoutType.Grid) ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
    }

    /// <summary>
    /// Helper: Adds and configures the GridLayoutGroup component.
    /// </summary>
    private static void AddGridLayoutGroup(MenuData menuData, 
        RectTransform targetRect)
    {
        GridLayoutGroup gridLayoutGroup = targetRect.gameObject.GetComponent<GridLayoutGroup>();
        gridLayoutGroup.padding = menuData.GridPadding;
        gridLayoutGroup.spacing = menuData.ItemSpacing;
        gridLayoutGroup.cellSize = menuData.ItemCellSize;
        gridLayoutGroup.childAlignment = menuData.GridChildAlignment;
        gridLayoutGroup.constraint = menuData.GridLayoutConstraint;
        gridLayoutGroup.constraintCount = menuData.GridLayoutConstraintCount;
        
        switch (menuData.LayoutType)
        {
            case GridLayoutType.Horizontal:
                gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                break;
            case GridLayoutType.Vertical:
                gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                break;
            case GridLayoutType.Grid:
                gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal; // Grid usually flows horizontally then wraps vertically
                break;
        }
    }

    /// <summary>
    /// Helper: Creates and configures a single Button item with Text.
    /// </summary>
    private static void PopulateMenuItems(Transform contentParent, int count, Color itemBackgroundColor, Color itemTextColor)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject itemGO = new GameObject("Button_Item_" + (i + 1));
            itemGO.transform.SetParent(contentParent, false);

            itemGO.AddComponent<RectTransform>(); // GridLayoutGroup will manage its size

            Image itemImage = itemGO.AddComponent<Image>();
            itemImage.color = itemBackgroundColor;

            Button buttonComponent = itemGO.AddComponent<Button>();
            int itemIndex = i + 1; // Capture for lambda
            buttonComponent.onClick.AddListener(() => Debug.Log($"Button {itemIndex} Clicked!"));

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(itemGO.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            Text textComponent = textGO.AddComponent<Text>();
            textComponent.text = "Item " + (i + 1);
            textComponent.color = itemTextColor;
            textComponent.fontSize = 20;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Use a default Unity font
        }
    }

    /// <summary>
    /// Helper: Sets the RectTransform properties of a UI element based on alignment and screen percentage.
    /// </summary>
    private static void SetRectTransformProperties(RectTransform rect, MenuAlignment alignment, float percent, RectTransform canvas)
    {
        rect.anchoredPosition = Vector2.zero;

        float canvasWidth = canvas.rect.width;
        float canvasHeight = canvas.rect.height;

        switch (alignment)
        {
            case MenuAlignment.TopLeft:
                rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case MenuAlignment.TopCenter:
                rect.anchorMin = new Vector2(0.5f, 1); rect.anchorMax = new Vector2(0.5f, 1); rect.pivot = new Vector2(0.5f, 1);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case MenuAlignment.TopRight:
                rect.anchorMin = new Vector2(1, 1); rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(1, 1);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case MenuAlignment.BottomLeft:
                rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(0, 0); rect.pivot = new Vector2(0, 0);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case MenuAlignment.BottomCenter:
                rect.anchorMin = new Vector2(0.5f, 0); rect.anchorMax = new Vector2(0.5f, 0); rect.pivot = new Vector2(0.5f, 0);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case MenuAlignment.BottomRight:
                rect.anchorMin = new Vector2(1, 0); rect.anchorMax = new Vector2(1, 0); rect.pivot = new Vector2(1, 0);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
            case MenuAlignment.LeftSide:
                rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 0.5f);
                rect.sizeDelta = new Vector2(canvasWidth * percent, 0);
                break;
            case MenuAlignment.RightSide:
                rect.anchorMin = new Vector2(1, 0); rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(1, 0.5f);
                rect.sizeDelta = new Vector2(canvasWidth * percent, 0);
                break;
            case MenuAlignment.TopPanel:
                rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(0.5f, 1);
                rect.sizeDelta = new Vector2(0, canvasHeight * percent);
                break;
            case MenuAlignment.BottomPanel:
                rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(1, 0); rect.pivot = new Vector2(0.5f, 0);
                rect.sizeDelta = new Vector2(0, canvasHeight * percent);
                break;
            case MenuAlignment.Center:
            default:
                rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f); rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(canvasWidth * percent, canvasHeight * percent);
                break;
        }
    }

    /// <summary>
    /// Helper: Forces an immediate layout rebuild for multiple RectTransforms.
    /// </summary>
    private static void ForceRebuildLayouts(params RectTransform[] rects)
    {
        foreach (RectTransform rect in rects)
        {
            if (rect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }
    }
}

[Serializable]
public class MenuData : IEquatable<MenuData>
{

    //Don't reveal the ID so that users won't get confused.
    [HideInInspector]
    public string ID;
    [HideInInspector]
    public RectTransform ParentCanvas; // Assign in Inspector
    
    public SerializableType ItemType;
    public string MenuName = "New_Dynamic_Menu";
    public Sprite ItemIcon;
    public bool IsAlwaysOn;

    public MenuType MenuType;
    
    public UIMenuGenerator.MenuAlignment Alignment = UIMenuGenerator.MenuAlignment.RightSide;

    [Range(0.01f, 1.0f)] // Provides a slider in the Inspector
    public float ScreenCoveragePercent = 0.25f;

    public UIMenuGenerator.GridLayoutType LayoutType = UIMenuGenerator.GridLayoutType.Vertical;

    public Vector2 ItemCellSize = new Vector2(120, 40);
    public Vector2 ItemSpacing = new Vector2(10, 10);

    // RectOffset can typically be initialized directly as a field in a [Serializable] class.
    // If you encounter the "set_left" error again, you'd need to initialize this in the
    // OnValidate() or Awake() of the MonoBehaviour that holds MenuData.
    public RectOffset GridPadding = new RectOffset(15, 15, 15, 15);

    public TextAnchor GridChildAlignment = TextAnchor.UpperCenter;

    // --- New Fields for GridLayout Constraint ---
    public GridLayoutGroup.Constraint GridLayoutConstraint = GridLayoutGroup.Constraint.Flexible;
    [Range(1, 100)] // Add a range attribute for better Inspector usability
    public int GridLayoutConstraintCount = 1; // Default to 1 (e.g., one column/row)
    // ------------------------------------------
    
    public Color MenuBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public Color ViewportBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    public Color ItemBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
    public Color ItemTextColor = Color.white;

    public bool IsBackgroundPanelVisible;
    // Ensure SerializableType is correctly implemented and [System.Serializable]


    // --- Static Readonly Fields for Default Values ---
    // NO LONGER directly initializing RectOffset here.
    // Instead, we store its individual integer components.
    public static readonly Vector2 DefaultItemCellSize = new Vector2(120, 40);
    public static readonly Vector2 DefaultItemSpacing = new Vector2(10, 10);

    // Changed these to individual int components
    public static readonly int DefaultGridPaddingLeft = 15;
    public static readonly int DefaultGridPaddingRight = 15;
    public static readonly int DefaultGridPaddingTop = 15;
    public static readonly int DefaultGridPaddingBottom = 15;

    public const TextAnchor DefaultGridChildAlignment = TextAnchor.UpperCenter;
    // --- New Static Defaults for GridLayout Constraint ---
    public static readonly GridLayoutGroup.Constraint DefaultGridLayoutConstraintStatic = GridLayoutGroup.Constraint.Flexible;
    public const int DefaultGridLayoutConstraintCountStatic = 1;
    // ---------------------------------------------------
    
    
    public static readonly Color DefaultMenuBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public static readonly Color DefaultViewportBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    public static readonly Color DefaultItemBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
    public static readonly Color DefaultItemTextColor = Color.white;
    public static readonly SerializableType DefaultItemType = new SerializableType(typeof(UnityEngine.UI.Button));
    // Added a constant for ScreenCoveragePercent for default
    public const float DefaultScreenCoveragePercent = 0.25f;

    public MenuType TypeOfMenu; // General menu type (e.g., Regular, ScrollRect)
    public MenuFormType TypeOfFormMenu; // Specific form error handling type

    public MenuData(string menuName, SerializableType itemType, UIMenuGenerator.MenuAlignment alignment,
        float screenCoveragePercent, UIMenuGenerator.GridLayoutType layoutType,
        MenuType typeOfMenu, MenuFormType typeOfFormMenu) // NEW: Added typeOfFormMenu parameter
    {
        MenuName = menuName;
        ItemType = itemType;
        Alignment = alignment;
        ScreenCoveragePercent = screenCoveragePercent;
        LayoutType = layoutType;
        TypeOfMenu = typeOfMenu;
        TypeOfFormMenu = typeOfFormMenu; // Assign the new field
    }
    // =================================================================================================
    // --- CONSTRUCTORS (Overloaded) ---
    // Now using new RectOffset(...) within the constructor, not in static initialization.
    // =================================================================================================

    // --- NEW CONSTRUCTOR FOR WIZARD REGISTRATION ---
    /// <summary>
    /// Constructor for registering a new menu type in MenuCreator, setting key properties and defaulting others.
    /// ParentCanvas is set to null, as it will be assigned by the user in the Inspector later.
    /// </summary>
    public MenuData(
        string menuName, // The "English" name
        SerializableType itemType, // The Type of the new menu MonoBehaviour
        UIMenuGenerator.MenuAlignment alignment = DefaultMenuData.DefaultAlignment, // Provide a default if not given
        float screenCoveragePercent = DefaultMenuData.DefaultScreenCoveragePercent,
        UIMenuGenerator.GridLayoutType layoutType = DefaultMenuData.DefaultLayoutType)
        : this(
            null, itemType, menuName, null,false, alignment, screenCoveragePercent, layoutType,
            DefaultItemCellSize,
            DefaultItemSpacing,
            // Create RectOffset here using the individual int components
            new RectOffset(DefaultGridPaddingLeft, DefaultGridPaddingRight, DefaultGridPaddingTop, DefaultGridPaddingBottom),
            DefaultGridChildAlignment,
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            DefaultMenuBackgroundColor,
            DefaultViewportBackgroundColor,
            DefaultItemBackgroundColor,
            DefaultItemTextColor,
            true,
            MenuType.Regular,
            MenuFormType.PerFieldErrorsType
        )
    {
    }

    // Add a helper class for default values in the new constructor for optional parameters
// This is often needed if you have parameters with no default in C# (like enums)
// or just to organize default values for specific constructors.
    public static class DefaultMenuData
    {
        public const UIMenuGenerator.MenuAlignment DefaultAlignment = UIMenuGenerator.MenuAlignment.Center;
        public const float DefaultScreenCoveragePercent = 0.5f;
        public const UIMenuGenerator.GridLayoutType DefaultLayoutType = UIMenuGenerator.GridLayoutType.Vertical;
        public const int DefaultItemCount = 10;
    }
    
    /// <summary>
    /// Primary (Comprehensive) Constructor: Initializes all properties of MenuData.
    /// All other constructors will chain to this one, providing default values for omitted parameters.
    /// </summary>
    public MenuData(
        RectTransform parentCanvas,
        SerializableType itemType,
        string menuName,
        Sprite itemIcon,
        bool isAlwaysOn,
        UIMenuGenerator.MenuAlignment alignment,
        float screenCoveragePercent,
        UIMenuGenerator.GridLayoutType layoutType,
        Vector2 itemCellSize,
        Vector2 itemSpacing,
        RectOffset gridPadding, // This is now directly taken as a parameter
        TextAnchor gridChildAlignment,
        GridLayoutGroup.Constraint gridLayoutConstraint,
        int gridLayoutConstraintCount,
        Color menuBackgroundColor,
        Color viewportBackgroundColor,
        Color itemBackgroundColor,
        Color itemTextColor, bool isBackgroundPanelVisible,
        MenuType typeOfMenu, MenuFormType typeOfFormMenu) // NEW: Added typeOfFormMenu parameter)
    {
        ParentCanvas = parentCanvas;
        ItemType = itemType; // <--- NEW ASSIGNMENT
        MenuName = menuName;
        Alignment = alignment;
        ScreenCoveragePercent = screenCoveragePercent;
        LayoutType = layoutType;

        ItemCellSize = itemCellSize;
        ItemSpacing = itemSpacing;
        GridPadding = gridPadding; // Assigning the passed-in RectOffset
        GridChildAlignment = gridChildAlignment;
        // --- New Field Initialization in Preset ---
        GridLayoutConstraint = gridLayoutConstraint;
        GridLayoutConstraintCount = gridLayoutConstraintCount;
        // ------------------------------------------
        MenuBackgroundColor = menuBackgroundColor;
        ViewportBackgroundColor = viewportBackgroundColor;
        ItemBackgroundColor = itemBackgroundColor;
        ItemTextColor = itemTextColor;
        IsBackgroundPanelVisible = isBackgroundPanelVisible;
        ID = menuName + "_" + itemType;
        TypeOfMenu = typeOfMenu;
        TypeOfFormMenu = typeOfFormMenu;
    }

    /// <summary>
    /// Constructor: Initializes core layout parameters, with all other properties defaulting.
    /// Example usage: `new MenuData(canvas, "MyMenu", Alignment.RightSide, 0.3f, GridLayoutType.Vertical)`
    /// </summary>
    public MenuData(
        RectTransform parentCanvas,
        SerializableType itemType,
        string menuName,
        Sprite itemIcon,
        bool isAlwaysOn,
        UIMenuGenerator.MenuAlignment alignment,
        float screenCoveragePercent,
        UIMenuGenerator.GridLayoutType layoutType)
        : this(
            parentCanvas, itemType, menuName, itemIcon,isAlwaysOn, alignment, screenCoveragePercent, layoutType,
            DefaultItemCellSize,
            DefaultItemSpacing,
            // Create RectOffset here using the individual int components
            new RectOffset(DefaultGridPaddingLeft, DefaultGridPaddingRight, DefaultGridPaddingTop, DefaultGridPaddingBottom),
            DefaultGridChildAlignment,
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            DefaultMenuBackgroundColor,
            DefaultViewportBackgroundColor,
            DefaultItemBackgroundColor,
            DefaultItemTextColor,
            true,
            MenuType.Regular,
            MenuFormType.PerFieldErrorsType
        )
    {
        ID = menuName + "_" + itemType;
    }

    /// <summary>
    /// Constructor: Initializes core layout, item count, and cell size, with other properties defaulting.
    /// Example usage: `new MenuData(canvas, "MyMenu", Alignment.RightSide, 0.3f, GridLayoutType.Vertical, 20, new Vector2(100, 50))`
    /// </summary>
    public MenuData(
        RectTransform parentCanvas,
        SerializableType itemType,
        string menuName,
        Sprite itemIcon,
        bool isAlwaysOn,
        UIMenuGenerator.MenuAlignment alignment,
        float screenCoveragePercent,
        UIMenuGenerator.GridLayoutType layoutType,
        Vector2 itemCellSize)
        : this(
            parentCanvas, itemType, menuName, itemIcon,isAlwaysOn, alignment, screenCoveragePercent, layoutType,
            itemCellSize, // Use provided itemCellSize
            DefaultItemSpacing,
            new RectOffset(DefaultGridPaddingLeft, DefaultGridPaddingRight, DefaultGridPaddingTop, DefaultGridPaddingBottom),
            DefaultGridChildAlignment,
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            DefaultMenuBackgroundColor,
            DefaultViewportBackgroundColor,
            DefaultItemBackgroundColor,
            DefaultItemTextColor,
            true, MenuType.Form, MenuFormType.PerFieldErrorsType)
    {
        ID = menuName + "_" + itemType;
    }

    /// <summary>
    /// Constructor: Initializes core layout, item count, cell size, and spacing, with other properties defaulting.
    /// </summary>
    public MenuData(
        RectTransform parentCanvas,
        SerializableType itemType,
        string menuName,
        Sprite itemIcon,
        bool isAlwaysOn,
        UIMenuGenerator.MenuAlignment alignment,
        float screenCoveragePercent,
        UIMenuGenerator.GridLayoutType layoutType,
        int itemCount,
        Vector2 itemCellSize,
        Vector2 itemSpacing)
        : this(
            parentCanvas, itemType, menuName,itemIcon, isAlwaysOn,alignment, screenCoveragePercent, layoutType,
            itemCellSize,
            itemSpacing, // Use provided itemSpacing
            new RectOffset(DefaultGridPaddingLeft, DefaultGridPaddingRight, DefaultGridPaddingTop, DefaultGridPaddingBottom),
            DefaultGridChildAlignment,
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            DefaultMenuBackgroundColor,
            DefaultViewportBackgroundColor,
            DefaultItemBackgroundColor,
            DefaultItemTextColor,
            true, MenuType.Form, MenuFormType.PerFieldErrorsType)
    {
        ID = menuName + "_" + itemType;
    }
    

    // --- Static Factory Methods (adjusting RectOffset initialization here too) ---

    /// <summary>
    /// Creates a default "Right Side Vertical" menu configuration.
    /// </summary>
    public static MenuData CreateRightSideVertical(RectTransform parentCanvas, SerializableType serializableType, string menuName = "RightSide_Default")
    {
        return new MenuData(
            parentCanvas: parentCanvas,
            itemType: serializableType,
            menuName: menuName,
            itemIcon: null,
            isAlwaysOn: false,
            alignment: UIMenuGenerator.MenuAlignment.RightSide,
            screenCoveragePercent: DefaultScreenCoveragePercent, // Using DefaultScreenCoveragePercent
            layoutType: UIMenuGenerator.GridLayoutType.Vertical,
            itemCellSize: DefaultItemCellSize,
            itemSpacing: DefaultItemSpacing,
            gridPadding: new RectOffset(DefaultGridPaddingLeft, DefaultGridPaddingRight, DefaultGridPaddingTop,
                DefaultGridPaddingBottom),
            gridChildAlignment: DefaultGridChildAlignment,
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            menuBackgroundColor: new Color(0.1f, 0.1f, 0.3f, 0.9f), // Specific color overrides
            viewportBackgroundColor: new Color(0.2f, 0.2f, 0.4f, 0.7f),
            itemBackgroundColor: new Color(0.4f, 0.4f, 0.6f, 1.0f),
            itemTextColor: Color.white,
            true, MenuType.Form, MenuFormType.PerFieldErrorsType);
    }

    /// <summary>
    /// Creates a default "Bottom Panel Horizontal" menu configuration.
    /// </summary>
    public static MenuData CreateBottomPanelHorizontal(RectTransform parentCanvas, SerializableType serializableType, string menuName = "BottomPanel_Default")
    {
        return new MenuData(
            parentCanvas: parentCanvas,
            itemType: serializableType,
            menuName: menuName,
            itemIcon: null,
            isAlwaysOn: false,
            alignment: UIMenuGenerator.MenuAlignment.BottomPanel,
            screenCoveragePercent: 0.15f,
            layoutType: UIMenuGenerator.GridLayoutType.Horizontal,
            itemCellSize: new Vector2(100, 70), // Specific cell size
            itemSpacing: DefaultItemSpacing,
            gridPadding: new RectOffset(DefaultGridPaddingLeft, DefaultGridPaddingRight, DefaultGridPaddingTop, DefaultGridPaddingBottom),
            gridChildAlignment: TextAnchor.MiddleLeft, // Specific alignment
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            menuBackgroundColor: new Color(0.3f, 0.1f, 0.1f, 0.9f),
            viewportBackgroundColor: new Color(0.4f, 0.2f, 0.2f, 0.7f),
            itemBackgroundColor: new Color(0.6f, 0.4f, 0.4f, 1.0f),
            itemTextColor: Color.yellow,
            true, MenuType.Form, MenuFormType.PerFieldErrorsType
        );
    }

    /// <summary>
    /// Creates a "Center Grid" menu configuration with configurable item count.
    /// </summary>
    public static MenuData CreateCenterGrid(RectTransform parentCanvas, SerializableType serializableType, string menuName = "Center_Grid_Default")
    {
        return new MenuData(
            parentCanvas: parentCanvas,
            itemType: serializableType,
            menuName: menuName,
            itemIcon: null,
            isAlwaysOn: false,
            alignment: UIMenuGenerator.MenuAlignment.Center,
            screenCoveragePercent: 0.6f, // Larger central menu
            layoutType: UIMenuGenerator.GridLayoutType.Grid,
            itemCellSize: new Vector2(80, 80), // Specific cell size
            itemSpacing: new Vector2(15, 15), // Specific spacing
            gridPadding: new RectOffset(25, 25, 25, 25), // Specific padding
            gridChildAlignment: TextAnchor.MiddleCenter, // Specific alignment
            gridLayoutConstraint: DefaultGridLayoutConstraintStatic,
            gridLayoutConstraintCount: 1,
            menuBackgroundColor: new Color(0.1f, 0.3f, 0.1f, 0.9f),
            viewportBackgroundColor: new Color(0.2f, 0.4f, 0.2f, 0.7f),
            itemBackgroundColor: new Color(0.4f, 0.6f, 0.4f, 1.0f),
            itemTextColor: Color.white,
            true, MenuType.Form, MenuFormType.PerFieldErrorsType
        );
    }

    public bool Equals(MenuData other)
    {
        if (other is null)
        {
            return false;
        }
        bool equalName = other.MenuName == MenuName;
        bool equalType = other.ItemType == ItemType.Type;
        bool equalID = other.ID == MenuName + "_" + ItemType;
        
        return equalName && equalType && equalID;
    }

    public override bool Equals(object other)
    {
        if (other is null)
        {
            return false;
        }

        bool equalType = other.GetType() == typeof(MenuData);
        bool equalID = Equals((MenuData)other);

        return equalType && equalID;
    }



    // Overloading of Binary "+" operator
    public static bool operator == (MenuData other1,  MenuData other2)
    {
        if (other1 is null && other2 is null)
        {
            return true;
        }
        if (other1 is null)
        {
            return false;
        }
        return other1.Equals(other2);
    }
    
    public static bool operator != (MenuData other1,  MenuData other2)
    {
        if (other1 is null && other2 is null)
        {
            return false;
        }
        if (other1 is null)
        {
            return true;
        }
        return !other1.Equals(other2);
    }
}