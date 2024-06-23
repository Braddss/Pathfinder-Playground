using System;

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

        public int numOctaves;

        public bool blackWhite;
    }
}
