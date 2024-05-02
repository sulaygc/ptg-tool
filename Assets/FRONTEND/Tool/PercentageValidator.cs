using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PercentageValidator : MonoBehaviour
{
    // hold reference to Colour UI Handler parent
    public GameObject UIColourHandler_GameObject;

    // hold reference to last validated input
    // input fields hold text
    private string last_validated_input;

    // when script starts, get the value
    // also assign the event listener
    void Start()
    {
        // this will be attached to the input field's gameobject
        // the colour will be instantiated with a valid percentage
        last_validated_input = gameObject.GetComponent<TMP_InputField>().text;

        // assign validation function to onEndEdit event
        gameObject.GetComponent<TMP_InputField>().onEndEdit.AddListener(ValidatePct);
    }

    public void ValidatePct(string new_input)
    {
        float new_value = float.Parse(new_input);
        bool isValid = UIColourHandler_GameObject.GetComponent<UIColourHandler>().IsValidPct(new_value, gameObject);

        // if not valid, set back to last valid input
        if (!isValid)
        {
            gameObject.GetComponent<TMP_InputField>().text = last_validated_input;
            return; // validation is complete
        }

        // if it hasn't returned, then it is valid & so this becomes the latest validated input
        // also it means that the field already has the right text inside of it
        last_validated_input = new_input;
    }
}
