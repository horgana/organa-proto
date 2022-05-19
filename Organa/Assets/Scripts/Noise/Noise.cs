using System;
using System.Collections.Generic;
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
    public interface INoiseSource<in TIn, out TOut>
    {
        public TOut NoiseAt(TIn p);
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
}

public class NoiseMenu : Attribute
{
    public string Label;

    public NoiseMenu(string displayName)
    {
        Label = displayName;
    }
    
    public static class Source<TIn, TOut>
    {
        // ReSharper disable once StaticMemberInGenericType
        public static List<Type> NoiseTypes;

        static Source()
        {
            // https://makolyte.com/csharp-get-all-classes-with-a-custom-attribute/
            NoiseTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsDefined(typeof(NoiseMenu)) && typeof(Noise.INoiseSource<TIn, TOut>).IsAssignableFrom(type)
                select type).ToList();
        }
    }
}