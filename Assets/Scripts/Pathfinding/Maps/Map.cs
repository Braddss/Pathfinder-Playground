using Braddss.Pathfinding.Jobs;
using System.Runtime.CompilerServices;
using TreeEditor;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Braddss.Pathfinding.Maps
{
    public struct Map
    {
        public NativeList<Tile> defaultTile;

        public NativeList<Tile> tiles;

        private readonly Perlin perlin;

        private Vector2Int size;

        public Map(Vector2Int size, PerlinConfig config, float isoValue)
        {
            this.size = size;
            
            defaultTile = new NativeList<Tile>(1, Allocator.Persistent)
            {
                Tile.Default()
            };

            perlin = new Perlin(config.seed);

            if (config.frequency == 0)
            {
                tiles = new NativeList<Tile>(size.x * size.y, Allocator.Persistent);

                for (int i = 0; i < tiles.Length; i++)
                {
                    Vector2Int index = IndexToVec(i);
                    tiles.Add(new Tile(i, index, 100));
                }

                return;
            }

            var passableArr = new NativeArray<Tile>(size.x * size.y, Allocator.TempJob);

            var job = new MapJob
            {
                tiles = passableArr,
                size = size,
                config = config,
                perlin = perlin,
                isoValue = isoValue,
            };

            job.Schedule(passableArr.Length, 256).Complete();
            //job.Run(passableArr.Length);

            //for (int i = 0; i < Tiles.Length; i++)
            //{
            //    Vector2Int index = IndexToVec(i);

            //    bool passable = perlin.OctaveNoise(index, config) < isoValue;
            //    Tiles[i] = new Tile(index, passable);
            //}


            tiles = new NativeList<Tile>(passableArr.Length, Allocator.Persistent);

            tiles.AddRange(passableArr);

            //for (int i = 0; i < passableArr.Length; i++)
            //{
            //    Vector2Int index = IndexToVec(i);
            //    tiles.Add(new Tile(i, index, passableArr[i]));
            //}

            passableArr.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Tile GetTile(Vector2Int index)
        {
            return ref IndexInBounds(index) ? ref tiles.ElementAt(ToIndex(index)) : ref defaultTile.ElementAt(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Tile GetTile(int index)
        {
            return ref tiles.ElementAt(index);
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
