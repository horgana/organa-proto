using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Organa
{
    public struct BiomeMap : IComponentData
    {
        public UnsafeHashMap<float2, Biome> Map;
        
    }
}