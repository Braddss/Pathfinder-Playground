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

            if (config.blackWhite)
            {
                tiles[index] = OctaveNoise(index2, config) < isoValue ? (byte)100 : (byte)0;
            }
            else
            {
                var noiseVal = OctaveNoise(index2, config);

                if (noiseVal < isoValue)
                {
                    tiles[index] = 100;
                }
                else if (noiseVal - 0.1 < isoValue)
                {
                    tiles[index] = 50;
                }
                else
                {
                    tiles[index] = 0;
                }
            }
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
            float frequency = config.frequency;
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
