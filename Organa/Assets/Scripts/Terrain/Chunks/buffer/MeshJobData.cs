using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Organa.Terrain
{
    public struct MeshJobData : IBufferElementData
    {
        public JobHandle Dependency;
        public UnsafeList<float3> Vertices;
        public UnsafeList<int> Indices;

        public bool IsCompleted => Dependency.IsCompleted;
        public void Complete() => Dependency.Complete();

        public void Dispose(JobHandle dependency = default)
        {
            Vertices.Dispose(Dependency);
            Indices.Dispose(Dependency);
        }
    }
}