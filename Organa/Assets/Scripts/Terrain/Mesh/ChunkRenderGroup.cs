using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Organa.Terrain
{
    public struct ChunkRenderGroup : IComponentData
    {
        public int2 Index;
        public int Division;
        
        public UnsafeList<float3> Vertices;
        public UnsafeList<uint> Indices;

        public ChunkRenderGroup(int2 index, int division, int bufferSize = 0)
        {
            Index = index;
            Division = division;
            
            Vertices = new UnsafeList<float3>(bufferSize, Allocator.Persistent);
            Indices = new UnsafeList<uint>(bufferSize, Allocator.Persistent);
        }
    }
}