using System;
using UnityEngine;
using UnityEngine.Events; // Needed for UnityAction if used for validation callbacks

namespace NP_UI
{
    /// <summary>
    /// Base class for UI menus that manage forms and require validation.
    /// Inherits from NpGenericMenu to leverage its core UI management functionalities.
    /// </summary>
    public class FormMenu : NpGenericMenu
    {
        private NP_Label errorLogLabel;
        private bool isBottomErrorsDirected = false;
        private bool isPerFieldErrorsDirected = false;
        public override void OpenMenu()
        {

            FillFields();
            if (isBottomErrorsDirected)
            {
                errorLogLabel = (npMenu as NP_MenuForm).errorLogLabel;
            }
            
            bool isContainNonValidableElements;
            if (genericUIDatas != null && genericUIDatas.Count > 0)
            {
                int numberOfValidableElements = genericUIDatas.FindAll(x => x is FormGenericUIData).Count;
                int allElementsNumber = genericUIDatas.Count;
                isContainNonValidableElements = numberOfValidableElements != allElementsNumber;
                if (isContainNonValidableElements)
                {
                    Debug.LogError("Error in Menu "+npMenu.menuData.MenuName+".\nUsing Non Validable Elements in FormMenu is not possible.\n"
                                   + "Look for genericUIDatas list and make sure all elements are inherited from FormGenericUIData.");
                    return;
                }
            }
            base.OpenMenu();
        }

        private void FillFields()
        {
            MenuFormType menuFormType = npMenu.menuData.TypeOfFormMenu;
            isBottomErrorsDirected = menuFormType == MenuFormType.BottomErrorsForm || menuFormType == MenuFormType.Both;
            isPerFieldErrorsDirected = menuFormType == MenuFormType.PerFieldErrorsType || menuFormType == MenuFormType.Both;
        }

        private void CreateSubmitButton()
        {
            Func<object,bool> func = (submitButtonData) => { return true; };
            ValidatableButtonData submitButtonData = new ValidatableButtonData(OnSubmitForm, "Submit",func, "");
                
            genericUIDatas += submitButtonData;
        }


        private void ClearErrorLog()
        {
            if (isBottomErrorsDirected)
            {
                errorLogLabel.SetText("");
            }
        }
        
        /// <summary>
        /// Virtual method to implement form validation logic.
        /// Derived form classes should override this to perform specific validation checks.
        /// </summary>
        /// <returns>True if the form is valid, false otherwise.</returns>
        public virtual bool ValidateForm()
        {
            if (isBottomErrorsDirected)
            {
                errorLogLabel.SetText("");
            }
            // Debug.Log("FormMenu: Default ValidateForm called. Override this method for specific validation.");
            // Default implementation returns true, assuming no validation is needed unless overridden.
            bool valid = true;
            foreach (FormGenericUIData inputFieldData in genericUIDatas)
            {
                if (inputFieldData is ValidatableInputFieldData)
                {
                    NP_InputField inputField = inputFieldData.GetUIElement() as NP_InputField;
                    if (inputField != null)
                    {
                        if (isPerFieldErrorsDirected)
                        {
                            inputField.SetErrorMessage("");
                        }

                        if (!inputFieldData.IsValid.Invoke(inputField.GetValueText()))
                        {
                            valid = false;
                            if (isBottomErrorsDirected)
                            {
                                AddErrorLog(inputField.GetValueText() + ": " + inputFieldData.ErrorMessage);
                            }
                            
                            if(isPerFieldErrorsDirected)
                            {
                                inputField.SetErrorMessage(inputFieldData.ErrorMessage);
                            }
                        }
                    }
                }
            }

            return valid;
        }

        protected void AddErrorLog(string error)
        {
            string errorLog = errorLogLabel.GetText();
            errorLogLabel.SetText(errorLog += error + "\n");
        }
        
        /// <summary>
        /// Example method for handling form submission.
        /// </summary>
        protected virtual void OnSubmitForm()
        {
            if (ValidateForm())
            {
                Debug.Log("Form submitted successfully!");
                CloseMenu();
                // Add logic for what happens after a successful form submission
            }
            else
            {
                Debug.LogWarning("Form validation failed. Please correct errors.");
                // Add logic for handling validation failures (e.g., showing error messages)
            }
        }

