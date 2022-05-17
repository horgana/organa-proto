using System;
using Organa;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Organa
{
    [Serializable]
    public struct BiomeGenerator : IGenerator2D<Biome>
    {
        UnsafeMultiHashMap<int2, Biome> Biomes;
        
        public JobHandle Schedule(NativeArray<Biome> output, float2 start, float2 dimensions, float stepSize = 1,
            int batchCount = 1, JobHandle dependency = default)
        {
            var job = new BiomeJob();
            throw new NotImplementedException();
        }

        struct BiomeJob : IJobParallelFor
        {
            public void Execute(int index)
            {
                throw new NotImplementedException();
            }
        }
    }
}