using System;
using System.Collections.Generic;
using UnityEngine;

namespace Braddss.Pathfinding.Astar
{
    public class AStar : IPathfinder
    {
        private static Vector2Int[] neighborDirs = new Vector2Int[]
        {
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.right,
        };

        private List<Tile> open = new List<Tile>();
        private List<Tile> closed = new List<Tile>();
        private List<Vector2Int> pathDirections = new List<Vector2Int>();

        private Tile current = null;

        private Vector2Int start;
        private Vector2Int end;

        private Map map;

        public IReadOnlyList<Tile> Open { get => open; }

        public IReadOnlyList<Tile> Closed { get => closed; }

        public AStar(Map map)
        {
            this.map = map;
        }

        public Vector2Int[] CalculatePath(Vector2Int start, Vector2Int end)
        {
            Clear();
            this.start = start;
            this.end = end;

            var startTile = map.GetTile(start);
            open.Add(startTile);

            for (int i = 0; i < neighborDirs.Length; i++)
            {
                var neighbor = map.GetTile(start + neighborDirs[i]);
                neighbor.SetParent(startTile);
                CalculateCost(neighbor);
            }

            CalculateCost(startTile);

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
            Clear();
            this.start = start;
            this.end = end;

            var startTile = map.GetTile(start);
            open.Add(startTile);

            for (int i = 0; i < neighborDirs.Length; i++)
            {
                var neighbor = map.GetTile(start + neighborDirs[i]);
                neighbor.SetParent(startTile);
                CalculateCost(neighbor);
            }

            CalculateCost(startTile);
        }

        public Vector2Int[] CalculatePathStepwise()
        {
            return Step();
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

            if (current.Index == end)
            {
                var path = CalculatePath();

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

                if (open.Contains(neighbor) && CalculateGCost(neighbor) + 1 >= neighbor.GCost)
                {
                    continue;
                }

                neighbor.SetParent(current);
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
            var hCost = Math.Abs(end.x - tile.Index.x) + Mathf.Abs(end.y - tile.Index.y);
            var fCost = gCost + hCost;

            tile.SetCosts(gCost, hCost, fCost);
        }

        private int CalculateGCost(Tile tile)
        {
            int counter = 0;
            var startTile = map.GetTile(start);

            while (tile != startTile)
            {

                //tile = tile.SetParent();
                tile = tile.Parent;

                counter++;

            }
            return counter;
        }

        private Vector2Int[] CalculatePath()
        {
            pathDirections.Clear();
            var tile = map.GetTile(end);
            var startTile = map.GetTile(start);

            while (tile != startTile)
            {
                pathDirections.Add(tile.ParentDir);

                tile = tile.Parent;
            }

            var path = new Vector2Int[pathDirections.Count + 1];

            var current = end;
            path[^1] = current;
            var dirCounter = 0;

            for(int i = pathDirections.Count - 1; i >= 0; i--)
            {
                current += pathDirections[dirCounter++];
                path[i] = current;
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
