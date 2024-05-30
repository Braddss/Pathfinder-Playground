using UnityEngine;

namespace Braddss.Pathfinding
{
    public class Tile
    {
        private Tile[] neighbors = new Tile[4];
        public Tile Parent { get; private set; }

        public Vector2Int ParentDir { get; private set; } = Vector2Int.zero;

        public bool Passable { get; private set; }

        public Vector2Int Index { get; private set; }

        public int GCost { get; private set; }

        public int HCost { get; private set; }

        public int FCost { get; private set; }

        public int NeighborCount { get; private set; }

        public Tile(Vector2Int index, bool passable, int neighborCount)
        {
            this.Index = index;
            this.Passable = passable;
            this.NeighborCount = neighborCount;
        }

        private Tile()
        {
            this.Passable = false;
            this.NeighborCount = 1;
        }

        public static Tile OOB()
        {
            return new Tile();
        }

        public void SetParent(Tile parent)
        {
            this.Parent = parent;

            if (parent.Index == this.Index + Vector2Int.down)
            {
                ParentDir = Vector2Int.down;
            }
            else if (parent.Index == this.Index + Vector2Int.left)
            {
                ParentDir = Vector2Int.left;
            }
            else if (parent.Index == this.Index + Vector2Int.up)
            {
                ParentDir = Vector2Int.up;
            }
            else if (parent.Index == this.Index + Vector2Int.right)
            {
                ParentDir = Vector2Int.right;
            }
        }

        public void SetCosts(int gCost, int hCost, int fCost)
        {
            this.GCost = gCost;
            this.HCost = hCost;
            this.FCost = fCost;
        }

        public void Clear()
        {
            FCost = 0;
            GCost = 0;
            HCost = 0;
            Parent = null;
            ParentDir = Vector2Int.zero;
            neighbors = new Tile[4];
        }
    }
}
