using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script is to be attached to the Remove Colour Button directly
//      with the button's OnClick event attached to the public method within this class instance
public class RemoveColourButton : MonoBehaviour
{
    // reference to the central colour component GameObject parent (via Unity Inspector)
    public GameObject colour_GameObject;

    // the goal of this method is to call the UIColourHandler script's RemoveColour method
    //   that method requires a reference to the colour component GameObject requesting deletion though
    //   & that information cannot be passed by OnClick hence this middleman method
    public void RequestColourRemoval()
    {
        // all colour GameObjects are instantiated as direct children of the UI Colour Handler
        UIColourHandler UIColourHandler_instance = colour_GameObject.transform.parent.GetComponent<UIColourHandler>();

        UIColourHandler_instance.RemoveColour(colour_GameObject);
    }
}
