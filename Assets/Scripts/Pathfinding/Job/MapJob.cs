using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Braddss.Pathfinding.Jobs
{
    [BurstCompile]
    public struct MapJob : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<byte> tiles;

        public Vector2Int size;

        public PerlinConfig config;

        public Perlin perlin;

        public float isoValue;


        public void Execute(int index)
        {
            Vector2Int index2 = IndexToVec(index);

            var noiseVal = OctaveNoise(index2, config);

            var max = isoValue;
            var min = isoValue - (config.transitionSize / 10);

            var t = (noiseVal - min) / (max - min);

            t = Mathf.Clamp(t, 0, 1);

            var value = (byte)(((float)Mathf.Clamp((int)((1 - t) * (config.steps + 2)), 0, (config.steps + 1)) / (config.steps + 1)) * 100);

            tiles[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector2Int IndexToVec(int i)
        {
            return new Vector2Int(i % size.x, i / size.x);
        }

        public float OctaveNoise(Vector2 index, PerlinConfig config)
        {
            var random = new Unity.Mathematics.Random((uint)config.seed);
            float offsetRange = 1000;

            float result = 0f;
            float frequency = config.frequency / 1000;
            float amplitude = config.amplitude;

            for (int i = 0; i < config.numOctaves; i++)
            {
                var inputX = index.x * frequency + (random.NextFloat(2) - 1) * offsetRange;
                var inputY = index.y * frequency + (random.NextFloat(2) - 1) * offsetRange;
                result += perlin.Noise(inputX, inputY) * amplitude;

                frequency *= config.octFrequency;
                amplitude *= config.octAmplitude;
            }

            return result;
        }
    }
}
