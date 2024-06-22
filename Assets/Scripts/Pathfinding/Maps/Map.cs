using Braddss.Pathfinding.Jobs;
using System.Runtime.CompilerServices;
using TreeEditor;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Braddss.Pathfinding.Maps
{
    public class Map
    {
        public Tile[] Tiles { get; private set; }

        private readonly Perlin perlin;

        private Vector2Int size;

        public Map(Vector2Int size, PerlinConfig config, float isoValue)
        {
            this.size = size;
            Tiles = new Tile[size.x * size.y];

            if (config.frequency == 0)
            {
                for (int i = 0; i < Tiles.Length; i++)
                {
                    Vector2Int index = IndexToVec(i);
                    Tiles[i] = new Tile(index, true);
                }

                return;
            }

            var passableArr = new NativeArray<bool>(size.x * size.y, Allocator.TempJob);

            perlin = new Perlin(config.seed);

            var job = new MapJob
            {
                tiles = passableArr,
                size = size,
                config = config,
                perlin = perlin,
                isoValue = isoValue,
            };

            var handle = job.Schedule(passableArr.Length, 64);


            //for (int i = 0; i < Tiles.Length; i++)
            //{
            //    Vector2Int index = IndexToVec(i);

            //    bool passable = perlin.OctaveNoise(index, config) < isoValue;
            //    Tiles[i] = new Tile(index, passable);
            //}

            for (int i = 0; i < Tiles.Length; i++)
            {
                Vector2Int index = IndexToVec(i);
                Tiles[i] = new Tile(index, false);
            }

            handle.Complete();

            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i].Passable = passableArr[i];
            }
            passableArr.Dispose();
        }

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
