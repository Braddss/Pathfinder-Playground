using System;
using UnityEngine;

namespace Braddss.Pathfinding
{
    public struct Tile : IEquatable<Tile>
    {
        public int Parent { get; set; }

        public Vector2Int ParentDir { get; set; }

        public byte PassablePercent { get; set; }

        public int Index { get; set; }

        public Vector2Int Index2 { get; set; }

        public int GCost { get; set; }

        public int HCost { get; set; }

        public int FCost { get; set; }

        public Tile(int index, Vector2Int index2, byte passablePercent)
        {
            this.Index = index;
            this.Index2 = index2;
            this.PassablePercent = passablePercent;
            this.ParentDir = Vector2Int.zero;
            this.Parent = -1;
            this.GCost = 0;
            this.HCost = 0;
            this.FCost = 0;
        }

        public static Tile Default()
        {
            return new Tile 
            {
                Parent = -1,
                PassablePercent = 0,
                Index = -1,
            };
        }

        public readonly bool IsDefault()
        {
            return Index == -1;
        }

        public void SetParent(Tile parent)
        {
            this.Parent = parent.Index;

            this.ParentDir = parent.Index2 - this.Index2;
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
            Parent = -1;
            ParentDir = Vector2Int.zero;
        }

        public readonly bool Equals(Tile other)
        {
            return other.Index == Index;
        }
    }
}
