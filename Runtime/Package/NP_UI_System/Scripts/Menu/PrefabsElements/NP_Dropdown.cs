using UnityEngine;
using UnityEngine.UI;

// --- NP_Dropdown ---
/// <summary>
/// Represents a UI Dropdown element, capable of displaying and selecting text.
/// Implements ITextableElement interface (for selected text display).
/// </summary>
public class NP_Dropdown : NP_UIElements, ITextableElement
{
    [SerializeField] protected Dropdown dropdownComponent; // Reference to Unity's Dropdown component
    [SerializeField] protected Text selectedTextComponent; // Reference to the Text component showing the selected option

    protected override void Awake()
    {
        base.Awake();
        dropdownComponent = GetComponent<Dropdown>();
        if (dropdownComponent == null)
        {
            Debug.LogError("NP_Dropdown requires a Dropdown component on its GameObject.", this);
        }
        // Assuming the dropdown has a child Text component for the selected value
        selectedTextComponent = dropdownComponent.captionText; // Common way to get the text of the selected option
        if (selectedTextComponent == null)
        {
             Debug.LogWarning("NP_Dropdown: Could not find caption Text component.", this);
        }
    }

    public void SetText(string text)
    {
        // For a dropdown, setting text typically means setting the initial selected value
        // or adding options. The diagram implies setting the *display* text of the selected item.
        if (selectedTextComponent != null)
        {
            selectedTextComponent.text = text;
        }
        else
        {
            Debug.LogWarning("NP_Dropdown: Cannot set text because caption text component is missing.", this);
        }
    }

    // Implementing ITextableElement methods for NP_Dropdown (mostly pass-through or adapting behavior)
    public void SetBackgroundColor(Color color)
    {
        Graphic graphic = GetComponent<Graphic>(); // Dropdown background
        if (graphic != null)
        {
            graphic.color = color;
        }
    }

    public void SetTextColor(Color color)
    {
        if (selectedTextComponent != null)
        {
            selectedTextComponent.color = color;
        }
    }

    public void SetBold(bool isBold)
    {
        if (selectedTextComponent != null)
        {
            selectedTextComponent.fontStyle = isBold ? FontStyle.Bold : FontStyle.Normal;
        }
    }

    // Example of adding options to the dropdown (not explicitly in diagram but common)
    public void AddOptions(System.Collections.Generic.List<string> options)
    {
        if (dropdownComponent != null)
        {
            dropdownComponent.AddOptions(options);
        }
    }
}
