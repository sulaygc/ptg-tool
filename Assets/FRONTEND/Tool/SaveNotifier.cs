using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class SaveNotifier : MonoBehaviour
{
    public GameObject save_notifier;

    // if shown again while already being shown
    // then increase this and use it to show the user
    // that another save has occurred
    private int show_count = 1;

    public void Notify()
    {
        // if already being shown, then reset the time before it's going to be hidden
        if (save_notifier.activeInHierarchy == true)
        {
            CancelInvoke("Hide");
        }

        SetText();

        Show();
        Invoke("Hide", 2);
    }

    private void SetText()
    {
        save_notifier.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text =
            // e.g. Saved example.ptgproj (2)
            "Saved " + Path.GetFileName(SaveLoadManager.filepath) + " (" + show_count.ToString() + ")";
    }

    private void Show()
    {
        save_notifier.SetActive(true);
        show_count++;
    }

    private void Hide()
    {
        save_notifier.SetActive(false);
        show_count = 1;
    }
}
