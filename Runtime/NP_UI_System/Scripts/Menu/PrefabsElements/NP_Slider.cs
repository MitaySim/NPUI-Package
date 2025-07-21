using System;
using DA_Assets.DAG;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NP_UI
{
// --- NP_Button ---
    /// <summary>
    /// Represents a UI Button element, capable of being clicked and having an image.
    /// Implements IClickableElement and IImageableElement interfaces.
    /// </summary>
    public class NP_Slider : NP_UIElements, IImageableElement, ITextableElement
    {
        [SerializeField] protected Slider sliderComponent; // Reference to Unity's Button component
        [SerializeField] protected Image backgroundImageComponent; // Reference to Unity's RawImage component
        [SerializeField] protected TextMeshProUGUI textHeadLine; // Reference to Unity's RawImage component

        protected override void Awake()
        {
            base.Awake();
            sliderComponent = GetComponent<Slider>();
            if (sliderComponent == null)
            {
                Debug.LogError("NP_Slider requires a Slider component on its GameObject.", this);
            }

            if (backgroundImageComponent == null)
            {
                Debug.LogWarning("NP_Slider: No RawImage component found to set background image.", this);
            }
        }

        public void SetOnClick(UnityAction<float> onClickAction)
        {
            if (sliderComponent != null)
            {
                sliderComponent.onValueChanged.AddListener(onClickAction);
            }
        }

        public void SetMaxVAlue(float maxValue)
        {
            sliderComponent.maxValue = maxValue;
        }

        public void SetMinVAlue(float minValue)
        {
            sliderComponent.minValue = minValue;
        }

        public float GetValue()
        {
            return sliderComponent.value;
        }
        
        public void SetValue(float value)
        {
            sliderComponent.value = value;
        }
        
        public void SetBackgroundImage(Sprite texture)
        {
            if (backgroundImageComponent != null && texture != null)
            {
                sliderComponent.image.sprite = texture;
            }
            else if (texture == null)
            {
                Debug.LogWarning("NP_Slider: Provided texture is null for SetBackgroundImage.", this);
            }
        }


        public void SetText(string text)
        {
            if (textHeadLine != null)
            {
                textHeadLine.text = text;
            }
        }

        public void SetBackgroundColor(Color color)
        {
            //ColorChanger.ShiftGradientHueDAG(dagBackgroundImageComponent, color);
            //ColorChanger.DarkenExistingGradientDAG(dagBackgroundImageComponent, 1f);
        }

        public void SetTextColor(Color color)
        {
            throw new NotImplementedException();
        }

        public void SetBold(bool isBold)
        {
            throw new NotImplementedException();
        }

        public void SetWholeNumbers(bool sliderDataWholeNumber)
        {
            sliderComponent.wholeNumbers = sliderDataWholeNumber;
        }
    }
}