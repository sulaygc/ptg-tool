using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public void ExitToMainMenu()
    {
        // load mode 'single' means that this scene will be unloaded in the process as desired
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
