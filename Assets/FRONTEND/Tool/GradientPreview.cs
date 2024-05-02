using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class GradientPreview : MonoBehaviour
{
    // hold reference to gradient preview component prefab
    public GameObject gradient_component_prefab;

    // hold an array of instantiated gradient preview components
    private GameObject[] gradient_components;

    private Gradient current_gradient;

    // width of gradient preview (height can be changed via the prefab)
    // this doesn't need to be public though as you can just use the XYZ scale properties of the parent
    // and that won't require any rerendering from this script
    private float width = 1000f;

    // method to receive a gradient, and then render its preview
    public void RenderPreview(Gradient gradient)
    {
        // store this gradient for if rerendering needs to occur
        // e.g. changing width during runtime
        current_gradient = gradient;

        // if the gradient has n colours, you'll need n-1 "transitions"
        // i.e. n-1 game objects
        gradient_components = new GameObject[gradient.Length - 1];

        // clear old gradient preview by deleting all children objects
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }

        // create a gradient between current colour in array and next via textures
        for (int i = 0; i < gradient.Length - 1; i++)
        {
            // create texture
            Texture2D tex = CreateGradientTexture
                (
                    FixAlpha(gradient.GradientArray[i]),
                    FixAlpha(gradient.GradientArray[i + 1])
                );

            // create Raw Image UI gameobject as a child of this gameobject 
            // & assign the texture
            gradient_components[i] = GameObject.Instantiate(gradient_component_prefab, transform);
            gradient_components[i].GetComponent<RawImage>().texture = tex;

            // get pcts for positioning this object correctly
            float from_pct = gradient.GradientArray[i].a;
            float to_pct = gradient.GradientArray[i+1].a;

            // the prefabs are anchored + pivoted at bottom left, so I can set the local X directly
            // it's currently at local position (0,0,0) so adding Vector3.right is the easiest way
            // Vector3.right is (1,0,0) so this will move it the correct amount right
            gradient_components[i].GetComponent<RectTransform>().localPosition += Vector3.right * from_pct * width;

            // the GameObject also needs to be scaled properly - e.g. a 20-40% gradient should take up 20% of the full space
            float pct_diff = to_pct - from_pct;
            gradient_components[i].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * pct_diff);

            // change the UV rect of the RawImage to remove the clamped edges of the two-pixel interpolated gradient
            gradient_components[i].GetComponent<RawImage>().uvRect = new Rect(0.25f, 0f, 0.5f, 1f);
        }

        // whenever RenderPreview is called, it's because the gradient has been changed
        // the mesh needs to be notified of this
        SendCurrentUIGradientToMesh();
    }

    // reference to gradient handler so that a new gradient received by this preview can be passed to the mesh
    public GradientHandler GradientHandler_instance;

    // this is its own separate method (despite its simplicity) as there will be times that this will need to be called
    //      despite the preview itself not changing
    // e.g. when the mesh changes size and so needs to retrieve the gradient again
    public void SendCurrentUIGradientToMesh()
    {
        // gradient doesn't need to be passed in to this method
        // the gradient is already retrieved and stored in RenderPreview to variable current_gradient
        // this makes re-retrieving the currently shown gradient easier
        GradientHandler_instance.RenderGradient(current_gradient);
    }

    private Texture2D CreateGradientTexture(Color color1, Color color2)
    {
        // create a Texture2D, width 2 height 1
        Texture2D tex = new Texture2D(2, 1);

        // create the size-2 array of pixel colours needed
        // between the current colour in the gradient and the next
        Color[] pixel_colors = new Color[2]
        {
                color1,
                color2
        };

        // ensures that the texture scales itself up properly and stretches out instead of mirroring, etc.
        tex.wrapMode = TextureWrapMode.Clamp;

        // assign colours to pixels
        tex.SetPixels(pixel_colors);
        tex.Apply();

        return tex;

    }

    private Color FixAlpha(Color color)
    {
        // the gradient array holds positions in the alpha slot, which is fine for the opaque shader
        // here though the alpha will actually affect what's displayed so the colour's alpha needs to be set to 1
        return new Color(color.r, color.g, color.b, 1);
    }
}
