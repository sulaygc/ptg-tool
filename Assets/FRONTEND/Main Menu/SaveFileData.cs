using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveFileData : MonoBehaviour
{
    // this will be set by the OpenFileHandler when this is instantiated
    // this style of getters and setters works via C# properties
    // hence the "backing field" _filepath
    string _filepath;
    public string filepath
    {
        get
        {
            return _filepath;
        }
        set
        {
            // ensure that the button text reflects the file you're opening
            button_text.text = Path.GetFileName(value);

            _filepath = value;
        }
    }

    // reference to the OpenFileHandler instance in order to let it know the button has been pressed
    // this will also be set upon instantiation by the OpenFileHandler itself
    public OpenFileHandler OpenFileHandler_instance;

    // reference to the button's text so that the file name can be displayed
    public TMP_Text button_text;

    // method to be attached to the OnClick method of the button
    public void NotifyFileOpenRequest()
    {
        // let the OpenFileHandler know the file to be opened
        OpenFileHandler_instance.LoadFile(filepath);
    }
}