        /// <summary>
        /// Clears all input fields and resets the form to an empty state.
        /// Derived classes should override this to clear specific form elements.
        /// </summary>
        protected virtual void ClearForm()
        {
            //Run for all the data elements in the list
            foreach (GenericUIData uiData in genericUIDatas)
            {
                if (uiData != null)
                {
                    //Find all inputFields 
                    if (uiData is InputFieldData)
                    {
                        //Clear the element
                        ClearElement(uiData.GetUIElement());
                    }
                }
            }
        }

        private void ClearElement(NP_UIElements element)
        {
            if (element == null)
            {
                Debug.Log("No element selected.");
                return;
            }

            if (element is NP_InputField)
            {
                NP_InputField inputField = element as NP_InputField;
                inputField.SetText("");
            }
            
        }

        /// <summary>
        /// Resets the form to its initial state, which might include default values.
        /// Derived classes should override this to reset specific form elements.
        /// </summary>
        protected virtual void ResetForm()
        {
            Debug.Log("FormMenu: Default ResetForm called. Override to reset specific form elements to defaults.");
            // Example:
            // NP_InputField usernameInputField = GetElementByID<NP_InputField>(usernameInput.ID);
            // if (usernameInputField != null)
            // {
            //     usernameInputField.SetText("Default Username"); // Or from initial data
            // }
        }

        public override void CreateMenuItems()
        {
            CreateSubmitButton();
        }

        public override void CloseMenu()
        {
            ClearErrorLog();
            base.CloseMenu();
        }

        /// <summary>
        /// Abstract base class for all UI data elements specifically designed for forms.
        /// It inherits from GenericUIData and enforces validation capabilities.
        /// </summary>
        public abstract class FormGenericUIData : GenericUIData, IValidatable
        {
            /// <summary>
            /// Gets or sets the validation function for this form element.
            /// Defaults to a function that always returns true if not explicitly set.
            /// </summary>
            public Func<object ,bool> IsValid { get; set; }
            public string ErrorMessage;

            protected FormGenericUIData()
            {
                // Initialize IsValidCheck to a default function that always returns true.
                // Developers can override this when creating specific form data instances.
                IsValid = (obj) => true; 
            }

            // The abstract GetUIElement() method is inherited from GenericUIData and must be
            // implemented by concrete derived classes (e.g., FormLabelData).
            public abstract override NP_UIElements GetUIElement();
        }
        
        protected interface IValidatable
        {
            /// <summary>
            /// A function that returns true if the element's current state is valid, false otherwise.
            /// This function will be set by the developer when creating the data element.
            /// </summary>
            Func<object, bool> IsValid { get; set; }
        }
        
    /// <summary>
    /// Data for a Label element within a Form, including validation capability.
    /// </summary>
    [Serializable]
    public class ValidatableLabelData : FormGenericUIData
    {
        public string Text;
        public int FontSize;

        public ValidatableLabelData(string text, Func<object, bool> isValidCheck, int fontSize = -1)
        {
            Text = text;
            FontSize = fontSize;
            IsValid = isValidCheck; // Assign the mandatory validation function
            // ID is handled by base constructor
        }

        public override NP_UIElements GetUIElement()
        {
            // Assuming NP_Lable is the correct UI element type for labels.
            return UIElement as NP_Label;
        }
    }

    /// <summary>
    /// Data for a Button element within a Form, including validation capability.
    /// </summary>
    [Serializable]
    public class ValidatableButtonData : FormGenericUIData
    {
        public UnityAction ClickAction;
        public string Text;
        public Color BackgroundColor;
        public Sprite MenuIcon;

        public ValidatableButtonData(UnityAction onClick, string textButton, Func<object, bool> isValidCheck, string errorMessage)
        {
            ClickAction = onClick;
            Text = textButton;
            MenuIcon = null;
            IsValid = isValidCheck; // Assign the mandatory validation function
            ErrorMessage = errorMessage;
        }
        public ValidatableButtonData(UnityAction onClick, Sprite menuIcon, Func<object, bool> isValidCheck, string errorMessage)
        {
            ClickAction = onClick;
            MenuIcon = menuIcon;
            Text = String.Empty;
            IsValid = isValidCheck; // Assign the mandatory validation function
            ErrorMessage = errorMessage;
        }
        public ValidatableButtonData(UnityAction onClick, Sprite menuIcon, string text, Func<object, bool> isValidCheck, string errorMessage)
        {
            ClickAction = onClick;
            MenuIcon = menuIcon;
            Text = text;
            IsValid = isValidCheck; // Assign the mandatory validation function
            ErrorMessage = errorMessage;
        }
        
