using Braddss.Pathfinding.Maps;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Braddss.Pathfinding.Astars
{
    internal class AStar4 : IPathfinder
    {
        private static Vector2Int[] neighborDirs = new Vector2Int[]
        {
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.right,
        };

        private readonly List<Tile> open = new List<Tile>();
        private readonly List<Tile> closed = new List<Tile>();
        private readonly HashSet<Tile> openSet = new HashSet<Tile>();
        private readonly HashSet<Tile> closedSet = new HashSet<Tile>();

        private List<Vector2Int> pathDirections = new List<Vector2Int>();

        private Tile current = Tile.Default();

        public Vector2Int Start { get; private set; }
        public Vector2Int End { get; private set; }

        private readonly Map map;

        public IEnumerable<Tile> Open { get => open; }

        public IEnumerable<Tile> Closed { get => closed; }

        private float heuristicMultiplier;

        public AStar4(Map map, float heuristicMultiplier)
        {
            this.map = map;
            this.heuristicMultiplier = heuristicMultiplier;
        }

        public Vector2Int[] CalculatePath(Vector2Int start, Vector2Int end)
        {
            InitAStarSimple(start, end);

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
            InitAStarSimple(start, end);
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

        private void InitAStarSimple(Vector2Int start, Vector2Int end)
        {
            Clear();
            this.Start = start;
            this.End = end;

            var startTile = map.GetTile(start);
            open.Add(startTile);
            openSet.Add(startTile);

            for (int i = 0; i < neighborDirs.Length; i++)
            {
                var neighbor = map.GetTile(start + neighborDirs[i]);

                if (neighbor.PassablePercent == 0)
                {
                    continue;
                }

                neighbor.SetParent(startTile);
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

            current = open[^1];

            for (int i = open.Count - 2; i >= 0; i--)
            {
                var tile = open[i];
                if (tile.FCost < current.FCost)
                {
                    current = tile;
                }
                else if (tile.FCost == current.FCost && tile.HCost < current.HCost)
                {
                    current = tile;
                }
            }

            open.Remove(current);
            openSet.Remove(current);
            closed.Add(current);
            closedSet.Add(current);

            if (current.Index2 == End)
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
                openSet.Clear();
                closedSet.Clear();

                return path;
            }


            for (int i = 0; i < neighborDirs.Length; i++)
            {
                var neighbor = map.GetTile(current.Index2 + neighborDirs[i]);

                if (neighbor.PassablePercent == 0 || closedSet.Contains(neighbor))
                {
                    continue;
                }

                if (openSet.Contains(neighbor) && current.GCost + 1>= neighbor.GCost)
                {
                    continue;
                }

                neighbor.SetParent(current);
                CalculateCost(neighbor);
                if (!open.Contains(neighbor))
                {
                    open.Add(neighbor);
                    openSet.Add(neighbor);
                }
            }

            return null;
        }
        
        private void CalculateCost(Tile tile)
        {
            var gCost = CalculateGCost(tile);
            var hCost = (int)((Math.Abs(End.x - tile.Index2.x) + Mathf.Abs(End.y - tile.Index2.y)) * 1000 * heuristicMultiplier);
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

            var costMultiplier = (tile.PassablePercent + parent.PassablePercent) / 2f;

            costMultiplier /= 100;

            return parent.GCost + (int)(1000 / costMultiplier);
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
