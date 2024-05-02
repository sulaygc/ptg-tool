using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class FileSystemHandler : MonoBehaviour
{
    // reference via Unity Inspector to LinkParamUI, which has references to all of the sliders, input fields, etc.
    public LinkParamUI parameters;

    // reference to the UI Colour Handler (via Unity Inspector) so that the list of colours can be sourced from the script
    public UIColourHandler UIColourHandler_instance;

    // run when Unity starts this script
    void Start()
    {
        // if it's not a new file, then the file needs to be loaded
        if (SaveLoadManager.is_new_file == false)
        {
            LoadFile();
        }
    }

    public SaveNotifier SaveNotifier_instance;

    // run each frame
    void Update()
    {
        // check if the UI is currently in use (i.e. if a UI element is selected)
        // if so, then the keyboard may be in use so ignore keybinds
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            // as per the keybind info, if they're pressing ` then save the project
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                SaveFile();

                // also need to display the pop-up that lets the user know that it's been saved
                SaveNotifier_instance.Notify();
            }
        }
    }

    public void SaveFile()
    {
        // false indicates append mode is off (i.e. overwrite old data)
        StreamWriter writer = new StreamWriter(SaveLoadManager.filepath, false);

        // provide all of the values from the parameter menu
        writer.WriteLine
            (
                parameters.dimension_field_x.text + ","
                + parameters.dimension_field_y.text + ","
                + parameters.dimension_field_z.text + ","
                + parameters.resolution_slider.value.ToString() + ","

                + parameters.noise_seed_field.text + ","
                + parameters.isovalue_slider.value.ToString() + ","
                + parameters.num_octaves_slider.value.ToString() + ","

                + parameters.frequency_slider.value.ToString() + ","
                + parameters.lacunarity_slider.value.ToString() + ","
                + parameters.amplitude_slider.value.ToString() + ","
                + parameters.persistence_slider.value.ToString() + ","

                + parameters.subtract_y_toggle.isOn.ToString()
            );

        // gradient preview doesn't need to be saved in any way - the gradient can just be passed to the gradient popup
        //      the popup will then update the preview as a result of the data it receives

        // firstly, store the start and end colours
        // percentages not needed as they must be 0 and 1
        writer.WriteLine
            (
                UIColourHandler_instance.fixed_start.GetComponent<LinkColourComponent>().GetColour().r.ToString() + ","
                + UIColourHandler_instance.fixed_start.GetComponent<LinkColourComponent>().GetColour().g.ToString() + ","
                + UIColourHandler_instance.fixed_start.GetComponent<LinkColourComponent>().GetColour().b.ToString() + ","

                + UIColourHandler_instance.fixed_end.GetComponent<LinkColourComponent>().GetColour().r.ToString() + ","
                + UIColourHandler_instance.fixed_end.GetComponent<LinkColourComponent>().GetColour().g.ToString() + ","
                + UIColourHandler_instance.fixed_end.GetComponent<LinkColourComponent>().GetColour().b.ToString()
            );

        // next, store the extra variable colours (rgb + percentage)
        for (int i=0; i < UIColourHandler_instance.current_variable_size; i++)
        {
            LinkColourComponent link_instance = UIColourHandler_instance.variable_colours[i].GetComponent<LinkColourComponent>();
            writer.Write
                (
                    link_instance.GetColour().r.ToString() + ","
                    + link_instance.GetColour().g.ToString() + ","
                    + link_instance.GetColour().b.ToString() + ","
                    + link_instance.GetPercentage().ToString()
                );

            // if last iteration then don't end with a comma
            if (i + 1 < UIColourHandler_instance.current_variable_size)
            {
                writer.Write(",");
            }
        }

        // if there are no variable colours, indicate this via -1
        if (UIColourHandler_instance.current_variable_size == 0)
        {
            writer.Write("-1");
        }

        writer.Write(Environment.NewLine); // ensure that any further writes occur on the next line

        writer.Close();
    }

    // note: LoadFile as a method is built upon the premise that it's being called upon a completely new file
    //       as this allows it to operate around defaults e.g. the existence of two fixed colours
    //       and as this allows it to simulate how a user would modify a new project and replicate the exact same results
    // therefore calling this at any point but initialisation will cause issues
    public void LoadFile()
    {
        // open file and parse into a list of lines
        string[] lines = System.IO.File.ReadAllLines(SaveLoadManager.filepath);

        // as per SaveFile's ordering
        LoadParameters(lines[0]);
        LoadFixedGradientColours(lines[1]);
        LoadVariableGradientColours(lines[2]);
    }

    // classes below are all subprocesses of the LoadFile method

    private void LoadParameters(string line_to_parse)
    {
        string[] parameters_to_parse = line_to_parse.Split(",");

        // the ordering is directly sourced from the saving order in SaveFile
        parameters.dimension_field_x.text = parameters_to_parse[0];
        parameters.dimension_field_y.text = parameters_to_parse[1];
        parameters.dimension_field_z.text = parameters_to_parse[2];
        parameters.resolution_slider.value = float.Parse(parameters_to_parse[3]);

        parameters.noise_seed_field.text = parameters_to_parse[4];
        parameters.isovalue_slider.value = float.Parse(parameters_to_parse[5]);
        parameters.num_octaves_slider.value = float.Parse(parameters_to_parse[6]);

        parameters.frequency_slider.value = float.Parse(parameters_to_parse[7]);
        parameters.lacunarity_slider.value = float.Parse(parameters_to_parse[8]);
        parameters.amplitude_slider.value = float.Parse(parameters_to_parse[9]);
        parameters.persistence_slider.value = float.Parse(parameters_to_parse[10]);

        parameters.subtract_y_toggle.isOn = bool.Parse(parameters_to_parse[11]);
    }

    private void LoadFixedGradientColours(string line_to_parse)
    {
        string[] split_rgbs_to_parse = line_to_parse.Split(",");

        // refer to SaveFile - the colours are stored as
        // start.r,start.g,start.b,end.r,end.g,end.b
        Color start_colour = new Color
            (
                float.Parse(split_rgbs_to_parse[0]),
                float.Parse(split_rgbs_to_parse[1]),
                float.Parse(split_rgbs_to_parse[2])
            );
        Color end_colour = new Color
            (
                float.Parse(split_rgbs_to_parse[3]),
                float.Parse(split_rgbs_to_parse[4]),
                float.Parse(split_rgbs_to_parse[5])
            );

        // use newly written method in LinkColourComponent - ensure that script has all references set correctly in Inspector
        UIColourHandler_instance.fixed_start.GetComponent<LinkColourComponent>().SetColour(start_colour);
        UIColourHandler_instance.fixed_end.GetComponent<LinkColourComponent>().SetColour(end_colour);

        // these SetColour calls are occurring before UpdatePreview has been attached as a listener to slider changes
        // therefore UpdateColour needs to be called manually
        UIColourHandler_instance.fixed_start.transform.Find("Colour View").GetComponent<ColourPreview>().UpdateColour();
        UIColourHandler_instance.fixed_end.transform.Find("Colour View").GetComponent<ColourPreview>().UpdateColour();
    }

    private void LoadVariableGradientColours(string line_to_parse)
    {
        // if the line just has '-1' then there's no additional colours to deal with
        if (line_to_parse == "-1")
        {
            return;
        }

        string[] split_colour_data_to_parse = line_to_parse.Split(",");

        // otherwise deal with the colours in groups of four (as per SaveFile)
        // colour.r,colour.g,colour.b,colour pct, ... etc.
        for (int i = 0; i < split_colour_data_to_parse.Length; i += 4)
        {
            // the easiest way is to just use AddColour as usual and then correct the percentage and colour later
            // this is because AddColour handles making the UI look correct for newly added colours
            // and because you'll be calling these Load methods from a blank slate i.e. a project with no variable colours added yet
            UIColourHandler_instance.AddColour();

            // now get a reference to this latest colour's linking script and set the colour and percentage accordingly
            LinkColourComponent colour_link =
                // e.g. if there's 3 additional colours, then the last one is at array position 2
                UIColourHandler_instance.variable_colours[UIColourHandler_instance.current_variable_size - 1].GetComponent<LinkColourComponent>();

            // make code cleaner and easier to understand by parsing the colour and percentage prior
            Color colour_rgb_data = new Color
                (
                    float.Parse(split_colour_data_to_parse[i]),
                    float.Parse(split_colour_data_to_parse[i+1]),
                    float.Parse(split_colour_data_to_parse[i+2])
                );
            float colour_pct_data = float.Parse(split_colour_data_to_parse[i + 3]);

            colour_link.SetColour(colour_rgb_data);
            colour_link.SetPercentage(colour_pct_data);

            // these SetColour calls are occurring before UpdatePreview has been attached as a listener to slider changes
            // therefore UpdateColour needs to be called manually
            // the passed in value is irrelevant - it's not used for reasons explained within UpdateColour
            colour_link.transform.Find("Colour View").GetComponent<ColourPreview>().UpdateColour();
        }
    }
}
