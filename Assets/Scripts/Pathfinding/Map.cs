using Braddss.Pathfinding;
using System.Linq;
using System.Runtime.CompilerServices;
using TreeEditor;
using UnityEngine;

namespace Braddss.Pathfinding
{
    public class Map
    {
        public Tile[] Tiles { get; private set; }

        private Perlin perlin = new Perlin();

        private Vector2Int size;

        public Map(Vector2Int size, PerlinConfig config, float isoValue)
        {
            this.size = size;
            Tiles = new Tile[size.x * size.y];

            for (int i = 0; i < Tiles.Length; i++)
            {
                var index = IndexToVec(i);

                var neighborCount = 4;

                if (index.x == 0 || index.x == size.x - 1)
                {
                    neighborCount -= 1;
                }

                if (index.y == 0 || index.y == size.y - 1)
                {
                    neighborCount -= 1;
                }

                var passable = perlin.OctaveNoise(index, config) < isoValue;
                Tiles[i] = new Tile(index, passable, neighborCount);
            }
        }

        public Tile[] Neighbors(Tile tile)
        {
            var neighbors = new Tile[4]; //tile.NeighborCount];

            var directions = new Vector2Int[]
            {
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.right,
            };


            for (int i = 0; i < directions.Length; i++) 
            {
                var neighborIndex = tile.Index + directions[i];
                neighbors[i] = IndexInBounds(neighborIndex) ? Tiles[VecToIndex(neighborIndex)] : Tile.OOB();
            }

            return Tiles;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tile GetTile(Vector2Int index)
        {
            return IndexInBounds(index) ? Tiles[VecToIndex(index)] : Tile.OOB();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector2Int IndexToVec(int i)
        {
            return new Vector2Int(i % size.x, i / size.x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int VecToIndex(Vector2Int vec)
        {
            return vec.x + vec.y * size.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IndexInBounds(Vector2Int index)
        {
            return index.x >= 0 && index.x < size.x && index.y >= 0 && index.y < size.y;
        }
    }
}
