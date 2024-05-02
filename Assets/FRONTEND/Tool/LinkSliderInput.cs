using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LinkSliderInput : MonoBehaviour
{
    // this allows you to attach these gameobjects in the inspector
    public Slider slider;
    public TMP_InputField input_field;

    private void Start()
    {
        // set input box initial display
        UpdateInputBox();
    }

    // the slider does its own validation so if a slider value is valid it can be passed directly to the input box
    public void UpdateInputBox()
    {
        input_field.text = Convert.ToString(slider.value);
    }

    // this function will need special validation in case out-of-range data is inputted for the sliders
    public void UpdateSlider()
    {
        float input_value = float.Parse(input_field.text);

        // use clamp function - it sets values above the max to max, values below min to min
        input_value = Mathf.Clamp(input_value, slider.minValue, slider.maxValue);

        // update input field to clamped input
        input_field.text = Convert.ToString(input_value);

        // pass this value to slider
        slider.value = input_value;
    }
}
