using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class ColourPreview : MonoBehaviour
{
    // reference to UI Colour Handler, in order to tell it to update the gradient preview
    public UIColourHandler UIColourHandler_instance;

    // this script should be attached directly to the raw image gameobject
    // hence the lack of a gameobject reference to the raw image

    // sliders to source validated values from
    public Slider r_slider;
    public Slider g_slider;
    public Slider b_slider;

    // set event listeners for when the values change
    private void Start()
    {
        // temporary iterable
        Slider[] sliders = new Slider[3]
        {
            r_slider, g_slider, b_slider
        };

        foreach (Slider slider in sliders)
        {
            slider.onValueChanged.AddListener(UpdateColour);
        }
    }

    public void UpdateColour(float value = 0f)
    {
        // a listener to onValueChanged must take in the changed value, but figuring out which slider sent the value
        //  would actually complicate things as it would require me to override the built-in Unity UI event code
        //  so the input value will just be ignored (hence the placeholder in case this is called independently)

        // alpha is set to 1 by default
        // divide by 255 as Unity uses RGB components between 0 and 1
        gameObject.GetComponent<RawImage>().color = new Color
            (
                r_slider.value / 255,
                g_slider.value / 255,
                b_slider.value / 255
            );

        // the gradient preview must also be updated
        UIColourHandler_instance.UpdateGradientPreview();
    }
}
