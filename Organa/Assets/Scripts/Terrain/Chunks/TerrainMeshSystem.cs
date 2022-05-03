using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Organa.Terrain
{
    public class TerrainMeshSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem endSimulationECB;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<ChunkLoader>();
            base.OnCreate();

            endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = endSimulationECB.CreateCommandBuffer();

            World.GetOrCreateSystem<ChunkManagerSystem>().LoaderJob.Complete();

            var meshGroups = GetBuffer<LinkedEntityGroup>(GetSingletonEntity<ChunkLoader>());

            Job
                .WithoutBurst()
                .WithCode(() =>
                {
                    foreach (var entityGroup in meshGroups)
                    {
                        var entity = entityGroup.Value;
                        var mesh = EntityManager.GetSharedComponentData<RenderMesh>(entity).mesh;

                        var buffer = GetBuffer<MeshJobData>(entity);
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            var meshJob = buffer[i];
                            if (!meshJob.IsCompleted) continue;
                            meshJob.Complete();

                            var vertices = meshJob.Vertices;
                            var indices = meshJob.Indices;
                        
                            mesh.SetVertexBufferParams(vertices.Length + mesh.vertexCount);
                            mesh.SetIndexBufferParams(indices.Length + mesh.vertexCount, 0);

                            //Debug.Log(vertices.Length);
                            unsafe
                            {
                                //mesh.SetVertexBufferData(vertices.ToArray(), 0, mesh.vertexCount, vertices.Length);
                                //mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
                                mesh.SetVertexBufferData(NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float3>(vertices.Ptr, vertices.Length, Allocator.Temp), 
                                    0, mesh.vertexCount, vertices.Length);
                                mesh.SetIndices(NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(indices.Ptr, indices.Length, Allocator.Temp), 
                                    MeshTopology.Triangles, 0);
                            }
                    
                            meshJob.Dispose();
                            buffer.RemoveAtSwapBack(i);
                            i--;
                        }
                    }
                }).Run();
        }
    }
}