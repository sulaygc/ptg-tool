using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LinkColourComponent : MonoBehaviour
{
    // get references to RawImage & the percentage input field via Inspector
    public RawImage colour_preview;
    public TMP_InputField pct_input;

    // references needed for setting colours (via Inspector as well)
    public Slider r_slider;
    public Slider g_slider;
    public Slider b_slider;

    public float GetPercentage()
    {
        return float.Parse(pct_input.text);
    }

    public void SetPercentage(float value)
    {
        pct_input.text = value.ToString();
    }

    public Color GetColour()
    {
        return colour_preview.color;
    }

    public void SetColour(Color colour)
    {
        // unity holds colours with floats between 0 and 1, but the standard is 0-255 as my UI does
        r_slider.value = colour.r * 255;
        g_slider.value = colour.g * 255;
        b_slider.value = colour.b * 255;
    }
}
