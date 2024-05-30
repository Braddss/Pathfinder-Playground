using UnityEngine;

namespace Braddss.Pathfinding
{
    public interface IPathfinder
    {
        public Vector2Int[] CalculatePath(Vector2Int start, Vector2Int end);
    }
}
