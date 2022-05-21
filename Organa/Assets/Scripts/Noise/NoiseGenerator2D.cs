using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Serialization;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using static Noise;

[Serializable]
public abstract class Generator : ScriptableObject
{
    public abstract JobHandle Schedule(NativeArray<float> output, float2 start, float2 dimensions, float stepSize = 1,
        int batchCount = 1, JobHandle dependency = default);
}

public class NoiseGenerator2D : Generator
{
    public static NoiseJob<Perlin> _test;
    public Type testType;
    [SerializeField]
    public object selectedNoise;
    [HideInInspector] public int choiceIndex = 0;
    
    public NoiseProfile profile = NoiseProfile.Default;

    void Init()
    {
        //NoiseMenu.NoiseGroup<float2, float>.Sources
    }
    
    void OnValidate()
    {
        var choice = NoiseMenu.Source<float2, float>.NoiseTypes[choiceIndex];
        if (choice == null) return;
        selectedNoise = choice;
    }

    T GetType<T>() where T: struct, INoiseSource<float2, float>
    {
        return (T) selectedNoise;
    }

    public override JobHandle Schedule(NativeArray<float> output, float2 start, float2 dimensions, float stepSize = 1,
        int batchCount = 1, JobHandle dependency = default)
    {
        var type = typeof(NoiseJob<>).MakeGenericType((Type) selectedNoise);
        var noiseJob = (INoiseJob<float>) Activator.CreateInstance(type);

        return noiseJob.Schedule(output, profile, start, dimensions, stepSize, batchCount, dependency);
    }

    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, CompileSynchronously = true)]
    public struct NoiseJob<T> : IJobFor, INoiseJob<float> where T : struct, INoiseSource<float2, float>
    { 
        public NoiseProfile Profile;
        public float2 Start;
        public float2 Dim;
        public float Step;

        static readonly T Generator = new T();

        [WriteOnly] public NativeArray<float> Noise;

        public void Execute(int index)
        {
            var p = new float2(index % Dim.x, (int) (index / Dim.x)) * Step + Start;// + 50000;

            var freq = Profile.frequency;
            float amplitude = Profile.amplitude, aSum = 0f, pSum = 0f;

            float n = 0f;
            for (int o = 0; o < Profile.octaves; o++)
            {
                var next = Generator.NoiseAt(p / freq);

                n += next * amplitude;
                //aSum += amplitude;
                pSum += math.pow(Profile.persistence, o);
                freq /= Profile.lacunarity;
                amplitude *= Profile.persistence;
            }

            Noise[index] = n / pSum; // / amplitudeSum;
        }

        public JobHandle Schedule(NativeArray<float> output, NoiseProfile profile, float2 start, float2 dimensions,
            float stepSize = 1,
            int innerLoopBatchCount = 1, JobHandle dependency = default)
            => new NoiseJob<T>
            {
                Profile = profile,
                Start = start,
                Dim = dimensions,
                Step = stepSize,

                Noise = output
            }.Schedule(output.Length, dependency);
    }
}


[BurstCompile]
[Serializable]
public struct NoiseGenerator2D<N> : IDisposable, INoiseJob<float>
    where N : struct, INoiseSource<float2, float>
{
    public int Capacity => map.Capacity;
    public int Count => map.Count();

    NoiseProfile profile;
    UnsafeHashMap<float2, float> map;
    N generator;

    public float this[float2 p]
    {
        [BurstCompile]
        get
        {
            if (!map.TryGetValue(p, out float n))
            {
                n = generator.NoiseAt(p);
                map.Add(p, n);
            }

            return n;
        }
    }

    public bool GetOrCreateValue(float2 p, out float n)
    {
        if (map.TryGetValue(p, out n)) return true;

        n = generator.NoiseAt(p);
        return false;
    }

    public struct ParallelReader
    {
        UnsafeHashMap<float2, float> map;
        N generator;
    }

    public bool TryGetValue(float2 p, out float n) => map.TryGetValue(p, out n);

    public JobHandle Schedule(NativeArray<float> noise, float2 start, float2 dimensions, float stepSize = 1,
        int batchCount = 1,
        JobHandle dependency = default)
    {
        var noiseBuffer =
            new NativeList<KeyValuePair<float2, float>>(noise.Length * profile.octaves, Allocator.TempJob);
        noiseBuffer.SetCapacity(noise.Length * profile.octaves * 2);
        var noiseJob = new NoiseJob2D
        {
            GeneratorJob = this,

            Start = start + 100000,
            Dim = dimensions,
            Step = stepSize,

            Noise = noise,
            NoiseBuffer = noiseBuffer.AsParallelWriter()
        };
        var mergeBufferJob = new MapBufferJob
        {
            Map = map.AsParallelWriter(),
            Buffer = noiseBuffer
        }.Schedule(noiseBuffer.Length, 1, noiseJob.Schedule(noise.Length, 1, dependency));

        noiseBuffer.Dispose(mergeBufferJob);
        return mergeBufferJob;
    }

    public JobHandle ScheduleBatch(NativeArray<float> noise, float2 start, float2 dimensions, int batchSize,
        JobHandle dependency = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose(JobHandle dependency) => map.Dispose(dependency);

    public void Dispose() => map.Dispose();

    public NoiseGenerator2D(NoiseProfile profile, int count = 0, Allocator allocator = Allocator.None)
    {
        this.profile = profile;
        map = new UnsafeHashMap<float2, float>(count, allocator);
        generator = new N();
    }

    struct NoiseJob2D : IJobParallelFor
    {
        [ReadOnly] public NoiseGenerator2D<N> GeneratorJob;

        public NoiseProfile Profile;
        public float2 Start;
        public float2 Dim;
        public float Step;

        [WriteOnly] public NativeArray<float> Noise;
        [WriteOnly] public NativeList<KeyValuePair<float2, float>>.ParallelWriter NoiseBuffer;

        public void Execute(int index)
        {
            var p = new float2(index % Dim.x, (int) (index / Dim.x)) * Step + Start;

            var freq = GeneratorJob.profile.frequency;
            float amplitude = GeneratorJob.profile.amplitude, aSum = 0f, pSum = 0f;

            float n = 0f;
            for (int o = 0; o < GeneratorJob.profile.octaves; o++)
            {
                if (!GeneratorJob.GetOrCreateValue(p / freq, out float next))
                    NoiseBuffer.AddNoResize(new KeyValuePair<float2, float>(p / freq, next));

                n += next * amplitude;
                //aSum += amplitude;
                pSum += math.pow(GeneratorJob.profile.persistence, o);
                freq *= GeneratorJob.profile.lacunarity;
                amplitude *= GeneratorJob.profile.persistence;
            }

            Noise[index] = n / pSum; // / amplitudeSum;
        }
    }

    unsafe struct MapBufferJob : IJobParallelFor
    {
        [WriteOnly] public UnsafeHashMap<float2, float>.ParallelWriter Map;
        [ReadOnly] public NativeList<KeyValuePair<float2, float>> Buffer;

        public void Execute(int index)
        {
            Map.TryAdd(Buffer[index].Key, Buffer[index].Value);
        }
    }

    public JobHandle Schedule(NativeArray<float> output, NoiseProfile profile, float2 start, float2 dimensions, float stepSize = 1,
        int innerLoopBatchCount = 1, JobHandle dependency = default)
    {
        throw new NotImplementedException();
    }
}