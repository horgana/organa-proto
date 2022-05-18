using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;


public struct BiomeMap : IComponentData
{
    public UnsafeHashMap<float2, Biome> Map;
}