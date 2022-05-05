using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Organa.Terrain
{
    public struct ChunkRenderGroup : IComponentData
    {
        public UnsafeList<float3> Vertices;
        public UnsafeList<uint> Indices;
    }
}