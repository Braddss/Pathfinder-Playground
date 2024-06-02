using Braddss.Pathfinding.Maps;
using System.Collections.Generic;
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

        private List<Tile> open = new List<Tile>();
        private List<Tile> closed = new List<Tile>();
        private List<Vector2Int> pathDirections = new List<Vector2Int>();

        private Tile current = null;

        public Vector2Int Start { get; private set; }
        public Vector2Int End { get; private set; }

        private readonly Map map;

        public IReadOnlyList<Tile> Open { get => open; }

        public IReadOnlyList<Tile> Closed { get => closed; }

        public AStar(Map map)
        {
            this.map = map;
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
            if (current == null)
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

            var startTile = map.GetTile(start);
            open.Add(startTile);

            for (int i = 0; i < neighborDirs.Length; i++)
            {
                var neighbor = map.GetTile(start + neighborDirs[i]);

                if (!neighbor.Passable)
                {
                    continue;
                }

                neighbor.SetParent(startTile, neighborDirs);
                CalculateCost(neighbor);
            }

            CalculateCost(startTile);
        }

        private Vector2Int[] Step()
        {
            if (open.Count == 0)
            {
                return new Vector2Int[0];
            }

            current = open[0];

            for (int i = 1; i < open.Count; i++)
            {
                var tile = open[i];
                if (tile.FCost < current.FCost)
                {
                    current = tile;
                }
            }

            open.Remove(current);
            closed.Add(current);

            if (current.Index == End)
            {
                var path = CalculatePath(map.GetTile(End));

                for (int i = 0; i < open.Count; i++)
                {
                    open[i].Clear();
                }

                for (int i = 0; i < closed.Count; i++)
                {
                    closed[i].Clear();
                }

                open.Clear();
                closed.Clear();

                return path;
            }

            for (int i = 0; i < neighborDirs.Length; i++)
            {
                var neighbor = map.GetTile(current.Index + neighborDirs[i]);

                if (!neighbor.Passable || closed.Contains(neighbor))
                {
                    continue;
                }

                if (open.Contains(neighbor) && current.GCost + DistanceToNeighbor(current, neighbor) >= neighbor.GCost)
                {
                    continue;
                }

                neighbor.SetParent(current, neighborDirs);
                CalculateCost(neighbor);
                if (!open.Contains(neighbor))
                {
                    open.Add(neighbor);
                }
            }

            return null;
        }
        
        private void CalculateCost(Tile tile)
        {
            var gCost = CalculateGCost(tile);
            var hCost = CalculateHCost(tile);
            var fCost = gCost + hCost;

            tile.SetCosts(gCost, hCost, fCost);
        }

        private float CalculateGCost(Tile tile)
        {
            float cost = 0;
            var startTile = map.GetTile(Start);

            while (tile != startTile)
            {
                cost += DistanceToNeighbor(tile, tile.Parent);

                tile = tile.Parent;

            }

            return cost;
        }

        private float CalculateHCost(Tile tile)
        {
            var index = (End - tile.Index);

            index = new Vector2Int(Mathf.Abs(index.x), Mathf.Abs(index.y));

            var min = Mathf.Min(index.x, index.y);
            var max = Mathf.Max(index.x, index.y);

            return min * 1.41421356237f + (max - min);
        }

        private float DistanceToNeighbor(Tile tile, Tile neighbor)
        {
            //return (tile.Index - neighbor.Index).magnitude;
            var index = tile.Index - neighbor.Index;

            var temp = (index.x != 0 ? 1 : 0) + (index.y != 0 ? 1 : 0);

            if (temp == 1)
            {
                return 1;
            }
            else if (temp == 2)
            {
                return 1.41421356237f;
            }

            return 0;
        }

        private Vector2Int[] CalculatePath(Tile tile)
        {
            pathDirections.Clear();
            var startTile = map.GetTile(Start);

            while (tile != startTile)
            {
                if (tile == null)
                {
                    return new Vector2Int[0];
                }

                pathDirections.Add(tile.ParentDir);

                tile = tile.Parent;
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
            pathDirections.Clear();

            current = null;
        }
    }
}
