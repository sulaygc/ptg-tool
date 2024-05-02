using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctaveNoise
{
    // i've decided to use a class, as you can instantiate this class with all of the parameters which shouldn't change between points
    // it would be interesting if a lot of the parameters could vary for different points but that's outside of the scope of this project for now

    // i'll handle all of the FastNoiseLite stuff in this class as well as a means of abstraction
    private FastNoiseLite noise;

    private int octaves;
    private float frequency; private float lacunarity;
    private float amplitude; private float persistence;

    public OctaveNoise(float frequency, float lacunarity, float amplitude, float persistence, int octaves, int seed)
    {
        // instantiate FastNoise object so that you can just call GetNoise on it later with your xyz
        noise = new FastNoiseLite(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S); // use of opensimplex2s will be explained in documentation

        this.frequency = frequency;
        this.lacunarity = lacunarity;
        this.amplitude = amplitude;
        this.persistence = persistence;
        this.octaves = octaves;
    }

    public float GetNoise(Vector3 position)
    {
        float current_value = 0;
        float current_frequency = frequency;
        float current_amplitude = amplitude;

        for (int i = 0; i < octaves; i++)
        {
            // all three coordinates need to be affected equally by the frequency
            Vector3 modified_position = position * current_frequency;
            current_value += current_amplitude * noise.GetNoise(modified_position.x, modified_position.y, modified_position.z);

            // now apply the lacunarity and persistence - you could use exponentiation but this makes the code easier to understand
            current_frequency *= lacunarity; // a typical lacunarity value is 2.0
            current_amplitude *= persistence; // a typical persistence value is 0.5, technically you could do this as a division by 2.0 but multiplication is the standard
        }
        return current_value;
    }
}
