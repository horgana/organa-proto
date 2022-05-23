using System;
using System.CodeDom.Compiler;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;
using static Noise;

//[UpdateBefore(typeof(ChunkManagerSystem))]
public partial class ChunkMapperSystem : SystemBase
{
    BeginSimulationEntityCommandBufferSystem beginSimulationECB;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<ChunkLoader>();
        base.OnCreate();

        beginSimulationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        _generator = Resources.Load<NoiseGenerator2D>("Noise/Recip");
    }

    Generator _generator;

    protected override void OnUpdate()
    {
        var ecb = beginSimulationECB.CreateCommandBuffer();

        var terrainSettings = GetSingleton<TerrainSettings>();
        var chunkSize = terrainSettings.ChunkSize;

        var generator = _generator;

        if (GetEntityQuery(typeof(JobProgress)).IsEmpty)
        {
            Entities
            .WithoutBurst()
            .WithAll<MapChunk>()
            .ForEach((Entity entity, in Chunk chunk) =>
            {
                var scale = 1;
                if (GetEntityQuery(typeof(JobProgress)).CalculateEntityCount() >= terrainSettings.LoadVolume) return; 
                var noise = new NativeArray<float>((chunkSize / scale + 1) * (chunkSize / scale + 1), Allocator.Persistent);

                var noiseJob = generator.Schedule(noise, chunk.Index * chunkSize+100000, chunkSize / scale + 1, scale);

                var meshStream = new MeshStream
                {
                    Vertices = new UnsafeStream(noise.Length * 12, Allocator.Persistent),
                    Indices = new UnsafeStream(noise.Length * 6, Allocator.Persistent)
                };

                var meshJob = new TerrainMeshJob
                {
                    Noise = noise,

                    Length = chunkSize / scale,
                    Start = chunk.Index * terrainSettings.ChunkSize,
                    Scale = scale,

                    VertexStream = meshStream.Vertices.AsWriter(),
                    IndexStream = meshStream.Indices.AsWriter()
                };

                meshStream.Dependency = meshJob.ScheduleBatch((chunkSize * chunkSize) / (scale * scale), 64 / scale, noiseJob);
                ecb.SetComponent(entity, meshStream);
                if (HasComponent<JobProgress>(entity))
                {
                    GetComponent<JobProgress>(entity).Dependency.Complete();
                    ecb.SetComponent(entity, new JobProgress
                    {
                        Dependency = meshStream.Dependency,
                        MaxFrameLength = 3
                    });
                }

                ecb.AddComponent(entity, new JobProgress
                {
                    Dependency = meshStream.Dependency,
                    MaxFrameLength = -1
                });

                ecb.RemoveComponent<MapChunk>(entity);
                ecb.AddComponent<UpdateMesh>(entity);

                noise.Dispose(meshStream.Dependency);
            }).Run();
        }        
    }
    

    public struct TerrainMeshJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<float> Noise;

        public int Length;
        public int Scale;
        public bool Smooth;

        public float2 Start;

        //[WriteOnly] public UnsafeList<float3> Vertices;
        //[WriteOnly] public UnsafeList<int> Indices;
        [WriteOnly] public UnsafeStream.Writer VertexStream;
        [WriteOnly] public UnsafeStream.Writer IndexStream;

        public void Execute(int startIndex, int count)
        {
            VertexStream.BeginForEachIndex(startIndex);
            IndexStream.BeginForEachIndex(startIndex);

            for (int i = startIndex; i < (startIndex + count); i++)
            {
                var v = new float3((i % Length), 0, (int) (i / Length)) * Scale + new float3(Start.x, 0, Start.y);
                var ni = i + i / Length;
                
                var v1 = v + new float3(0, Noise[ni], 0);
                var v2 = v + new float3(0, Noise[ni + Length + 1], Scale);
                var v3 = v + new float3(Scale, Noise[ni + 1], 0);
                var n = math.cross(v2 - v1, v3 - v1);
                
                /*if (Smooth)
                {
                    var offs = (i + i / Length) * 2;
                    Noise[offs++] = v1;
                    Noise[offs++] = n;
                    if ((i + 1) % Length == 0)
                    {
                        Noise[offs++] = v2;
                        Noise[offs++] = n;
                    }
                    if ((i + 1) / Length > Length)
                    {
                        for (int j = 0; j < Length + 1; j++)
                        {
                            v1 = v + new float3(0, Noise[ni], 0);
                            v2 = v + new float3(0, Noise[ni+1], Step);
                            v3 = v + new float3(-Step, Noise[ni-Length-1], 0);
                            n = math.cross(v2 - v1, v3 - v1);
                            Noise[offs++] = v1;
                            Noise[offs++] = n;
                        }
                    }
                }*/
                VertexStream.Write(v1);
                VertexStream.Write(n);
                VertexStream.Write(v2);
                VertexStream.Write(n);
                VertexStream.Write(v3);
                VertexStream.Write(n);

                v1 = v + new float3(Scale, Noise[ni + Length + 1 + 1], Scale);
                n = math.cross(v3 - v1, v2 - v1);

                VertexStream.Write(v1);
                VertexStream.Write(n);
                VertexStream.Write(v3);
                VertexStream.Write(n);
                VertexStream.Write(v2);
                VertexStream.Write(n);

                for (int j = 0; j < 6; j++) IndexStream.Write((uint) (i * 6 + j));
            }

            VertexStream.EndForEachIndex();
            IndexStream.EndForEachIndex();
        }
    }
}