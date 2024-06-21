using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Braddss.Pathfinding
{
    public struct Perlin
    {
        [ReadOnly]
        private NativeArray<int> p;

        [ReadOnly]
        private NativeArray<float> g3;

        [ReadOnly]
        private NativeArray<float> g2;

        [ReadOnly]
        private NativeArray<float> g1;

        private const int size = 514;

        public Perlin(int seed = 0)
        {
            p = new NativeArray<int>(514, Allocator.Persistent);
            g3 = new NativeArray<float>(514 * 3, Allocator.Persistent);
            g2 = new NativeArray<float>(514 * 2, Allocator.Persistent);
            g1 = new NativeArray<float>(514, Allocator.Persistent);

            SetSeed(seed);
        }

        private float S_curve(float t)
        {
            return t * t * (3f - 2f * t);
        }

        private float Lerp(float t, float a, float b)
        {
            return a + t * (b - a);
        }

        private void Setup(float value, out int b0, out int b1, out float r0, out float r1)
        {
            float num = value + 4096f;
            b0 = (int)num & 0xFF;
            b1 = (b0 + 1) & 0xFF;
            r0 = num - (float)(int)num;
            r1 = r0 - 1f;
        }

        private float At2(float rx, float ry, float x, float y)
        {
            return rx * x + ry * y;
        }

        private float At3(float rx, float ry, float rz, float x, float y, float z)
        {
            return rx * x + ry * y + rz * z;
        }

        public float Noise(float arg)
        {
            Setup(arg, out var b, out var b2, out var r, out var r2);
            float t = S_curve(r);
            float a = r * g1[p[b]];
            float b3 = r2 * g1[p[b2]];
            return Lerp(t, a, b3);
        }

        public float Noise(float x, float y)
        {
            Setup(x, out var b, out var b2, out var r, out var r2);
            Setup(y, out var b3, out var b4, out var r3, out var r4);
            int num = p[b];
            int num2 = p[b2];
            int num3 = p[num + b3];
            int num4 = p[num2 + b3];
            int num5 = p[num + b4];
            int num6 = p[num2 + b4];
            float t = S_curve(r);
            float t2 = S_curve(r3);
            float a = At2(r, r3, g2[num3], g2[num3 + size]);
            float b5 = At2(r2, r3, g2[num4], g2[num4 + size]);
            float a2 = Lerp(t, a, b5);
            a = At2(r, r4, g2[num5], g2[num5 + size]);
            b5 = At2(r2, r4, g2[num6], g2[num6 + size]);
            float b6 = Lerp(t, a, b5);
            return Lerp(t2, a2, b6);
        }

        public float Noise(float x, float y, float z)
        {
            Setup(x, out var b, out var b2, out var r, out var r2);
            Setup(y, out var b3, out var b4, out var r3, out var r4);
            Setup(z, out var b5, out var b6, out var r5, out var r6);
            int num = p[b];
            int num2 = p[b2];
            int num3 = p[num + b3];
            int num4 = p[num2 + b3];
            int num5 = p[num + b4];
            int num6 = p[num2 + b4];
            float t = S_curve(r);
            float t2 = S_curve(r3);
            float t3 = S_curve(r5);
            float a = At3(r, r3, r5, g3[num3 + b5], g3[num3 + b5 + size], g3[num3 + b5 + 2 * size]);
            float b7 = At3(r2, r3, r5, g3[num4 + b5], g3[num4 + b5 + size], g3[num4 + b5 + 2 * size]);
            float a2 = Lerp(t, a, b7);
            a = At3(r, r4, r5, g3[num5 + b5], g3[num5 + b5 + size], g3[num5 + b5 + 2 * size]);
            b7 = At3(r2, r4, r5, g3[num6 + b5], g3[num6 + b5 + size], g3[num6 + b5 + 2 * size]);
            float b8 = Lerp(t, a, b7);
            float a3 = Lerp(t2, a2, b8);
            a = At3(r, r3, r6, g3[num3 + b6], g3[num3 + b6 + 2 * size], g3[num3 + b6 + 2 * size]);
            b7 = At3(r2, r3, r6, g3[num4 + b6], g3[num4 + b6 + size], g3[num4 + b6 + 2 * size]);
            a2 = Lerp(t, a, b7);
            a = At3(r, r4, r6, g3[num5 + b6], g3[num5 + b6 + size], g3[num5 + b6 + 2 * size]);
            b7 = At3(r2, r4, r6, g3[num6 + b6], g3[num6 + b6 + size], g3[num6 + b6 + 2 * size]);
            b8 = Lerp(t, a, b7);
            float b9 = Lerp(t2, a2, b8);
            return Lerp(t3, a3, b9);
        }

        private float2 Normalize2(float x, float y)
        {
            float num = (float)Mathf.Sqrt(x * x + y * y);
            x /= num;
            y /= num;

            return new float2(x, y);
        }

        private float3 Normalize3(float x, float y, float z)
        {
            float num = (float)Mathf.Sqrt(x * x + y * y + z * z);
            x /= num;
            y /= num;
            z /= num;

            return new float3(x, y, z);
        }

        public void SetSeed(int seed)
        {
            var random = new Unity.Mathematics.Random((uint)seed);
            int i;
            for (i = 0; i < 256; i++)
            {
                p[i] = i;
                g1[i] = (random.NextFloat(512) - 256) / 256f;
                for (int j = 0; j < 2; j++)
                {
                    g2[i + j * size] = (random.NextFloat(512) - 256) / 256f;
                }

                float2 t2 = Normalize2(g2[i], g2[i + size]);
                g2[i] = t2.x;
                g2[i + size] = t2.y;

                for (int j = 0; j < 3; j++)
                {
                    g3[i + j * size] = (random.NextFloat(512) - 256) / 256f;
                }

                float3 t3 = Normalize3(g3[i], g3[i + size], g3[i + 2 * size]);

                g3[i] = t3.x;
                g3[i + size] = t3.y;
                g3[i + 2 * size] = t3.z;
            }

            while (--i != 0)
            {
                int num = p[i];
                int j;
                p[i] = p[j = (int)(random.NextFloat(256))];
                p[j] = num;
            }

            for (i = 0; i < 258; i++)
            {
                p[256 + i] = p[i];
                g1[256 + i] = g1[i];
                for (int j = 0; j < 2; j++)
                {
                    g2[256 + i + j * size] = g2[i + j * size];
                }

                for (int j = 0; j < 3; j++)
                {
                    g3[256 + i + j * size] = g3[i + j * size];
                }
            }
        }
    }
}
