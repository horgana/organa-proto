using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
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
                    
                    /*var jobData = new MeshJobData
                    {
                        Vertices = new UnsafeList<float3>(noise.Length*6, Allocator.TempJob),
                        Indices = new UnsafeList<int>(noise.Length*6, Allocator.TempJob)
                    };

                    var meshJob = new TerrainMeshJob
                    {
                        Noise = noise,
                        
                        Dim = chunkSize,
                        
                        Vertices = jobData.Vertices,
                        Indices = jobData.Indices
                    };

                    jobData.Dependency = meshJob.Schedule(noise.Length, 1);// chunkNoiseJob);
                    GetBuffer<MeshJobData>(meshBuffers[chunk.Division].Value).Add(jobData);*/
                    
                    ecb.RemoveComponent<MapChunk>(entity);
                    noiseJobDependency.Complete();

                    noise.Dispose(noiseJobDependency);

                }).Run();
            
            CompleteDependency();
        }

        // todo: use generator map instead of noise
        struct TerrainMeshJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float> Noise;

            public int2 Dim;
            
            [WriteOnly] public UnsafeList<float3> Vertices;
            [WriteOnly] public UnsafeList<int> Indices;

            public void Execute(int index)
            {
                float x = index % Dim.x;
                float z = index / (float) Dim.x;
                
                Vertices.Add(new float3(x, Noise[index], z));
                Vertices.Add(new float3(x, Noise[index+1], z+1));
                Vertices.Add(new float3(x+1, Noise[index+Dim.x], z));
                Vertices.Add(new float3(x+1, Noise[index+Dim.x], z));
                Vertices.Add(new float3(x, Noise[index+1], z+1));
                Vertices.Add(new float3(x+1, Noise[index+Dim.x+1], z+1));

                for (int j = 6; j > 0; j++)
                {
                    Indices.Add(Vertices.Length-j);
                }
            }
        }
    }
}