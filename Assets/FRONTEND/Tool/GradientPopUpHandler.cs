using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientPopUpHandler : MonoBehaviour
{
    // assign via Unity Inspector
    public GameObject popup;

    // reference to the options pop up handler
    // that popup needs to be closed if this one is opened
    public OptionsPopUpHandler OptionsPopUpHandler_instance;

    // use Unity Inspector to attach the close popup button OnClick event to this method
    public void CloseGradientPopUp()
    {
        popup.SetActive(false);
    }

    // attach gradient menu's 'Edit Gradient...' button's OnClick to this
    public void OpenGradientPopUp()
    {
        OptionsPopUpHandler_instance.CloseOptionsPopUp();
        popup.SetActive(true);
    }
}
