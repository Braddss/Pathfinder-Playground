using UnityEngine;

namespace Braddss.Pathfinding
{
    public class Tile
    {
        public Tile Parent { get; private set; }

        public Vector2Int ParentDir { get; private set; } = Vector2Int.zero;

        public bool Passable { get; set; }

        public Vector2Int Index { get; private set; }

        public int GCost { get; private set; }

        public int HCost { get; private set; }

        public int FCost { get; private set; }

        public Tile(Vector2Int index, bool passable)
        {
            this.Index = index;
            this.Passable = passable;
        }

        private Tile()
        {
            this.Passable = false;
        }

        public static Tile OOB()
        {
            return new Tile();
        }

        public void SetParent(Tile parent, Vector2Int[] directions)
        {
            this.Parent = parent;

            for (int i = 0; i < directions.Length; i++)
            {
                if (parent.Index == this.Index + directions[i])
                {
                    ParentDir = directions[i];
                    break;
                }
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
        }
    }
}
