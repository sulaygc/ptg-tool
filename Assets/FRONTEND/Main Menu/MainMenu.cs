using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// is static & does not inherit from MonoBehaviour so that it can persist between scenes
public static class SaveLoadManager
{
    public const string file_extension = ".ptgproj"; // procedural terrain generator project

    // the Tool scene will check this immediately upon starting
    public static bool is_new_file;

    // filepath that the Tool scene will save changes to via the Save button
    // in addition, if it's not a new file then this filepath will also be loaded at startup
    public static string filepath;
}

public class MainMenu : MonoBehaviour
{
    // GameObjects attached via Unity Inspector hence public
    public GameObject initial_UI_holder;
    public GameObject start_new_UI_holder;
    public GameObject open_file_UI_holder;
    public GameObject main_text;

    // attach to OnClick event of 'Start new terrain file' button
    public void StartNew()
    {
        // first, disable the three main menu buttons and ask for a filename
        initial_UI_holder.SetActive(false);
        start_new_UI_holder.SetActive(true);
    }

    // attach to OnClick event of 'Confirm file name' button
    public void ConfirmStartNew()
    {
        // get text from the input field
        // no validation needed as I've already set the input field to only accept alphanumeric characters
        string new_file_name = start_new_UI_holder.transform.Find("File Name Input").GetComponent<TMP_InputField>().text;

        // however the input does need to be rejected if it's empty
        if (new_file_name == "")
        {
            return;
        }

        // pass data to SaveLoadManager so that it can be read by the Tool scene despite this scene being unloaded
        SaveLoadManager.is_new_file = true;
        SaveLoadManager.filepath = Path.Combine(Application.persistentDataPath, new_file_name + SaveLoadManager.file_extension);

        // there are no further steps needed within this scene
        StartTool();
    }

    // attach to OnClick event of any back buttons
    public void ReturnToInitialMenu()
    {
        initial_UI_holder.SetActive(true);
        start_new_UI_holder.SetActive(false);
        open_file_UI_holder.SetActive(false);
        main_text.SetActive(true);
    }

    public void OpenFileMenu()
    {
        // disable main screen UI and enable the open file menu UI
        initial_UI_holder.SetActive(false);
        start_new_UI_holder.SetActive(false);
        main_text.SetActive(false);
        open_file_UI_holder.SetActive(true);

        // the open file section automatically has scripts attached that trigger when made active to proceed from here
    }

    private void StartTool()
    {
        // if this doesn't work, the scene is probably not assigned properly in Unity's Build Settings
        // mode single means that the new scene is the only active scene
        SceneManager.LoadScene("Tool", LoadSceneMode.Single);
    }
}
