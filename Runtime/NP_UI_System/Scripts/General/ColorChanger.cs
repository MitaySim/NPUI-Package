using DA_Assets.DAG;
using UnityEngine;

public class ColorChanger
{
    private static Color newBaseColor = Color.white; // For setting a single color gradient
    //[Range(0.01f, 1f)] // Factor to multiply the color by for darkening
    //private static float darkenFactor = 0.8f; 



    /// <summary>
    /// Changes the DAGradient to be a solid color while preserving existing alpha.
    /// </summary>
    /// <param name="color">The new solid color for the gradient.</param>
    //public static void ApplyColorToDAGradient(DAGradient targetDAGradient, Color color)
    //{
    //    if (targetDAGradient == null)
    //    {
    //        Debug.LogError("No DAGradient component assigned or found!");
    //        return;
    //    }
//
    //    // Get the current gradient. Adapt this line to your DAGradient implementation.
    //    Gradient currentGradient = targetDAGradient.Gradient; // Example: targetDAGradient.myGradientField;
//
    //    // Create new color keys for a solid color
    //    GradientColorKey[] colorKeys = new GradientColorKey[2];
    //    colorKeys[0] = new GradientColorKey(color, 0f);
    //    colorKeys[1] = new GradientColorKey(color, 1f);
//
    //    // Preserve existing alpha keys
    //    GradientAlphaKey[] alphaKeys = currentGradient.alphaKeys;
//
    //    currentGradient.SetKeys(colorKeys, alphaKeys);
//
    //    // Update the DAGradient component. Adapt this line to your DAGradient implementation.
    //    targetDAGradient.Gradient = currentGradient; 
//
    //}

    /// <summary>
    /// Darkens each color in the existing gradient by a specified factor.
    /// Preserves the gradient's transitions and alpha keys.
    /// </summary>
    /// <param name="factor">The multiplication factor for darkening (e.g., 0.8 for 20% darker). 
    /// Should be between 0 and 1.</param>
    public static void DarkenExistingGradientDAG(DAGradient targetDAGradient, float factor)
    {
        if (targetDAGradient == null)
        {
            Debug.LogError("No DAGradient component assigned or found!");
            return;
        }

        if (factor < 0f || factor > 1f)
        {
            Debug.LogWarning("Darken factor should be between 0 and 1. Clamping.");
            factor = Mathf.Clamp01(factor);
        }

        // Get the current gradient. Adapt this line to your DAGradient implementation.
        Gradient currentGradient = targetDAGradient.Gradient; // Example: targetDAGradient.myGradientField;

        // Get the color keys
        GradientColorKey[] colorKeys = currentGradient.colorKeys;

        // Iterate through each color key and darken its color
        for (int i = 0; i < colorKeys.Length; i++)
        {
            Color originalColor = colorKeys[i].color;
            // Multiply each color component (R, G, B) by the factor
            // This darkens the color. Alpha (A) is typically left as is unless you want to affect transparency.
            Color darkerColor = new Color(
                originalColor.r * factor,
                originalColor.g * factor,
                originalColor.b * factor,
                originalColor.a // Keep alpha the same
            );
            colorKeys[i].color = darkerColor;
        }

        // Get the alpha keys (we're keeping them as they are)
        GradientAlphaKey[] alphaKeys = currentGradient.alphaKeys;

        // Apply the modified color keys and original alpha keys back to the gradient
        currentGradient.SetKeys(colorKeys, alphaKeys);

        // Update the DAGradient component. Adapt this line to your DAGradient implementation.
        targetDAGradient.Gradient = currentGradient; 
    }
    
    
    public static void ShiftGradientHueDAG(DAGradient targetDAGradient, Color targetColor)
    {
        if (targetDAGradient == null) return;

        // Extract HSV
        Color.RGBToHSV(targetColor, out float targetH, out float targetS, out float targetV);
        bool targetIsGray = targetS < 0.01f;

        Gradient currentGradient = targetDAGradient.Gradient;
        GradientColorKey[] colorKeys = currentGradient.colorKeys;
        GradientAlphaKey[] alphaKeys = currentGradient.alphaKeys;

        if (colorKeys.Length == 0) return;

        // Find base hue (first colored key)
        float baseHue = targetH;
        foreach (var key in colorKeys)
        {
            Color.RGBToHSV(key.color, out float h, out float s, out _);
            if (s > 0.01f) { baseHue = h; break; }
        }
        float hueOffset = targetH - baseHue;

        Gradient newGradient = new Gradient();

        for (int i = 0; i < colorKeys.Length; i++)
        {
            Color original = colorKeys[i].color;
            Color.RGBToHSV(original, out float h, out float s, out float v);

            bool wasGray = s < 0.01f;
            Color result;

            if (targetIsGray)
            {
                // Target grayscale → force output grayscale, match targetV exactly
                result = Color.HSVToRGB(0f, 0f, targetV, true);
            }
            else if (wasGray)
            {
                // Original grayscale → colorize with target hue and saturation
                result = Color.HSVToRGB(targetH, targetS, Mathf.Max(v, targetV), true);
            }
            else
            {
                // Colored → colored → apply hue offset
                float newH = Mathf.Repeat(h + hueOffset, 1f);
                result = Color.HSVToRGB(newH, s, v, true);
            }

            result.a = original.a;
            colorKeys[i].color = result;
        }

        newGradient.SetKeys(colorKeys, alphaKeys);
        targetDAGradient.Gradient = newGradient;
    }










    // Editor context menus for easy testing
    //[ContextMenu("Apply New Base Color (Solid)")]
    //private void EditorApplySolidColor()
    //{
    //    ApplyColorToDAGradient(newBaseColor);
    //}
//
    //[ContextMenu("Darken Current Gradient")]
    //private void EditorDarkenGradient()
    //{
    //    DarkenExistingGradient(darkenFactor);
    //}
}