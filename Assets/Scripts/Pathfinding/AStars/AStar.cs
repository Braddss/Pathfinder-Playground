using Braddss.Pathfinding.Maps;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Braddss.Pathfinding.Astars
{
    internal class AStar : IPathfinder
    {
        private static Vector2Int[] neighborDirs = new Vector2Int[]
        {
            Vector2Int.down,
            Vector2Int.down + Vector2Int.left,
            Vector2Int.left,
            Vector2Int.left + Vector2Int.up,
            Vector2Int.up,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.right,
            Vector2Int.right + Vector2Int.down,
        };

        private readonly NativeList<int> open = new NativeList<int>(1000, Allocator.Persistent);
        private readonly NativeList<int> closed = new NativeList<int>(1000, Allocator.Persistent);
        private readonly NativeHashSet<int> openSet = new NativeHashSet<int>(1000, Allocator.Persistent);
        private readonly NativeHashSet<int> closedSet = new NativeHashSet<int>(1000, Allocator.Persistent);

        private List<Vector2Int> pathDirections = new List<Vector2Int>();

        private Tile current = Tile.Default();

        public Vector2Int Start { get; private set; }
        public Vector2Int End { get; private set; }

        private readonly Map map;

        public IEnumerable<Tile> Open 
        { 
            get
            {
                for (int i = 0; i < open.Length; i++)
                {
                    yield return map.GetTile(open[i]);
                }
            } 
        }

        public IEnumerable<Tile> Closed
        {
            get
            {
                for (int i = 0; i < closed.Length; i++)
                {
                    yield return map.GetTile(closed[i]);
                }
            }
        }

        private float heuristicMultiplier;

        public AStar(Map map, float heuristicMultiplier)
        {
            this.map = map;
            this.heuristicMultiplier = heuristicMultiplier;
        }

        public Vector2Int[] CalculatePath(Vector2Int start, Vector2Int end)
        {
            InitAStar(start, end);

            while (true)
            {
                var result = Step();

                if (result != null)
                {
                    return result;
                }
            }
        }

        public void InitCalculatePathStepwise(Vector2Int start, Vector2Int end)
        {
            InitAStar(start, end);
        }

        public Vector2Int[] CalculatePathStepwise()
        {
            return Step();
        }

        public Vector2Int[] GetTempPath()
        {
            if (current.IsDefault())
            {
                return new Vector2Int[0];
            }

            return CalculatePath(current);
        }

        private void InitAStar(Vector2Int start, Vector2Int end)
        {
            Clear();
            this.Start = start;
            this.End = end;

            ref var startTile = ref map.GetTile(start);
            open.Add(startTile.Index);
            openSet.Add(startTile.Index);

            for (int i = 0; i < neighborDirs.Length; i++)
            {
                ref var neighbor = ref map.GetTile(start + neighborDirs[i]);

                if (neighbor.PassablePercent == 0)
                {
                    continue;
                }

                neighbor.SetParent(startTile);
                CalculateCost(ref neighbor);
            }

            CalculateCost(ref startTile);
        }

        private Vector2Int[] Step()
        {
            if (open.Length == 0)
            {
                return new Vector2Int[0];
            }

            current = map.GetTile(open[open.Length -1]);

            for (int i = open.Length - 2; i >= 0; i--)
            {
                ref var tile = ref map.GetTile(open[i]);
                if (tile.FCost < current.FCost)
                {
                    current = tile;
                }
                else if (tile.FCost == current.FCost && tile.HCost < current.HCost)
                {
                    current = tile;
                }
            }

            var index = -1;

            for(int i = 0; i < open.Length; i++)
            {
                if (open[i] == current.Index)
                {
                    index = i;
                    break;
                }
            }

            Assert.IsTrue(index >= 0);

            open.RemoveAt(index);
            openSet.Remove(current.Index);
            closed.Add(current.Index);
            closedSet.Add(current.Index);

            if (current.Index2 == End)
            {
                var path = CalculatePath(map.GetTile(End));

                for (int i = 0; i < open.Length; i++)
                {
                    map.GetTile(open[i]).Clear();
                }

                for (int i = 0; i < closed.Length; i++)
                {
                    map.GetTile(closed[i]).Clear();
                }

                open.Clear();
                closed.Clear();
                openSet.Clear();
                closedSet.Clear();

                return path;
            }

            for (int i = 0; i < neighborDirs.Length; i++)
            {
                ref var neighbor = ref map.GetTile(current.Index2 + neighborDirs[i]);

                if (neighbor.PassablePercent == 0 || closedSet.Contains(neighbor.Index))
                {
                    continue;
                }

                if (openSet.Contains(neighbor.Index) && current.GCost + DistanceToNeighbor(current, neighbor) >= neighbor.GCost)
                {
                    continue;
                }

                neighbor.SetParent(current);
                CalculateCost(ref neighbor);
                if (!openSet.Contains(neighbor.Index))
                {
                    open.Add(neighbor.Index);
                    openSet.Add(neighbor.Index);
                }
            }

            return null;
        }
        
        private void CalculateCost(ref Tile tile)
        {
            var gCost = CalculateGCost(tile);
            var hCost = (int)(CalculateHCost(tile) * heuristicMultiplier);
            var fCost = gCost + hCost;

            tile.SetCosts(gCost, hCost, fCost);
        }

        private int CalculateGCost(Tile tile)
        {
            if (tile.Parent == -1)
            {
                return 0;
            }

            var parent = map.GetTile(tile.Parent);

            return parent.GCost + DistanceToNeighbor(tile, parent);
        }

        private int CalculateHCost(Tile tile)
        {
            //return (int)((End - tile.Index).magnitude * 1000);

            var index = (End - tile.Index2);

            index = new Vector2Int(Mathf.Abs(index.x), Mathf.Abs(index.y));

            var min = Mathf.Min(index.x, index.y);
            var max = Mathf.Max(index.x, index.y);

            return min * 1414 + (max - min) * 1000;
        }

        private int DistanceToNeighbor(Tile tile, Tile neighbor)
        {
            //return (tile.Index - neighbor.Index).magnitude;
            var costMultiplier = (tile.PassablePercent + neighbor.PassablePercent) / 2f;

            costMultiplier /= 100;

            costMultiplier = Mathf.Clamp(costMultiplier, 0.01f, 1);

            var index = tile.Index2 - neighbor.Index2;

            var temp = (index.x != 0 ? 1 : 0) + (index.y != 0 ? 1 : 0);

            if (temp == 1)
            {
                return (int)(1000 / costMultiplier);
            }
            else if (temp == 2)
            {
                return (int)(1414 / costMultiplier);// 421356237f;
            }

            return (int)(1000 / costMultiplier);
        }

        private Vector2Int[] CalculatePath(Tile tile)
        {
            pathDirections.Clear();
            var startTile = map.GetTile(Start);
            while (tile.Index != startTile.Index)
            {
                if (tile.IsDefault())
                {
                    return new Vector2Int[0];
                }

                pathDirections.Add(tile.ParentDir);

                tile = map.GetTile(tile.Parent);
            }

            var path = new Vector2Int[pathDirections.Count + 1];

            var current = Start;
            path[0] = current;
            var pathCounter = 1;

            for (int i = pathDirections.Count - 1; i >= 0; i--)
            {
                current -= pathDirections[i];
                path[pathCounter++] = current;
            }

            pathDirections.Clear();

            return path;
        }

        private void Clear()
        {
            open.Clear();
            closed.Clear();
            openSet.Clear();
            closedSet.Clear();
            pathDirections.Clear();

            current = Tile.Default();
        }
    }
}
