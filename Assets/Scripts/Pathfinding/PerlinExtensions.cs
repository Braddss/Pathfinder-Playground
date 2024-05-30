using Codice.CM.Common;
using System.Net.NetworkInformation;
using TreeEditor;
using UnityEngine;

namespace Braddss.Pathfinding
{
    public static class PerlinExtensions
    {
        public static float OctaveNoise(this Perlin perlin, Vector2 index, PerlinConfig config)
        {
            perlin.SetSeed(config.seed);

            var rng = new System.Random(config.seed);
            float offsetRange = 1000;

            float result = 0f;
            float frequency = config.frequency;
            float amplitude = config.amplitude;

            for (int i = 0; i < config.numOctaves; i++)
            {
                var inputX = index.x * frequency + ((float)rng.NextDouble() * 2 - 1) * offsetRange;
                var inputY = index.y * frequency + ((float)rng.NextDouble() * 2 - 1) * offsetRange;
                result += perlin.Noise(inputX, inputY) * amplitude;

                frequency *= config.octFrequency;
                amplitude *= config.octAmplitude;
            }

            return result;
        }
    }
}
