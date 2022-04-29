using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[BurstCompile]
public static class Noise
{
    public interface INoiseMethod2D
    {
        public float NoiseAt(float2 p);
    }

    [Serializable]
    public struct NoiseProfile
    {
        public static NoiseProfile Default => new NoiseProfile()
        {
            seed = 0,
            depth = 1,
            frequency = 16f,
            lacunarity = 1f,
            persistence = 0.5f
        };
        
        public int seed;
        
        public int depth;
        
        public float frequency;
        public float lacunarity;
        public float persistence;
    }
}