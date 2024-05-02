using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class OpenFileHandler : MonoBehaviour
{
    public GameObject scroll_content_holder;
    public GameObject save_file_UI_prefab;

    // standardised spacing between open save file buttons
    private float UI_spacing_y = 100f;

    // Unity calls this automatically when this script is enabled
    // which will occur when this whole Open section is enabled
    void OnEnable()
    {
        SetUpFilePickerUI();
    }

    // if you go back, and come back into this menu, the list of files should be re-retrieved
    void OnDisable()
    {
        foreach (Transform child in scroll_content_holder.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void LoadFile(string filepath)
    {
        // set SaveLoadManager settings
        SaveLoadManager.is_new_file = false;
        SaveLoadManager.filepath = filepath;

        // start tool
        SceneManager.LoadScene("Tool", LoadSceneMode.Single);
    }

    private void SetUpFilePickerUI()
    {
        string[] save_paths = GetSaveList().ToArray();

        int colour_UIs_instantiated = 0;
        foreach (string save_path in save_paths)
        {
            // instantiate a save file UI component as a child of the scroll content holder
            GameObject new_save_UI = Instantiate(save_file_UI_prefab, scroll_content_holder.transform);

            // assign correct filepath for this save & ensure it has a reference to this script
            new_save_UI.GetComponent<SaveFileData>().filepath = save_path;
            new_save_UI.GetComponent<SaveFileData>().OpenFileHandler_instance = this;

            // the setter for filepath will ensure the button text updates correctly (check SaveFileData)

            // position this new UI component properly as well
            // Vector3.up is (0,1,0) so multiplying it allows you to add to only the Y component
            // it's -= as the UI components actually move down the scroll view
            new_save_UI.transform.position -= Vector3.up * UI_spacing_y * colour_UIs_instantiated;

            // otherwise they won't be spaced correctly
            colour_UIs_instantiated++;
        }

        // make the scroll content region the correct size
        scroll_content_holder.GetComponent<RectTransform>().SetSizeWithCurrentAnchors
            (RectTransform.Axis.Vertical, scroll_content_holder.GetComponent<RectTransform>().rect.height + UI_spacing_y * colour_UIs_instantiated);
    }

    private List<string> GetSaveList()
    {
        // files are stored in Unity's OS-specific directory for persistent files
        // if saved anywhere else, there's a possibility for the data to be overwritten by accident
        string[] filepaths_in_dir = Directory.GetFiles(Application.persistentDataPath);

        // exclude non-ptgproj files if there are any
        List<string> valid_save_filepaths = new List<string>();
        foreach (string filepath in filepaths_in_dir)
        {
            if (Path.GetExtension(filepath) != ".ptgproj")
            {
                continue; // skip the rest of this iteration
            }

            valid_save_filepaths.Add(filepath);
        }

        return valid_save_filepaths;
    }
}
