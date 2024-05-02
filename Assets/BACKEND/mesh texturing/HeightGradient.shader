Shader "Custom/HeightGradient"
{
    SubShader
    {
        // LOD is a necessary value for when you have multiple SubShaders; it doesn't do anything here but is needed
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            // worldPos will automatically be passed in by Unity
            // explained here: https://docs.unity3d.com/Manual/SL-SurfaceShaders.html
            float3 worldPos;
        };

        // gradient array must have a defined length at compile time, so I can just oversize the array
        // realistically with computable region sizes you probably won't use more than 10 different colours, so 20 is fine
        fixed4 gradient[20];
        int gradient_length; // this can be set at runtime and hold the true length

        // region information
        float region_height;

        // this function returns a value between 0 and 1 indicating the percentage a value is between two others
        float InvLerp(float value1, float value2, float value_in_between)
        {
            // first find the vector from the start to the end
            float total_change = value2 - value1;

            // now find the vector from the start to the value in between
            float change = value_in_between - value1;

            // divide this vector by the total_change vector to get a percentage
            return change / total_change;
        }

        fixed4 LerpGradientColors(fixed4 color1, fixed4 color2, float position)
        {
            // get the lerp percentage
            float pct_through = InvLerp(float(color1[3]), float(color2[3]), position);

            fixed4 interpolated_color = lerp(color1, color2, pct_through);

            // this will have also interpolated the 4th value of the colours, which will be confusing if transparency is eventually enabled for the mesh
            // so we can just set the alpha value to 1
            interpolated_color[3] = 1;

            return interpolated_color;
        }

        fixed4 GradientColor(float position)
        {
            // deal with the start
            if (position == 0)
            {
                // fix alpha value as done earlier
                fixed4 start_color = gradient[0];
                start_color[3] = 1;

                return start_color;
            }

            // deal with the end
            if (position == 1)
            {
                // fix alpha value as done earlier
                fixed4 end_color = gradient[gradient_length - 1];
                end_color[3] = 1;

                return end_color;
            }

            // loop through to find the two colours the position lies between
            // it's gradient_length - 1 as you're comparing the current colour and the colour after
            for (int i = 0; i < gradient_length - 1; i++)
            {
                if (float(gradient[i][3]) <= position && float(gradient[i + 1][3]) > position)
                {
                    // it's between these two colours
                    return LerpGradientColors(gradient[i], gradient[i + 1], position);
                }
            }

            // if nothing is returned after looping through there's an issue, return black as an error colour
            return fixed4(0, 0, 0, 1);
        }

        // this function will be called as the 'main function' of the shader
        // both parameters are passed in by Unity
        // the SurfaceOutputStandard is specified as 'inout' as it can be modified and its final state will be sent back for rendering
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // get Y position as a percentage of the max height; use this as a gradient position
            // clamp the value to a range of 0.0 to 1.0 just in case anything gets drawn outside of this region and produces a value out of range
            float position = clamp(IN.worldPos.y / region_height, 0.0, 1.0);
            o.Albedo = GradientColor(position);
        }
        ENDCG
    }
    // use Unity's default 'Diffuse' shader as a fallback
    FallBack "Diffuse"
}
