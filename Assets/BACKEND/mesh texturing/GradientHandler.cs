using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gradient
{
    // within the shader, the array is oversized to 20 so an array is needed of this size
    // use properties rather than straight variables so that getters and setters can be easily made
    public Color[] GradientArray { get; private set; } = new Color[20];
    public int Length { get; private set; } = 0;
    public void AddColor(Color color, float position)
    {
        // if max length, you can't add any more colours
        if (Length == 20)
        {
            Debug.Log("Gradient array is full.");
            return;
        }

        // encode position within the colour using unused alpha slot (for fixed4)
        color.a = position;

        // e.g. when you have 3 colours, the next colour will be added at slot 3 due to zero-indexing
        GradientArray[Length] = color;
        Length++;
    }
}

public class GradientHandler : MonoBehaviour
{
    public Material mat;

    public void RenderGradient(Gradient gradient)
    {
        // if there's no mesh yet, then this can't proceed
        if (gameObject.GetComponent<MeshGenerator>().region == null)
        {
            return;
        }

        // pass in the array and gradient length
        mat.SetColorArray("gradient", gradient.GradientArray);
        mat.SetInt("gradient_length", gradient.Length);

        // to render correctly, the max height of the region is also needed
        float region_height = gameObject.GetComponent<MeshGenerator>().region.height;
        mat.SetFloat("region_height", region_height);
    }
}
