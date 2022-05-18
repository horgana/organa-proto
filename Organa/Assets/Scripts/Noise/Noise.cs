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


[Serializable]
public class NoiseGenerator : ScriptableObject
{
    public INoiseProcessor2D<float> generatorObj;

    public void Set<T>(T obj) where T : struct, INoiseProcessor2D<float>
    {
        generatorObj = obj;
    }

    public T Get<T>() where T : struct, INoiseProcessor2D<float>
    {
        return (T) generatorObj;
    }

    public void Set(INoiseProcessor2D<float> obj)
    {
        generatorObj = obj;
    }

    public object Get()
    {
        return generatorObj;
    }
}

public class TestScriptable : ScriptableObject
{
    public int i;
}

public class NoiseMenu : Attribute
{
    public static class NoiseGroup<TIn, TOut>
    {
        public static Type[] Sources;

        static NoiseGroup()
        {
            // https://makolyte.com/csharp-get-all-classes-with-a-custom-attribute/
            Sources = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsDefined(typeof(NoiseMenu))
                select type).ToArray();
        }
    }
}

public interface INoiseProcessor2D<T> where T : struct
{
    public JobHandle Schedule(NativeArray<T> output, float2 start, float2 dimensions, float stepSize = 1,
        int batchCount = 1, JobHandle dependency = default);
}

[BurstCompile]
public static class Noise
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
        [Range(2, 1024)] public float frequency;
        [Range(1, 256)] public float amplitude;
        [Range(1, 8)] public float lacunarity;
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

    [NoiseMenu]
    public struct Perlin : INoiseSource<float2, float>, INoiseSource<float3, float>, INoiseSource<float4, float>
    {
        public float NoiseAt(float2 p) => noise.cnoise(p);
        public float NoiseAt(float3 p) => noise.cnoise(p);
        public float NoiseAt(float4 p) => noise.cnoise(p);
    }
}