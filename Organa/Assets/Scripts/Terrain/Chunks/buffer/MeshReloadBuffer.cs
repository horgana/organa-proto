using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


public struct VertexBuffer : IBufferElementData
{
    public float3 Value;
}

public struct IndexBuffer : IBufferElementData
{
    public int Value;
}