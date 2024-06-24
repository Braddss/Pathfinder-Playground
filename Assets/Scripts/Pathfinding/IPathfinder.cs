using System.Collections.Generic;
using UnityEngine;

namespace Braddss.Pathfinding
{
    public interface IPathfinder
    {
        public Vector2Int[] CalculatePath(Vector2Int start, Vector2Int end);

        public void InitCalculatePathStepwise(Vector2Int start, Vector2Int end);

        public Vector2Int[] CalculatePathStepwise();

        public Vector2Int[] GetTempPath();

        public IEnumerable<Tile> Open { get; }

        public IEnumerable<Tile> Closed { get; }

        public Vector2Int Start { get; }

        public Vector2Int End { get; }
    }
}