        public ValidatableButtonData(UnityAction onClick, string text, Color backgroundColor, Func<object, bool> isValidCheck, string errorMessage)
        {
            ClickAction = onClick;
            MenuIcon = null;
            Text = text;
            BackgroundColor = backgroundColor;
            IsValid = isValidCheck; // Assign the mandatory validation function
            ErrorMessage = errorMessage;
        }

        public override NP_UIElements GetUIElement()
        {
            return UIElement as NP_Button;
        }
    }

    /// <summary>
    /// Data for an InputField element within a Form, including validation capability.
    /// </summary>
    [Serializable]
    public class ValidatableInputFieldData : FormGenericUIData
    {

        public UnityAction<string> OnValueChanged;
        public string Text; // Initial text or current text
        public string Description;
        

        public ValidatableInputFieldData(Func<object, bool> isValidCheck, string errorMessage, string description)
        {
            OnValueChanged = null;
            Text = "";
            IsValid = isValidCheck; // Assign the mandatory validation function
            ErrorMessage = errorMessage;
            Description = description;
        }
        public ValidatableInputFieldData(UnityAction<string> onValueChanged, Func<object, bool> isValidCheck, string errorMessage, string description)
        {
            OnValueChanged = onValueChanged;
            Text = "";
            IsValid = isValidCheck; // Assign the mandatory validation function
            ErrorMessage = errorMessage;
            Description = description;
        }
        public ValidatableInputFieldData(string text, Func<object, bool> isValidCheck, string errorMessage, string description)
        {
            OnValueChanged = null;
            Text = text;
            IsValid = isValidCheck; // Assign the mandatory validation function
            ErrorMessage = errorMessage;
            Description = description;
        }
        public ValidatableInputFieldData(UnityAction<string> onValueChanged, string text, Func<object, bool> isValidCheck, string errorMessage, string description)
        {
            OnValueChanged = onValueChanged;
            Text = text;
            IsValid = isValidCheck; // Assign the mandatory validation function
            ErrorMessage = errorMessage;
            Description = description;
        }
        
        

        public override NP_UIElements GetUIElement()
        {
            return UIElement as NP_InputField;
        }
    }

    /// <summary>
    /// Data for a Slider element within a Form, including validation capability.
    /// </summary>
    [Serializable]
    public class ValidatableSliderData : FormGenericUIData
    {
        public UnityAction<float> OnValueChanged;
        public string Text; // Label for the slider
        public float Value;
        public float MinValue;
        public float MaxValue;
        public bool WholeNumber;

        public ValidatableSliderData(float value, float minValue, float maxValue, UnityAction<float> onValueChanged, Func<object, bool> isValidCheck, bool wholeNumber = true)
        {
            Value = value;
            MinValue = minValue;
            MaxValue = maxValue;
            WholeNumber = wholeNumber;
            OnValueChanged = onValueChanged;
            Text = "";
            IsValid = isValidCheck; // Assign the mandatory validation function
        }
        public ValidatableSliderData(float value, float minValue, float maxValue, Func<object, bool> isValidCheck, bool wholeNumber = true)
        {
            Value = value;
            MinValue = minValue;
            MaxValue = maxValue;
            WholeNumber = wholeNumber;
            OnValueChanged = (x) => { }; // Default empty action
            Text = "";
            IsValid = isValidCheck; // Assign the mandatory validation function
        }
        public ValidatableSliderData(UnityAction<float> onValueChanged, Func<object, bool> isValidCheck)
        {
            OnValueChanged = onValueChanged;
            Text = "";
            IsValid = isValidCheck; // Assign the mandatory validation function
        }
        public ValidatableSliderData(string text, Func<object, bool> isValidCheck)
        {
            OnValueChanged = null;
            Text = text;
            IsValid = isValidCheck; // Assign the mandatory validation function
        }
        public ValidatableSliderData(UnityAction<float> onValueChanged, string text, Func<object, bool> isValidCheck)
        {
            OnValueChanged = onValueChanged;
            Text = text;
            IsValid = isValidCheck; // Assign the mandatory validation function
        }

        // Note: The SetValue(float initialValue) method from your previous SliderData
        // is not directly part of the data class's core responsibility but rather
        // an operation on the UI element itself. It's usually handled by the NP_Slider component.
        // If it's crucial for the data class, you might reconsider its placement.

        public override NP_UIElements GetUIElement()
        {
            return UIElement as NP_Slider;
        }
    }
    }
}
