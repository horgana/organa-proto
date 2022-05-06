using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Organa.Terrain
{
    public partial class TerrainMeshSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem endSimulationECB;

        List<Mesh> meshes;
        
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<TerrainData>();
            base.OnCreate();

            endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            meshes = new List<Mesh>();
        }
        
        protected override void OnUpdate()
        {
            var ecb = endSimulationECB.CreateCommandBuffer();

            var terrainData = GetSingleton<TerrainData>();
            //var mesh = new Mesh();
            //mesh.SetVertexBufferParams(0, 
                //new VertexAttributeDescriptor(VertexAttribute.Position),
                //new VertexAttributeDescriptor(VertexAttribute.Normal));
            
            World.GetOrCreateSystem<ChunkManagerSystem>().LoaderJob.Complete();
            Entities
                .WithoutBurst()
                .WithAll<UpdateMesh>()
                .WithNone<JobProgress, MapChunk>()
                .ForEach((int entityInQueryIndex, Entity entity, ref MeshStream meshStream) =>
                {
                    var mesh = new Mesh();
                    //mesh.Clear();
                    var vertices = meshStream.Vertices.ToNativeArray<float3>(Allocator.Temp);
                    var indices = meshStream.Indices.ToNativeArray<uint>(Allocator.Temp);
                    
                    mesh.SetVertexBufferParams(indices.Length, new VertexAttributeDescriptor(VertexAttribute.Position),
                        new VertexAttributeDescriptor(VertexAttribute.Normal));
                    mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
                    
                    mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
                    mesh.SetIndexBufferData(indices, 0, 0, indices.Length);
                    
                    mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length));
                    
                    mesh.RecalculateBounds();
                    meshes.Add(mesh);
                    ecb.RemoveComponent<UpdateMesh>(entity);
                }).Run();

            var material = Resources.Load<Material>("New Material");
            foreach (var mesh in meshes)
            {
                Graphics.DrawMesh(mesh, Matrix4x4.identity, material, 0);
            }

            /*Entities
                .WithoutBurst()
                .ForEach((Entity entity, in Chunk chunk, in DynamicBuffer<VertexBuffer> vertexBuffer,
                    in DynamicBuffer<IndexBuffer> indexBuffer) =>
                {
                    if (!HasComponent<JobProgress>(entity) && !HasComponent<MapChunk>(entity))
                    {
                        ecb.RemoveComponent<VertexBuffer>(entity);
                        ecb.RemoveComponent<IndexBuffer>(entity);

                        return;
                    }
                    
                    mesh.SetVertexBufferParams(vertexBuffer.Length, 
                        new VertexAttributeDescriptor(VertexAttribute.Position),
                        new VertexAttributeDescriptor(VertexAttribute.Normal));
                    mesh.SetVertexBufferData(vertexBuffer.AsNativeArray(), 0, 0, vertexBuffer.Length);
                    
                    Graphics.DrawMesh(mesh, new Vector3(chunk.Index.x*terrainData.ChunkSize, 0, chunk.Index.y*terrainData.ChunkSize), 
                        Quaternion.identity, Resources.Load<Material>("New Material"), 0);
                }).Run();*/


            /*var meshGroups = GetBuffer<LinkedEntityGroup>(GetSingletonEntity<ChunkLoader>());

            //Debug.Log(meshGroups.Length);
            Job
                .WithoutBurst()
                .WithCode(() =>
                {
                    foreach (var meshGroup in meshGroups)
                    {
                        var entity = meshGroup.Value;
                        var buffer = GetBuffer<MeshJobDataBuffer>(entity);
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
            
            CompleteDependency();*/
        }
    }
}