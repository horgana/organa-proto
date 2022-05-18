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

    [CreateAssetMenu(fileName = "NoiseProfile", menuName = "Organa/Noise Profile", order = 1)]
    public class NoisePreset : ScriptableObject
    {
        //public static NoisePreset Default => CreateInstance<NoisePreset>();

        public NoiseProfile profile;

        [SerializeField, Range(1, 16)] public int reloadRadius = 1;

        void Awake()
        {
            profile = NoiseProfile.Default;
        }
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
    
    public static class NoiseGroup<TIn, TOut>
    {
        public static Type[] Sources;
        public static string[] Labels;

        static NoiseGroup()
        {
            // https://makolyte.com/csharp-get-all-classes-with-a-custom-attribute/
            Sources = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsDefined(typeof(NoiseMenu))
                select type).ToArray();
            
            Labels = new string[Sources.Length];
            for (int i = 0; i < Labels.Length; i++)
                Labels[i] = ((NoiseMenu)Sources[i].GetCustomAttribute(typeof(NoiseMenu))).Label;
        }
    }
}