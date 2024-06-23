using System;
using UnityEngine;

namespace Braddss.Pathfinding
{
    [Serializable]
    public struct PerlinConfig
    {
        public int seed;

        public float frequency;

        public float amplitude;

        public float octFrequency;

        public float octAmplitude;

        [Range(0, 20)]
        public int numOctaves;

        [Range(0, 100)]
        public int steps;

        public float transitionSize;
    }
}
