using System;
using System.CodeDom.Compiler;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Noise;

[BurstCompile]
public struct NoiseGenerator2D<N> : IDisposable where N : struct, INoiseMethod2D
{
    NoiseProfile profile;
    NativeHashMap<float2, float> map;
    N generator;

    float this[float2 p]
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

    public JobHandle Schedule(NativeArray<float> noise, float2 start, float2 dimensions, int batchCount = 1,
        JobHandle dependency = default) => new NoiseJob2D
        {
            G = this,
            Start = start,
            Dim = dimensions
        }.Schedule(noise.Length, batchCount, dependency);

    public JobHandle ScheduleBatch(NativeArray<float> noise, float2 start, float2 dimensions, int batchSize, JobHandle dependency = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose() => map.Dispose();

    public NoiseGenerator2D(NoiseProfile profile, int count, Allocator allocator)
    {
        this.profile = profile;
        map = new NativeHashMap<float2, float>(count, allocator);
        generator = new N();
    }
    
    public NoiseGenerator2D(NoiseProfile profile, Allocator allocator) { this = new NoiseGenerator2D<N>(profile, 0, allocator); }

    struct NoiseJob2D : IJobParallelFor
    {
        public NoiseGenerator2D<N> G;
        public float2 Start;
        public float2 Dim;

        [WriteOnly] public NativeArray<float> Noise;
        
        public void Execute(int index)
        {
            
        }
    }
}
