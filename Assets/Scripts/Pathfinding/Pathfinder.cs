using Braddss.Pathfinding.Assets.Scripts.Pathfinding.Dijkstras;
using Braddss.Pathfinding.Astars;
using Braddss.Pathfinding.Maps;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Braddss.Pathfinding
{
    public class Pathfinder : IPathfinder
    {
        [Serializable]
        public enum PathfindingAlogrithm
        {
            AStar,
            AStarSimple,
            Dijkstra,
        }

        private readonly IPathfinder pathfinder;

        public IReadOnlyList<Tile> Open => pathfinder.Open;

        public IReadOnlyList<Tile> Closed => pathfinder.Closed;

        public Vector2Int Start => pathfinder.Start;

        public Vector2Int End => pathfinder.End;

        public Pathfinder(Map map, PathfindingAlogrithm algo = PathfindingAlogrithm.AStar) 
        {
            switch (algo) 
            {
                case PathfindingAlogrithm.AStar:
                    pathfinder = new AStar(map);
                    break;
                case PathfindingAlogrithm.AStarSimple:
                    pathfinder = new AStarSimple(map);
                    break;
                case PathfindingAlogrithm.Dijkstra:
                    pathfinder = new Dijkstra(map);
                    break;
            }
        }

        public Vector2Int[] CalculatePath(Vector2Int start, Vector2Int end)
        {
            return pathfinder.CalculatePath(start, end);
        }

        public void InitCalculatePathStepwise(Vector2Int start, Vector2Int end)
        {
            pathfinder.InitCalculatePathStepwise(start, end);
        }

        public Vector2Int[] CalculatePathStepwise()
        {
            return pathfinder.CalculatePathStepwise();
        }

        public Vector2Int[] GetTempPath()
        {
            return pathfinder.GetTempPath();
        }
    }
}
