using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


public struct MeshStream : IComponentData
{
    public JobHandle Dependency;

    //public UnsafeList<float3> Vertices;
    //public UnsafeList<int> Indices;
    public UnsafeStream Vertices;
    public UnsafeStream Indices;

    public bool IsCompleted => Dependency.IsCompleted;
    public void Complete() => Dependency.Complete();

    public void Dispose()
    {
        Vertices.Dispose(Dependency);
        Indices.Dispose(Dependency);
    }
}