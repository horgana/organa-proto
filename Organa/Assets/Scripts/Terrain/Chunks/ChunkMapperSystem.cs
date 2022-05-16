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

namespace Organa.Terrain
{
    //[UpdateBefore(typeof(ChunkManagerSystem))]
    public partial class ChunkMapperSystem : SystemBase
    {
        BeginSimulationEntityCommandBufferSystem beginSimulationECB;
        
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<ChunkLoader>();
            base.OnCreate();
            
            beginSimulationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        NoiseProfile _profile;
        NoiseGenerator2D<Perlin> _generator;

        protected override void OnUpdate()
        {
            var ecb = beginSimulationECB.CreateCommandBuffer();

            NoiseProfile profile;
            try
            {
                profile = Resources.Load<NoisePreset>("Noise/Default").profile;
            }
            catch (NullReferenceException)
            {
                return;
            }
            
            var terrainSettings = GetSingleton<TerrainSettings>();
            var chunkSize = terrainSettings.ChunkSize;
            
            if (!profile.Equals(_profile))
            {
                if (_profile.Equals(null))
                    _profile = profile;
                else
                {
                    _profile = profile;
                    
                    _generator = new NoiseGenerator2D<Perlin>(profile,
                        (chunkSize + 1) * (chunkSize + 1), Allocator.Persistent);

                    World.GetOrCreateSystem<TerrainMeshSystem>().meshes.Clear();
                
                    var count = 0;
                    Entities
                        .WithNone<MapChunk>()
                        .WithNone<IndexBuffer, VertexBuffer>()
                        .ForEach((Entity entity, in MeshStream meshStream) =>
                        {
                            if (count >= terrainSettings.LoadVolume) return;
                            //count++;
                            /*ecb.AddBuffer<VertexBuffer>(entity)
                                .CopyFrom(meshStream.Vertices.ToNativeArray<VertexBuffer>(Allocator.Temp));
                            ecb.AddBuffer<IndexBuffer>(entity)
                                .CopyFrom(meshStream.Indices.ToNativeArray<IndexBuffer>(Allocator.Temp));*/
                            ecb.AddComponent<MapChunk>(entity);
                            //ecb.AddComponent<UpdateMesh>(entity);
                        }).Run();
                }
            }

            var generator = _generator;
            
            Entities
                .WithoutBurst()
                .WithAll<MapChunk>()
                .ForEach((Entity entity, in Chunk chunk) =>
                {
                    //if (GetEntityQuery(typeof(JobProgress)).CalculateEntityCount() >= terrainData.LoadVolume) return; 
                    var noise = new NativeArray<float>((chunkSize + 1) * (chunkSize + 1), Allocator.Persistent);

                    var noiseJob = generator.Schedule(noise, chunk.Index * chunkSize, chunkSize+1, 1);

                    var meshStream = new MeshStream
                    {
                        Vertices = new UnsafeStream(noise.Length * 12, Allocator.Persistent),
                        Indices = new UnsafeStream(noise.Length * 6, Allocator.Persistent)
                    };

                    var meshJob = new TerrainMeshJob
                    {
                        Noise = noise,

                        Length = chunkSize,
                        Start = chunk.Index*terrainSettings.ChunkSize,

                        VertexStream = meshStream.Vertices.AsWriter(),
                        IndexStream = meshStream.Indices.AsWriter()
                    };

                    meshStream.Dependency = meshJob.ScheduleBatch(chunkSize * chunkSize, 64, noiseJob);
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
                        MaxFrameLength = 3
                    });
                    
                    ecb.RemoveComponent<MapChunk>(entity);
                    ecb.AddComponent<UpdateMesh>(entity);

                    noise.Dispose(meshStream.Dependency);
                }).Run();
        }

        public struct TerrainMeshJob : IJobParallelForBatch
        {
            [ReadOnly] public NativeArray<float> Noise;

            public int Length;

            public float2 Start;
            //[WriteOnly] public UnsafeList<float3> Vertices;
            //[WriteOnly] public UnsafeList<int> Indices;
            [WriteOnly] public UnsafeStream.Writer VertexStream;
            [WriteOnly] public UnsafeStream.Writer IndexStream;

            public void Execute(int startIndex, int count)
            {
                VertexStream.BeginForEachIndex(startIndex);
                IndexStream.BeginForEachIndex(startIndex);

                for (int i = startIndex; i < startIndex + count; i++)
                {
                    var v = new float3((i % Length), 0, (int)(i / Length)) + new float3(Start.x, 0, Start.y);
                    var ni = i + i / Length;
            
                    var v1 = v + new float3(0, Noise[ni], 0);
                    var v2 = v + new float3(0, Noise[ni+Length+1], 1);
                    var v3 = v + new float3(1, Noise[ni+1], 0);
                    var n = math.cross(v2 - v1, v3 - v1);
                    
                    VertexStream.Write(v1);
                    VertexStream.Write(n);
                    VertexStream.Write(v2);
                    VertexStream.Write(n);
                    VertexStream.Write(v3);
                    VertexStream.Write(n);
                    
                    v1 = v + new float3(1, Noise[ni+Length+1+1], 1);
                    n = math.cross(v3 - v1, v2 - v1);
                    
                    VertexStream.Write(v1);
                    VertexStream.Write(n);
                    VertexStream.Write(v3);
                    VertexStream.Write(n);
                    VertexStream.Write(v2);
                    VertexStream.Write(n);
                    
                    for (int j = 0; j < 6; j++) IndexStream.Write((uint)(i*6+j));
                }
                
                VertexStream.EndForEachIndex();
                IndexStream.EndForEachIndex();
            }
        }
    }
}