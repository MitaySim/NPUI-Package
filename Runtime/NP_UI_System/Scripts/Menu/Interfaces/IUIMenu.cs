using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NP_UI
{
    public class NpGenericMenu : MonoBehaviour
    {
        protected Dictionary<string, NP_UIElements> IElementsIDDictionary = new Dictionary<string, NP_UIElements>();
        protected GameObject MenuGameObject { get; set; }

        [SerializeField] private NP_Label headLineText;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        public NP_Menu npMenu;
        protected ElemetsIDSystem _elementsSystem;
        protected List<GenericUIData> genericUIDatas = new List<GenericUIData>();


        public virtual void CloseMenu()
        {
            ClearUI();
            MenuGameObject.SetActive(false);
        }

        public virtual void OpenMenu()
        {
            CreateUI();
            MenuGameObject.SetActive(true);
        }


        public void Awake()
        {
            AlwaysOnHandler();
        }

        private void AlwaysOnHandler()
        {
            npMenu = GetComponent<NP_Menu>();
            SetID();
            if (npMenu != null)
            {
                MenuData menuData = npMenu.menuData;
                if (menuData != null)
                {
                    bool isAlwaysOn = menuData.IsAlwaysOn;
                    if (isAlwaysOn)
                    {
                        OpenMenu();
                    }
                }
                else
                {
                    Debug.Log("MenuData not found for AlwaysOnHandler for unknown menu");
                }
            }
            else
            {
                Debug.Log("npMenu not found for AlwaysOnHandler for unknown menu");
            }
        }

        protected void ClearUI(bool clearData = true)
        {
            if (IElementsIDDictionary != null && IElementsIDDictionary.Count > 0)
            {
                foreach (NP_UIElements element in IElementsIDDictionary.Values)
                {
                    if (element != null)
                    {
                        Destroy(element.gameObject);
                    }
                }

                if (clearData)
                {
                    IElementsIDDictionary.Clear();
                    genericUIDatas.Clear();
                }
            }
        }

        protected void CreateUI()
        {
            SetFields();
            CreateMenuItems();
            AddElementsByDataToMenu(genericUIDatas, _elementsSystem);
            AddListeners();
            StartAfterCreation();
        }

        protected virtual void AddListeners()
        {
            if (!npMenu.menuData.IsAlwaysOn)
            {
                NP_EventsManager.CloseAllMenus.AddListener(CloseMenu);
            }
        }

        public virtual void StartAfterCreation()
        {

        }

        public void InitializeMenu(NP_UIMenuData menuData)
        {
            MenuGameObject = gameObject;
            headLineText.SetText(menuData.headLine);
        }

        public void SetFields(NP_Menu npMenu)
        {
            headLineText = npMenu.headLineText;
            gridLayoutGroup = npMenu.gridLayoutGroup;
        }

        private void SetFields()
        {
            _elementsSystem = new ElemetsIDSystem(npMenu.menuData.MenuName);
            gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>(true);
            if (MenuGameObject == null)
            {
                MenuGameObject = gameObject;
            }
        }

        private void SetID()
        {
            npMenu.menuData.ID = npMenu.menuData.MenuName + "_" + npMenu.menuData.ItemType;
        }

        public void AddElementToMenu(NP_UIElements element)
        {
            element.transform.SetParent(gridLayoutGroup.transform, false);
        }

        public void AddListOfElementsToMenu(List<NP_UIElements> elements)
        {
            if (elements == null || elements.Count == 0)
            {
                Debug.LogError("Elements cannot be null or empty");
                return;
            }

            foreach (var element in elements)
            {
                element.transform.SetParent(gridLayoutGroup.transform, false);
            }
        }

        protected void AddElementsByDataToMenu(List<GenericUIData> elements, ElemetsIDSystem elementsSystem)
        {
            if (elements == null || elements.Count == 0)
            {
                Debug.Log("Menu has no elements");
                return;
            }
            
            foreach (var element in elements)
            {

                elementsSystem.SetElementID(element);
                NP_UIElements uiElement = NP_MenuDesignData.Instance.CreateUIElementByData(element);
                
                element.SetValue(uiElement);

                if (!IElementsIDDictionary.ContainsKey(element.ID))
                {
                    IElementsIDDictionary.Add(element.ID, uiElement);
                }
                
                if (uiElement != null)
                {
                    uiElement.transform.SetParent(gridLayoutGroup.transform, false);
                }
                else
                {
                    Debug.LogError("Element can't be null or empty");
                }
            }
        }
        
        protected NP_UIElements GetElementByID(string elementID)
        {
            NP_UIElements[] elements = GetComponentsInChildren<NP_UIElements>(true);
            NP_UIElements element = null;
            if (elements != null && elements.Length > 0)
            {
                element = elements.FirstOrDefault(element => element.ID == elementID);
            }

            Debug.LogWarning($"GetElementByID: Element {elementID} not found!");
            return element;
        }

        public GridLayoutGroup GetGridLayoutGroup()
        {
            return gridLayoutGroup;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!npMenu.menuData.IsAlwaysOn)
                {
                    CloseMenu();
                }
            }
        }

        public virtual void CreateMenuItems()
        {

        }

        /// <summary>
        /// Generic function to retrieve a specific UI element by its data ID.
        /// </summary>
        /// <typeparam name="TElementData">The type of the element data (e.g., LabelData, ButtonData).</typeparam>
        /// <typeparam name="TResult">The expected type of the UI element to return (e.g., NP_Lable, NP_Button).</typeparam>
        /// <param name="elementData">The data object containing the ID.</param>
        /// <returns>The found UI element cast to TResult, or null if not found or type mismatch.</returns>
        protected TResult GetElementByID<TResult>(GenericUIData elementData)
            //where TElementData : GenericUIData // Constraint: TElementData must implement IBaseElementData
            where TResult : NP_UIElements // Constraint: TResult must be NP_UIElement or derive from it
        // (Adjust 'NP_UIElement' if your base class is different, e.g., MonoBehaviour)
        {
            if (elementData == null || elementData.ID == null)
            {
                Debug.LogError("Element can't be null or empty");
                return null;
            }
            if (IElementsIDDictionary.ContainsKey(elementData.ID))
            {
                NP_UIElements foundElement = IElementsIDDictionary[elementData.ID];

                // Attempt to cast the found element to the requested TResult type
                if (foundElement is TResult result)
                {
                    return result;
                }
                else
                {
                    // This means an element was found for the ID, but it's not the requested type
                    Debug.LogWarning($"GetElementByID: Element '{elementData.ID}' found, but its type " +
                                     $"('{foundElement.GetType().Name}') does not match the requested type " +
                                     $"('{typeof(TResult).Name}'). Returning null.");
                    return null;
                }
            }
            else
            {
                Debug.LogWarning($"GetElementByID: Element '{elementData.ID}' not found in dictionary!");
                return null;
            }
        }
    }

    //public interface ICreatableMenu
    //{
    //    void CreateMenuItems();
//
    //    void StartAfterCreation();
    //}
}