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
        BeginSimulationEntityCommandBufferSystem _commandBuffer;
        
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<ChunkLoader>();
            base.OnCreate();
            
            _commandBuffer = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _commandBuffer.CreateCommandBuffer();

            var terrain = GetSingleton<TerrainSettings>();
            var chunkSize = terrain.ChunkSize;
            
            var generator = new NoiseGenerator2D<Perlin>(NoiseProfile.Default, 
                (chunkSize+1)*(chunkSize+1), Allocator.TempJob);
            
            var meshBuffers = GetBuffer<LinkedEntityGroup>(GetSingletonEntity<ChunkLoader>());

            Entities
                .WithoutBurst()
                .WithAll<MapChunk>()
                .ForEach((Entity entity, in Chunk chunk) =>
                {
                    var noise = new NativeArray<float>(generator.Capacity, Allocator.TempJob);

                    var chunkNoiseJob = generator.Schedule(noise, chunk.Index * chunkSize, 
                        chunkSize, 1);

                    var jobData = new MeshJobData
                    {
                        Vertices = new UnsafeList<float3>(chunkSize * chunkSize * 6, Allocator.TempJob),
                        Indices = new UnsafeList<int>(chunkSize * chunkSize * 6, Allocator.TempJob)
                    };

                    var meshJob = new TerrainMeshJob
                    {
                        Noise = noise,
                        
                        Dim = chunkSize,
                        
                        Vertices = jobData.Vertices,
                        Indices = jobData.Indices
                    };
                    
                    jobData.Dependency = meshJob.Schedule(noise.Length, 1, chunkNoiseJob);
                    GetBuffer<MeshJobData>(meshBuffers[chunk.Division].Value).Add(jobData);

                    ecb.RemoveComponent<MapChunk>(entity);
                    noise.Dispose(jobData.Dependency);

                }).Run();
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