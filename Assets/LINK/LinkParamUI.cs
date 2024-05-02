using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LinkParamUI : MonoBehaviour
{
    // these are all public - the easiest thing is to assign these in the Inspector honestly
    // otherwise you need a nested mess of references to object children

    public TMP_InputField dimension_field_x;
    public TMP_InputField dimension_field_y;
    public TMP_InputField dimension_field_z;
    public Slider resolution_slider;

    public TMP_InputField noise_seed_field;
    public Slider isovalue_slider;
    public Slider num_octaves_slider;

    public Slider frequency_slider;
    public Slider lacunarity_slider;
    public Slider amplitude_slider;
    public Slider persistence_slider;

    public Toggle subtract_y_toggle;

    // also need to get a reference to the meshgenerator script
    public GameObject MeshGenerator_GO;
    private MeshGenerator MeshGenerator_Script;

    private void Start()
    {
        // this code is run whenever this script starts
        MeshGenerator_Script = MeshGenerator_GO.GetComponent<MeshGenerator>();
    }

    private void SendRegion()
    {
        // get values from input fields, as they may change
        Region region = new Region
            (
                float.Parse(dimension_field_x.text),
                float.Parse(dimension_field_y.text),
                float.Parse(dimension_field_z.text),
                resolution_slider.value,
                Vector3.zero // you can only create one region, so use origin
            );

        // set the MeshGenerator reference to this region
        MeshGenerator_Script.region = region;
    }

    private void SendNoise()
    {
        // set up octave noise object here, so it can easily be sent
        OctaveNoise octave_noise = new OctaveNoise
            (  
                frequency_slider.value,
                lacunarity_slider.value,
                amplitude_slider.value,
                persistence_slider.value,
                (int)num_octaves_slider.value,
                Int32.Parse(noise_seed_field.text)
            );

        MeshGenerator_Script.octave_noise = octave_noise;

        // also have some misc data that needs to be sent
        MeshGenerator_Script.isovalue = isovalue_slider.value;
        MeshGenerator_Script.sub_y_flag = subtract_y_toggle.isOn;
    }

    // reference to the GradientPreview script, so that whenever a new region is generated a gradient retrieval call can be made
    public GradientPreview GradientPreview_instance;

    public void SendTerrainUpdate()
    {
        // send all prerequisite data
        SendRegion();
        SendNoise();

        // notify MeshGenerator that it can regenerate the terrain
        MeshGenerator_Script.Regenerate();

        // retrieve current gradient from the GradientPreview instance and send it to the mesh as well
        GradientPreview_instance.SendCurrentUIGradientToMesh(); 
    }
}
