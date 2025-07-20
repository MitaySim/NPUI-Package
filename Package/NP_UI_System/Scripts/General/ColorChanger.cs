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
    
    
    /// <summary>
    /// Shifts the hue of the entire gradient to a new target hue, preserving 
    /// relative saturation, value (brightness), and the gradient's original transitions.
    /// </summary>
    /// <param name="targetColor">A color whose hue will be used as the target hue for the gradient.</param>
    public static void ShiftGradientHueDAG(DAGradient targetDAGradient,  Color targetColor) // Changed input to Color
    {
        if (targetDAGradient == null)
        {
            Debug.LogError("No DAGradient component assigned or found!");
            return;
        }

        // Extract the hue from the targetColor
        float newHue;
        float tempS, tempV; // We only care about the hue here
        Color.RGBToHSV(targetColor, out newHue, out tempS, out tempV);

        // Get the current gradient. Adapt this line to your DAGradient implementation.
        Gradient currentGradient = targetDAGradient.Gradient; 

        // Get the color keys
        GradientColorKey[] colorKeys = currentGradient.colorKeys;

        // Handle case where gradient might be empty or invalid initially
        if (colorKeys.Length == 0)
        {
            Debug.LogWarning("Gradient has no color keys. Cannot perform hue shift.");
            return;
        }

        // Calculate the hue shift amount based on the first color key's original hue
        float firstColorHue;
        float firstColorSaturation;
        float firstColorValue; 
        Color.RGBToHSV(colorKeys[0].color, out firstColorHue, out firstColorSaturation, out firstColorValue);

        // The difference between the new target hue and the original first hue
        float hueOffset = newHue - firstColorHue;

        // Iterate through each color key and apply the hue shift
        for (int i = 0; i < colorKeys.Length; i++)
        {
            Color originalColor = colorKeys[i].color;

            float h, s, v;
            Color.RGBToHSV(originalColor, out h, out s, out v);

            // Apply the hue shift. Use Mathf.Repeat to wrap around the 0-1 range.
            h = Mathf.Repeat(h + hueOffset, 1f); 

            // Convert back to RGB
            Color shiftedColor = Color.HSVToRGB(h, s, v);

            // Preserve original alpha
            shiftedColor.a = originalColor.a;

            colorKeys[i].color = shiftedColor;
        }

        // Get the alpha keys (we're keeping them as they are)
        GradientAlphaKey[] alphaKeys = currentGradient.alphaKeys;

        // Apply the modified color keys and original alpha keys back to the gradient
        currentGradient.SetKeys(colorKeys, alphaKeys);

        // Update the DAGradient component. Adapt this line to your DAGradient implementation.
        targetDAGradient.Gradient = currentGradient; 
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