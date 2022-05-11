using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Noise;

[BurstCompile]
[Serializable]
public struct NoiseGenerator2D<N> : IDisposable where N : struct, INoiseMethod2D
{
    public int Capacity => map.Capacity;
    public int Count => map.Count();
    
    [SerializeField]
    public NoiseProfile profile;
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
            Generator = this,

            Start = start+100000,
            Dim = dimensions,
            Step = stepSize,

            Noise = noise,
            NoiseBuffer = noiseBuffer.AsParallelWriter()
        };
        var mergeBufferJob = new MapBufferJob
        {
            Map = map.AsParallelWriter(),
            Buffer = noiseBuffer
        }.Schedule(noiseBuffer.Length, 1,  noiseJob.Schedule(noise.Length, 1, dependency));
        
        noiseBuffer.Dispose(mergeBufferJob);
        return mergeBufferJob;
    } 

    public JobHandle ScheduleBatch(NativeArray<float> noise, float2 start, float2 dimensions, int batchSize, JobHandle dependency = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose(JobHandle dependency) => map.Dispose(dependency);

    public void Dispose() => map.Dispose();

    public NoiseGenerator2D(NoiseProfile profile, int count, Allocator allocator)
    {
        this.profile = profile;
        map = new UnsafeHashMap<float2, float>(count, allocator);
        generator = new N();
    }
    
    public NoiseGenerator2D(NoiseProfile profile, Allocator allocator) { this = new NoiseGenerator2D<N>(profile, 0, allocator); }
    
    struct NoiseJob2D : IJobParallelFor
    {
        [ReadOnly] public NoiseGenerator2D<N> Generator;

        public NoiseProfile Profile;
        public float2 Start;
        public float2 Dim;
        public float Step;

        [WriteOnly] public NativeArray<float> Noise;
        [WriteOnly] public NativeList<KeyValuePair<float2, float>>.ParallelWriter NoiseBuffer;
        
        public void Execute(int index)
        {
            var p = new float2(index % Dim.x, (int)(index / Dim.x)) * Step + Start;

            var freq = Generator.profile.frequency;
            float amplitude = Generator.profile.amplitude, amplitudeSum = 0f;

            float n = 0f;
            for (int o = 0; o < Generator.profile.octaves; o++)
            {
                if (!Generator.GetOrCreateValue(p / freq, out float next))
                    NoiseBuffer.AddNoResize(new KeyValuePair<float2, float>(p / freq, next));

                n += next * amplitude;
                amplitudeSum += amplitude;
                freq *= Generator.profile.lacunarity;
                amplitude *= Generator.profile.persistence;
            }

            Noise[index] = n;// / amplitudeSum;
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
}
