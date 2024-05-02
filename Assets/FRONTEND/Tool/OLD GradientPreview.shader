Shader "Custom/GradientPreview"
{
    // properties are specified within the Unity Inspector for the material
    Properties
    {
        _Color1 ("Color 1", Color) = (1,1,1,1)
        _Color2 ("Color 2", Color) = (0,0,0,0)
    }
    SubShader
    {
        CGPROGRAM

        // surface & surf are necessary for a surface Shader
        // a lighting model must be specified - use a custom one called NoLighting
        // noambient removes effects of ambient lighting
        #pragma surface surf NoLighting noambient

        // this struct is automatically passed in by Unity
        struct Input
        {
            // get world position of "pixel" we're shading
            float3 worldPos;
        };

        // declare these variables that values are passed in for via Properties
        fixed4 _Color1;
        fixed4 _Color2;

        // define our custom lighting model
        // https://docs.unity3d.com/520/Documentation/Manual/SL-SurfaceShaderLighting.html
        half4 LightingNoLighting (SurfaceOutput s, half3 lightDir, half atten)
        {
            // return the RGBA colour completely unaffected
            return half4(s.Albedo, s.Alpha);
        }

        void surf (Input IN, inout SurfaceOutput s)
        {
            // test whether colours are rendered without shading
            s.Albedo = _Color1.rgb;
            s.Alpha = _Color1.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
