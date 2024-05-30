using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

#pragma warning disable IDE1006 // Naming Styles
namespace Braddss.Pathfinding
{
    public static class VectorExtensions
    {
        private static readonly int2 s_Zero = new int2(0, 0);

        private static readonly int2 s_One = new int2(1, 1);

        private static readonly int2 s_Up = new int2(0, 1);

        private static readonly int2 s_Down = new int2(0, -1);

        private static readonly int2 s_Left = new int2(-1, 0);

        private static readonly int2 s_Right = new int2(1, 0);


        public static int2 zero
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return s_Zero;
            }
        }

        public static int2 one
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return s_One;
            }
        }

        public static int2 up
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return s_Up;
            }
        }

        public static int2 down
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return s_Down;
            }
        }

        public static int2 left
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return s_Left;
            }
        }

        public static int2 right
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return s_Right;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 ToInt2(this Vector2Int c)
        {
            return new int2(c.x, c.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2[] ToInt2Arr(this Vector2Int[] arr)
        {
            var int2Arr = new int2[arr.Length];

            for (int i = 0; i < arr.Length; i++)
            {
                int2Arr[i] = arr[i].ToInt2();
            }

            return int2Arr;
        }
    }
}
