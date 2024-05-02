using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LimitInputRange : MonoBehaviour
{
    private TMP_InputField input_field;

    // can be specified in unity inspector, default 0
    public float minimum = 0;

    // this is an extension of the old LimitInputMinimum script, which doesn't have maximums
    public float maximum = 100; // for the percentage box
    public bool limit_max = false; // false by default as to not break previous components

    private void Start()
    {
        // get reference to input_field via the gameobject this is attached to
        input_field = gameObject.GetComponent<TMP_InputField>();

        // automatically assign this to make it easier
        input_field.onEndEdit.AddListener(Limit);
    }

    public void Limit(string text)
    {
        float input_value = float.Parse(text);

        // if less than minimum, set to minimum
        input_value = Mathf.Max(minimum, input_value);

        // if limit_max is true & greater than maximum set to max
        if (limit_max)
        {
            input_value = Mathf.Min(maximum, input_value);
        }

        // set the input box to this value
        input_field.text = Convert.ToString(input_value);
    }
}
