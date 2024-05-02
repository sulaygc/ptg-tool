using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class UIColourHandler : MonoBehaviour
{
    // the only fixed colours are the 2 at the start and end (0 and 100%)
    // the fixed ones are public as they already exist and aren't created by this script
    const int size = 20;
    public GameObject fixed_start;
    public GameObject[] variable_colours { get; private set; } = new GameObject[size - 2];
    public GameObject fixed_end;

    // you also need the current size of the variable array
    public int current_variable_size { get; private set; } = 0;

    // reference to current size text display & add colour button
    public TMP_Text colour_count_display;
    public Button add_colour_button;

    public GameObject colour_component_prefab; // for instantiating new colours
    private float y_spacing = -137.8958f;

    // get reference to gradient preview script attached to gradient preview via Unity Inspector
    public GradientPreview GradientPreview_instance;

    // called by Unity as this script starts
    private void Start()
    {
        // display an accurate gradient preview
        UpdateGradientPreview();
    }

    private Gradient GenerateGradient()
    {
        Gradient gradient = new Gradient();

        // add start colour
        gradient.AddColor(fixed_start.GetComponent<LinkColourComponent>().GetColour(), 0.0f);

        // add the variable colours
        for (int i = 0; i < current_variable_size; i++)
        {
            gradient.AddColor(variable_colours[i].GetComponent<LinkColourComponent>().GetColour(),
                variable_colours[i].GetComponent<LinkColourComponent>().GetPercentage());
        }

        // add the end colour
        gradient.AddColor(fixed_end.GetComponent<LinkColourComponent>().GetColour(), 1.0f);

        return gradient;
    }

    // make public so that other classes & scripts can call this
    public void UpdateGradientPreview()
    {
        Gradient gradient = GenerateGradient();
        GradientPreview_instance.RenderPreview(gradient);

    }

    // method for correcting the displayed positions given a sorted array
    private void FixPositions()
    {
        // get position of the fixed start
        Vector3 start_position = fixed_start.GetComponent<RectTransform>().localPosition;

        // the fixed start should never move so it's the rest that's dealt with
        // also the last one should be dealt with independently as it's not in the array
        for (int i = 0; i < current_variable_size; i++)
        {
            // e.g. if i = 0, it's actually going to need to be spaced once away from the start hence i+1
            // Vector3.up is the unit Y vector
            Vector3 new_position = start_position + ((i + 1) * y_spacing) * Vector3.up;
            variable_colours[i].GetComponent<RectTransform>().localPosition = new_position;
        }

        // display last one correctly
        Vector3 last_position = start_position + ((current_variable_size + 1) * y_spacing) * Vector3.up;
        fixed_end.GetComponent<RectTransform>().localPosition = last_position;
    }

    // method for resorting the variable_colours array
    private void Sort()
    {
        // the objects need to be sorted by their percentages
        // each one will have the LinkColourComponent script from which percentages can be retrieved
        // use linq to sort via a lambda expression that changes the sorting value (the gameobject) into the percentage from that script
        // OrderBy is ascending by default so this should work
        variable_colours = variable_colours.OrderBy(x => SortingKey(x)).ToArray();
    }

    private float SortingKey(GameObject x)
    {
        // if the array position is empty, then it should be sorted to the end
        // so its value should be higher than 1, as everything else is sorted ascending relative to percentage of the colour
        if (x == null)
        {
            return 2;
        }

        // otherwise return the colour's percentage
        return x.GetComponent<LinkColourComponent>().GetPercentage();
    }

    // receives a percentage, and checks whether that percentage would produce a duplicate
    public bool IsValidPct(float pct, GameObject source)
    {
        // can't be 0 or 100% or exceed the range
        if ((pct <= 0) || (pct >= 1))
        {
            return false;
        }

        for (int i = 0; i < current_variable_size; i++)
        {
            // ignore source gameobject - you can't compare for duplicates by comparing to self
            if (variable_colours[i].transform.Find("% Input").gameObject == source)
            {
                continue; // skips rest of this iteration
            }

            // return invalid if duplicate
            if (variable_colours[i].GetComponent<LinkColourComponent>().GetPercentage() == pct)
            {
                return false;
            }
        }

        // if false hasn't been returned, then it's valid
        // on top of this, if it's valid then that means that the colour preview needs to be fixed
        // i.e. the array needs to be resorted & redisplayed
        Sort();
        FixPositions();

        // if resorting has occurred, then the gradient preview needs to be kept up to date
        UpdateGradientPreview();

        return true; // let the percentage validator know it's valid & so to leave the input field alone
    }

    // this method should be invoked directly by the onClick event of the button
    public void AddColour()
    {
        // if colour limit is reached, notify the user by changing the top-right text for 2 seconds
        //  and perform an early return
        if (current_variable_size == size - 2)
        {
            NotifyColourMax(2);
            return;
        }

        // determine the percentage that this new colour should have
        // give it the percentage half way between the 2nd to last and the last (100%)
        float new_pct = GetPctHalfWayToEnd();

        // if the current variable size is 2, then the next array position that's free is position 2
        variable_colours[current_variable_size] = GameObject.Instantiate(colour_component_prefab, transform);
        variable_colours[current_variable_size].GetComponent<LinkColourComponent>().SetPercentage(new_pct);

        // assign the new '% Input' child GameObject's reference back to this parent GameObject, which is needed for percentage validation
        variable_colours[current_variable_size].transform.Find(
            "% Input").GetComponent<PercentageValidator>().UIColourHandler_GameObject = gameObject;

        // assign a script reference this time for the colour preview, which allows it to tell this script that a colour change has occurred
        variable_colours[current_variable_size].transform.Find(
            "Colour View").GetComponent<ColourPreview>().UIColourHandler_instance = this;

        // make the content box bigger (height-wise) for this new colour
        // this script is attached to the context GameObject
        float current_content_box_height = gameObject.GetComponent<RectTransform>().rect.height;
        gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, 
            current_content_box_height + Mathf.Abs(y_spacing)); // use absolute value as y_spacing is negative

        // update colour count
        current_variable_size++;
        DisplayColourCount();

        // make sure the new colour is displayed correctly
        Sort();
        FixPositions();

        // affect gradient with new colour
        UpdateGradientPreview();
    }

    // colour components are held as an array of GameObjects, so passing in the GameObject reference is the easiest way to do this
    public void RemoveColour(GameObject colour_GameObject)
    {
        // delete the GameObject and set the array position's value to null
        for (int i = 0; i < current_variable_size; i++)
        {
            if (variable_colours[i] == colour_GameObject)
            {
                variable_colours[i] = null;

                // colour found so the for loop can end & the colour can be deleted
                Destroy(colour_GameObject);
                break;
            }
        }

        // make the content box smaller (height-wise) to account for there being less colours to scroll through
        // this script is attached to the context GameObject hence the direct reference
        float current_content_box_height = gameObject.GetComponent<RectTransform>().rect.height;
        gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            current_content_box_height - Mathf.Abs(y_spacing)); // use absolute value as y_spacing is negative

        // decrement colour count & update display
        current_variable_size--;
        DisplayColourCount();

        // fix the UI for the list of colours
        Sort();
        FixPositions();

        // update gradient to account for deleted colour
        UpdateGradientPreview();
    }

    // returns the percentage that's half way between the 2nd to last colour's and the end's (100%)
    private float GetPctHalfWayToEnd()
    {
        // if the variable array is empty, then it's 50%
        if (current_variable_size == 0) { return 0.5f; }
        else
        {
            // e.g. if there's 4 colours, then the last colour is at 3
            float midpoint = (variable_colours[current_variable_size - 1].GetComponent<LinkColourComponent>().GetPercentage() + 1) / 2;
            return midpoint;
        }
    }

    private void DisplayColourCount()
    {
        // ensure the add colour button is reenabled if not enabled already
        add_colour_button.interactable = true;

        colour_count_display.text =
            "Colour count: " + "<b>"
            + (current_variable_size + 2).ToString() + "/" + size.ToString()
            + "</b>";

        // the default black used for text
        colour_count_display.color = new UnityEngine.Color(50 / 255, 50 / 255, 50 / 255);
    }

    private void NotifyColourMax(float seconds)
    {
        colour_count_display.text = "Colour max reached";
        colour_count_display.color = UnityEngine.Color.red;

        // disable add colour button for duration so that it can't be spammed
        add_colour_button.interactable = false;

        Invoke("DisplayColourCount", seconds);
    }
}
