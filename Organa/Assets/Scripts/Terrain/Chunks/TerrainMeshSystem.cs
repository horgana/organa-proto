using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Organa.Terrain
{
    public partial class TerrainMeshSystem : SystemBase
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

            //Debug.Log(meshGroups.Length);
            Job
                .WithoutBurst()
                .WithCode(() =>
                {
                    foreach (var entityGroup in meshGroups)
                    {
                        var entity = entityGroup.Value;
                        var buffer = GetBuffer<MeshJobData>(entity);
                        if (buffer.Length == 0) continue;

                        var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);
                        var mesh = renderMesh.mesh;

                        //Debug.Log(buffer.Length);
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            var meshJob = buffer[i];
                            if (!meshJob.IsCompleted) continue;
                            meshJob.Complete();

                            var vertices = meshJob.Vertices.ToNativeArray<float3>(Allocator.Temp);
                            var indices = meshJob.Indices.ToNativeArray<ushort>(Allocator.Temp);

                            //Debug.Log(vertices.Length);
                            mesh.SetVertexBufferParams(vertices.Length + mesh.vertexCount, new VertexAttributeDescriptor(VertexAttribute.Position));
                            mesh.SetIndexBufferParams(indices.Length + mesh.vertexCount, IndexFormat.UInt16);
                            
                            mesh.SetVertexBufferData(vertices, 0, mesh.vertexCount-vertices.Length, vertices.Length);
                            mesh.SetIndices(indices, 0, indices.Length, MeshTopology.Triangles, 0);
                            
                            mesh.RecalculateBounds();
                            mesh.RecalculateNormals();
                            ecb.SetComponent(entity, new RenderBounds{Value = mesh.bounds.ToAABB()});
                            //renderMesh.mesh = mesh;
                            //mesh.SetIndexBufferData(indices, 0, mesh.vertexCount-vertices.Length, indices.Length);
                            
                            
                            meshJob.Dispose();
                            buffer.RemoveAtSwapBack(i);
                            i--;
                        }

                        //renderMesh.mesh = mesh;
                        ecb.AddComponent<ChunkWorldRenderBounds>(entity);
                    }
                }).Run();
            
            CompleteDependency();
        }
    }
}