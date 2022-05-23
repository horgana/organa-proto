using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using static Unity.Mathematics.math;

public interface INoiseJob<T> where T : struct
{
    public JobHandle Schedule(NativeArray<T> output, Noise.NoiseProfile profile, float2 start, float2 dimensions, float stepSize = 1,
        int innerLoopBatchCount = 1, JobHandle dependency = default);
}

[BurstCompile]
public struct Noise
{
    public interface INoiseSource {}
    public interface INoiseSource<in TIn, out TOut> : INoiseSource
        where TIn: unmanaged 
        where TOut: unmanaged
    {
        public TOut NoiseAt(TIn p);
    }

    [Serializable]
    public class NoiseSettings : ScriptableObject
    {
        public string Name;
        public NoiseProfile Profile;
    }

    [Serializable]
    public struct NoiseProfile
    {
        public static NoiseProfile Default => new NoiseProfile
        {
            seed = 0,
            octaves = 1,
            frequency = 16f,
            amplitude = 16f,
            lacunarity = 1f,
            persistence = 0.5f
        };

        public int seed;
        [Range(1, 10)] public int octaves;
        [Range(16, 512)] public float frequency;
        [Range(1, 256)] public float amplitude;
        [Range(0.5f, 2)] public float lacunarity;
        [Range(0, 1)] public float persistence;
    }
    
    [BurstCompile, NoiseMenu("Perlin")]
    public struct Perlin : INoiseSource<float2, float>, INoiseSource<float3, float>, INoiseSource<float4, float>
    {
        public float NoiseAt(float2 p) => noise.cnoise(p);
        public float NoiseAt(float3 p) => noise.cnoise(p);
        public float NoiseAt(float4 p) => noise.cnoise(p);
    }

    [BurstCompile, NoiseMenu("Perlin Fractal")]
    public struct PerlinFractal : INoiseSource<float2, float>
    {
        public float NoiseAt(float2 p)
        {
            var depth = 10;
            var n = noise.cnoise(p);
            for (int i = 0; i < depth; i++)
            {
                var f = 2 << i;
                n += noise.cnoise(p * f) / f;
            }

            return n * (1 << depth) / ((2 << depth) - 1);
        }
    }

    [BurstCompile, NoiseMenu("Simplex")]
    public struct Simplex : INoiseSource<float2, float>
    {
        public float NoiseAt(float2 p) => noise.snoise(p);
    }

    [BurstCompile, NoiseMenu("Perlin Linear")]
    public struct PerlinLinear : INoiseSource<float2, float>
    {
        public float NoiseAt(float2 p)
        {
            float4 Pi = floor(p.xyxy) + float4(0.0f, 0.0f, 1.0f, 1.0f);
            float4 Pf = frac(p.xyxy) - float4(0.0f, 0.0f, 1.0f, 1.0f);
            Pi = mod289(Pi); // To avoid truncation effects in permutation
            float4 ix = Pi.xzxz;
            float4 iy = Pi.yyww;
            float4 fx = Pf.xzxz;
            float4 fy = Pf.yyww;

            float4 i = permute(permute(ix) + iy);

            float4 gx = frac(i * (1.0f / 41.0f)) * 2.0f - 1.0f;
            float4 gy = abs(gx) - 0.5f;
            float4 tx = floor(gx + 0.5f);
            gx = gx - tx;

            float2 g00 = float2(gx.x, gy.x);
            float2 g10 = float2(gx.y, gy.y);
            float2 g01 = float2(gx.z, gy.z);
            float2 g11 = float2(gx.w, gy.w);

            float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
            g00 *= norm.x;
            g01 *= norm.y;
            g10 *= norm.z;
            g11 *= norm.w;

            float n00 = dot(g00, float2(fx.x, fy.x));
            float n10 = dot(g10, float2(fx.y, fy.y));
            float n01 = dot(g01, float2(fx.z, fy.z));
            float n11 = dot(g11, float2(fx.w, fy.w));

            float2 fade_xy = (Pf.xy);
            float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
            float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
            return 2.3f * n_xy;
        }
    }

    [BurstCompile, NoiseMenu("Panels")]
    public struct Panels : INoiseSource<float2, float>
    {
        public float NoiseAt(float2 p)
        {
            float4 Pi = floor(p.xyxy) + float4(0.0f, 0.0f, 1.0f, 1.0f);
            float4 Pf = frac(p.xyxy) - float4(0.0f, 0.0f, 1.0f, 1.0f);
            Pi = mod289(Pi); // To avoid truncation effects in permutation
            float4 ix = Pi.xzxz;
            float4 iy = Pi.yyww;
            float4 fx = Pf.xzxz;
            float4 fy = Pf.yyww;

            float4 i = permute(permute(ix) + iy);
            
            float n00 = i.x;
            float n10 = i.y;
            float n01 = i.z;
            float n11 = i.w;

            float2 fade_xy = (Pf.xy);
            float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
            float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
            return 2.3f * n_xy /289f;
        }
    }

    // Modulo 289 without a division (only multiplications)
        static float  mod289(float x)  { return x - floor(x * (1.0f / 289.0f)) * 289.0f; }
        static float2 mod289(float2 x) { return x - floor(x * (1.0f / 289.0f)) * 289.0f; }
        static float3 mod289(float3 x) { return x - floor(x * (1.0f / 289.0f)) * 289.0f; }
        static float4 mod289(float4 x) { return x - floor(x * (1.0f / 289.0f)) * 289.0f; }

        // Modulo 7 without a division
        static float3 mod7(float3 x) { return x - floor(x * (1.0f / 7.0f)) * 7.0f; }
        static float4 mod7(float4 x) { return x - floor(x * (1.0f / 7.0f)) * 7.0f; }

        // Permutation polynomial: (34x^2 + x) math.mod 289
        static float  permute(float x)  { return mod289((34.0f * x + 1.0f) * x); }
        static float3 permute(float3 x) { return mod289((34.0f * x + 1.0f) * x); }
        static float4 permute(float4 x) { return mod289((34.0f * x + 1.0f) * x); }

        static float  taylorInvSqrt(float r)  { return 1.79284291400159f - 0.85373472095314f * r; }
        static float4 taylorInvSqrt(float4 r) { return 1.79284291400159f - 0.85373472095314f * r; }

        static float2 fade(float2 t) { return t*t*t*(t*(t*6.0f-15.0f)+10.0f); }
        static float3 fade(float3 t) { return t*t*t*(t*(t*6.0f-15.0f)+10.0f); }
        static float4 fade(float4 t) { return t*t*t*(t*(t*6.0f-15.0f)+10.0f); }

        static float4 grad4(float j, float4 ip)
        {
            float4 ones = float4(1.0f, 1.0f, 1.0f, -1.0f);
            float3 pxyz = floor(frac(float3(j) * ip.xyz) * 7.0f) * ip.z - 1.0f;
            float  pw   = 1.5f - dot(abs(pxyz), ones.xyz);
            float4 p = float4(pxyz, pw);
            float4 s = float4(p < 0.0f);
            p.xyz = p.xyz + (s.xyz*2.0f - 1.0f) * s.www;
            return p;
        }

        // Hashed 2-D gradients with an extra rotation.
        // (The constant 0.0243902439 is 1/41)
        static float2 rgrad2(float2 p, float rot)
        {
            // For more isotropic gradients, math.sin/math.cos can be used instead.
            float u = permute(permute(p.x) + p.y) * 0.0243902439f + rot; // Rotate by shift
            u = frac(u) * 6.28318530718f; // 2*pi
            return float2(cos(u), sin(u));
        }
}