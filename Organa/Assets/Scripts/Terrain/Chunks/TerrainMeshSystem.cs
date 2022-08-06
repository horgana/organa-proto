using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

 // add mesh batch count property
public partial class TerrainMeshSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem endSimulationECB;

    public List<Mesh> meshes;
    public Dictionary<Chunk, Mesh> meshMap;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<TerrainSettings>();
        base.OnCreate();

        endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        meshes = new List<Mesh>();
        meshMap = new Dictionary<Chunk, Mesh>();
    }

    EntityQuery meshStreamQuery;
    
    protected override void OnUpdate()
    {
        var ecb = endSimulationECB.CreateCommandBuffer().AsParallelWriter();

        var terrainSettings = GetSingleton<TerrainSettings>();
        //var mesh = new Mesh();
        //mesh.SetVertexBufferParams(0, 
        //new VertexAttributeDescriptor(VertexAttribute.Position),
        //new VertexAttributeDescriptor(VertexAttribute.Normal));

        //World.GetOrCreateSystem<ChunkManagerSystem>().LoaderJob.Complete()

        var updatedChunks = meshStreamQuery.ToComponentDataArray<Chunk>(Allocator.Temp);

        if (updatedChunks.Length > 0)
        {
            var dataArray = Mesh.AllocateWritableMeshData(updatedChunks.Length);

            Entities
                .WithStoreEntityQueryInField(ref meshStreamQuery)
                .WithAll<UpdateMesh, Chunk>()
                .WithNone<JobProgress, MapChunk>()
                .ForEach((int entityInQueryIndex, Entity entity, ref MeshStream meshStream) =>
                {
                    var meshData = dataArray[entityInQueryIndex];
                    meshData.subMeshCount = 1;

                    var vertices = meshStream.Vertices.ToNativeArray<float3>(Allocator.Temp);
                    var indices = meshStream.Indices.ToNativeArray<uint>(Allocator.Temp);

                    meshData.SetVertexBufferParams(indices.Length,
                        new VertexAttributeDescriptor(VertexAttribute.Position),
                        new VertexAttributeDescriptor(VertexAttribute.Normal));
                    meshData.GetVertexData<float3>().CopyFrom(vertices);
                    //meshData.GetVertexData<float3>().ReinterpretStore(0, meshStream.Vertices);

                    meshData.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
                    meshData.GetIndexData<uint>().CopyFrom(indices);

                    meshData.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length));

                    //meshData.RecalculateBounds();

                    ecb.RemoveComponent<UpdateMesh>(entityInQueryIndex, entity);
                }).WithScheduleGranularity(ScheduleGranularity.Entity).ScheduleParallel();
            
            CompleteDependency();
            
            var meshBuffer = new Mesh[dataArray.Length];
            for (int i = 0; i < meshBuffer.Length; i++) meshBuffer[i] = new Mesh();
            
            Mesh.ApplyAndDisposeWritableMeshData(dataArray, meshBuffer);
            
            for (int i = 0; i < meshBuffer.Length; i++)
            {
                meshBuffer[i].RecalculateBounds();
                meshMap[updatedChunks[i]] = meshBuffer[i];
            }
            
            //meshes.AddRange(meshBuffer);
            
            //Mesh.ApplyAndDisposeWritableMeshData(dataArray, meshes);
            //dataArray.Dispose();
        }
        
        
        /*Entities
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
                ecb.RemoveComponent<UpdateMesh>(entityInQueryIndex, entity);
            }).Run();*/
        
        var material = Resources.Load<Material>("New Material");
        foreach (var mesh in meshMap.Values)
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

                var mesh = new Mesh();
                mesh.SetVertexBufferParams(vertexBuffer.Length,
                    new VertexAttributeDescriptor(VertexAttribute.Position),
                    new VertexAttributeDescriptor(VertexAttribute.Normal));
                mesh.SetVertexBufferData(vertexBuffer.AsNativeArray(), 0, 0, vertexBuffer.Length);
                mesh.SetIndexBufferParams(indexBuffer.AsNativeArray().Length, IndexFormat.UInt32);
                mesh.SetIndexBufferData(indexBuffer.AsNativeArray(), 0, 0, indexBuffer.AsNativeArray().Length);

                Graphics.DrawMesh(mesh,
                    new Vector3(chunk.Index.x * terrainSettings.ChunkSize, 0,
                        chunk.Index.y * terrainSettings.ChunkSize),
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