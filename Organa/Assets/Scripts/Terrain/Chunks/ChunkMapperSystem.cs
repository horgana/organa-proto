using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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
                .WithAll<MapChunk>()
                .ForEach((Entity entity, in Chunk chunk) =>
                {
                    var noise = new NativeArray<float>(generator.Capacity, Allocator.TempJob);

                    var chunkNoiseJob = generator.Schedule(noise, chunk.Index * chunkSize, 
                        chunkSize, 1);

                    var jobData = new MeshJobData
                    {
                        MeshData = Mesh.AllocateWritableMeshData(1)[0]
                    };

                    var meshJob = new TerrainMeshJob
                    {
                        Noise = noise,

                        MeshData = jobData.MeshData
                    };
                    
                    jobData.Dependency = meshJob.Schedule(noise.Length, 1, chunkNoiseJob);
                    GetBuffer<MeshJobData>(meshBuffers[chunk.Division].Value).Add(jobData);

                    ecb.RemoveComponent<MapChunk>(entity);
                    noise.Dispose(jobData.Dependency);

                }).Schedule();
        }

        struct TerrainMeshJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float> Noise;

            [WriteOnly] public Mesh.MeshData MeshData;

            public void Execute(int index)
            {
                
            }
        }
    }
}