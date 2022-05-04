using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static Noise;

namespace Organa.Terrain
{
    public partial class ChunkMapperSystem : SystemBase
    {
        BeginSimulationEntityCommandBufferSystem beginSimulationECB;
        
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<ChunkLoader>();
            base.OnCreate();
            
            beginSimulationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
            var chunkSize = 64;
            generator = new NoiseGenerator2D<Perlin>(NoiseProfile.Default, 
              (chunkSize+1)*(chunkSize+1), Allocator.Persistent);
        }
        NoiseGenerator2D<Perlin> generator; 
        protected override void OnUpdate()
        {
            var ecb = beginSimulationECB.CreateCommandBuffer();

            var terrain = GetSingleton<TerrainSettings>();
            var chunkSize = terrain.ChunkSize;
            
            var meshBuffers = GetBuffer<LinkedEntityGroup>(GetSingletonEntity<ChunkLoader>());

            JobHandle noiseJobDependency = Dependency;
            Entities
                .WithoutBurst()
                .WithAll<MapChunk>()
                .ForEach((Entity entity, in Chunk chunk) =>
                {
                    var noise = new NativeArray<float>(generator.Capacity, Allocator.TempJob);

                    noiseJobDependency = JobHandle.CombineDependencies(noiseJobDependency, 
                        generator.Schedule(noise, chunk.Index * chunkSize, chunkSize, 1, dependency: noiseJobDependency));
                    
                    noiseJobDependency.Complete();
                    var jobData = new MeshJobData
                    {
                        Vertices = new UnsafeStream(noise.Length*6, Allocator.Persistent),
                        Indices = new UnsafeStream(noise.Length*6, Allocator.Persistent)
                    };

                    var meshJob = new TerrainMeshJob
                    {
                        Noise = noise,
                        
                        Dim = chunkSize,
                        
                        VertexStream = jobData.Vertices.AsWriter(),
                        IndexStream = jobData.Indices.AsWriter()
                    };

                    jobData.Dependency = meshJob.ScheduleBatch(chunkSize*chunkSize, 64, noiseJobDependency);
                    GetBuffer<MeshJobData>(meshBuffers[chunk.Division].Value).Add(jobData);
                    
                    ecb.RemoveComponent<MapChunk>(entity);
                    //noiseJobDependency.Complete();

                    noise.Dispose(jobData.Dependency);
                }).Run();
            
            CompleteDependency();
        }

        struct TerrainMeshJob : IJobParallelForBatch
        {
            [ReadOnly] public NativeArray<float> Noise;

            public int2 Dim;
            
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
                    var x =  i % Dim.x;
                    var z = i / Dim.x;
                    var offsetIndex = i + z;
                
                    VertexStream.Write(new float3(x, Noise[offsetIndex], z));
                    VertexStream.Write(new float3(x, Noise[offsetIndex+1], z+1));
                    VertexStream.Write(new float3(x+1, Noise[offsetIndex+Dim.x], z));
                    VertexStream.Write(new float3(x+1, Noise[offsetIndex+Dim.x], z));
                    VertexStream.Write(new float3(x, Noise[offsetIndex+1], z+1));
                    VertexStream.Write(new float3(x+1, Noise[offsetIndex+Dim.x+1], z+1));
                    
                    for (int j = 0; j < 6; j++) IndexStream.Write((ushort)(i*6+j));
                }
                
                VertexStream.EndForEachIndex();
                IndexStream.EndForEachIndex();
            }
        }
    }
}