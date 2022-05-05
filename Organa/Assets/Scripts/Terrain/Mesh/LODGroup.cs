using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Organa.Terrain
{
    public struct LODGroup : IComponentData
    {
        public int RegionSize;
        public UnsafeHashMap<int2, Entity> ChunkRenderGroups;

        public Entity this[float2 index] => ChunkRenderGroups[(int2) index / RegionSize];
    }
}