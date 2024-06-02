using System.Runtime.CompilerServices;
using TreeEditor;
using UnityEngine;

namespace Braddss.Pathfinding
{
    public class Map
    {
        public Tile[] Tiles { get; private set; }

        private readonly Perlin perlin = new Perlin();

        private Vector2Int size;

        public Map(Vector2Int size, PerlinConfig config, float isoValue)
        {
            this.size = size;
            Tiles = new Tile[size.x * size.y];

            for (int i = 0; i < Tiles.Length; i++)
            {
                Vector2Int index = IndexToVec(i);

                bool passable = perlin.OctaveNoise(index, config) < isoValue;
                Tiles[i] = new Tile(index, passable);
            }
        }

        //public Tile[] Neighbors(Tile tile)
        //{
        //    Tile[] neighbors = new Tile[4]; //tile.NeighborCount];

        //    Vector2Int[] directions = new Vector2Int[]
        //    {
        //        Vector2Int.down,
        //        Vector2Int.left,
        //        Vector2Int.up,
        //        Vector2Int.right,
        //    };


        //    for (int i = 0; i < directions.Length; i++)
        //    {
        //        Vector2Int neighborIndex = tile.Index + directions[i];
        //        neighbors[i] = IndexInBounds(neighborIndex) ? Tiles[ToIndex(neighborIndex)] : Tile.OOB();
        //    }

        //    return Tiles;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tile GetTile(Vector2Int index)
        {
            return IndexInBounds(index) ? Tiles[ToIndex(index)] : Tile.OOB();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector2Int IndexToVec(int i)
        {
            return new Vector2Int(i % size.x, i / size.x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ToIndex(Vector2Int vec)
        {
            return vec.x + (vec.y * size.x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IndexInBounds(Vector2Int index)
        {
            return index.x >= 0 && index.x < size.x && index.y >= 0 && index.y < size.y;
        }
    }
}
