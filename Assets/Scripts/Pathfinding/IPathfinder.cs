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

        public IReadOnlyList<Tile> Open { get; }

        public IReadOnlyList<Tile> Closed { get; }

        public Vector2Int Start { get; }

        public Vector2Int End { get; }
    }
}
