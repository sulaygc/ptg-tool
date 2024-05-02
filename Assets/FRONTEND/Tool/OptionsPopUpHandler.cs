using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsPopUpHandler : MonoBehaviour
{
    // assign via Unity Inspector
    public GameObject popup;

    // reference to the gradient pop up handler
    // that popup needs to be closed if this one is opened
    public GradientPopUpHandler GradientPopUpHandler_instance;

    // setting the parent to inactive does the same for all its children
    // inactive just makes it disappear/stop running scripts while preserving data which is what we need

    public void CloseOptionsPopUp()
    {
        popup.SetActive(false);
    }

    public void OpenOptionsPopUp()
    {
        GradientPopUpHandler_instance.CloseGradientPopUp();
        popup.SetActive(true);
    }
}
