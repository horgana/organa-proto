using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Organa
{
    public struct Chunk : IComponentData
    {
        public int2 Index;
        public int Division;
    }
    
    public struct ChunkMap : IComponentData
    {
        UnsafeMultiHashMap<int2, MaterialStack> LayerMap;

        public ChunkMap(int2 dim)
        {
            this = new ChunkMap(dim, Allocator.Persistent);
        }

        public ChunkMap(int2 dim, Allocator allocator)
        {
            LayerMap = new UnsafeMultiHashMap<int2, MaterialStack>(dim.x * dim.y, allocator);
        }
        
    }

    public struct MaterialStack
    {
        NativeArray<float> Nodes;

        public NativeArray<int> ExposedIndices;
    }
}