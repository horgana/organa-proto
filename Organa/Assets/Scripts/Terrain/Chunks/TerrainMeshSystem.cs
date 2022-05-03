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

            foreach (var entityGroup in meshGroups)
            {
                var entity = entityGroup.Value;
                var mesh = EntityManager.GetSharedComponentData<RenderMesh>(entity).mesh;

                var buffer = GetBuffer<MeshJobData>(entity);

                var vertexCount = 0;
                var indexCount = 0;
                foreach (var meshData in buffer)
                {
                    vertexCount += meshData.Vertices.Length;
                    indexCount += meshData.Indices.Length;
                }

                mesh.SetVertexBufferParams(vertexCount);
                mesh.SetIndexBufferParams(indexCount, 0);

                foreach (var meshData in buffer)
                {
                    var vertices = meshData.Vertices;
                    var indices = meshData.Indices;
                    unsafe
                    {
                        mesh.SetVertexBufferData(NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float3>(vertices.Ptr, vertices.Length, Allocator.Temp), 
                            0, mesh.vertexCount, vertices.Length);
                        mesh.SetIndices(NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(indices.Ptr, indices.Length, Allocator.Temp), 
                            MeshTopology.Triangles, 0);
                    }
                }
            }
        }
    }
}