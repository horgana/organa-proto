using System;
using System.Collections.Generic;
using Organa;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Organa
{
    [Serializable]
    public struct BiomeGenerator : IGenerator2D<Biome>
    {
        UnsafeMultiHashMap<int2, Biome> biomes;
        
        public int resolution;
        
        public JobHandle Schedule(NativeArray<Biome> output, float2 start, float2 dimensions, float stepSize = 1,
            int batchCount = 1, JobHandle dependency = default)
        {
            var job = new BiomeJob();
            throw new NotImplementedException();
        }

        struct BiomeJob : IJobParallelFor
        {
            public int Res;
            public int2 Dim;
            
            public void Execute(int index)
            {
                var p = new float2(index % Dim.x, (int) (index / Dim.x)) * Res;

                
            }
        }
    }

    public class Test
    {
        public NativeList<BiomeGenerator> test;
    }
}