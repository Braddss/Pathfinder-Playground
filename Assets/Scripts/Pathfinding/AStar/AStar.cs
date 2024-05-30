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
                CalculateCost(map.GetTile(start + neighborDirs[i]));
            }

            CalculateCost(startTile);

            while (true)
            {
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
                    CalculatePath();

                    for (int i = 0;i < open.Count; i++)
                    {
                        open[i].Clear();
                    }

                    for (int i = 0; i < closed.Count; i++)
                    {
                        closed[i].Clear();
                    }

                    open.Clear();
                    closed.Clear();

                    return pathDirections.ToArray();
                }


                for (int i = 0; i < neighborDirs.Length; i++)
                {
                    var neighbor = map.GetTile(current.Index + neighborDirs[i]);

                    if (!neighbor.Passable || closed.Contains(neighbor)) 
                    {
                        continue;
                    }

                    if (open.Contains(neighbor) || CalculateGCost(neighbor) + 1 >= neighbor.GCost)
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
            }
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

        private void CalculatePath()
        {
            pathDirections.Clear();
            var tile = map.GetTile(end);
            var startTile = map.GetTile(start);

            while (tile != startTile)
            {
                pathDirections.Add(tile.ParentDir);

                tile = tile.Parent;
            }

            pathDirections.Reverse();
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