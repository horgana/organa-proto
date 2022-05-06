using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Organa.Terrain
{
    public partial class ChunkManagerSystem : SystemBase
    {
        public JobHandle LoaderJob => Dependency;
        
        EndInitializationEntityCommandBufferSystem _endInitializationECB;
        
        EntityArchetype _chunkArchetype;
        EntityArchetype _renderGroupArchetype;

        protected override void OnCreate()
        {
            RequireForUpdate(EntityManager.CreateEntityQuery(typeof(ChunkLoader)));
            base.OnCreate();

            _chunkArchetype = EntityManager.CreateArchetype(
                typeof(Chunk),
                typeof(MapChunk),
                typeof(Parent),
                typeof(LocalToWorld),
                typeof(LocalToParent)
            );
            _renderGroupArchetype = EntityManager.CreateArchetype(
                typeof(ChunkRenderGroup),
                typeof(ChunkWorldRenderBounds),
                typeof(Parent),
                typeof(LocalToWorld),
                typeof(LocalToParent)
                );
            
            _endInitializationECB = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _endInitializationECB.CreateCommandBuffer().AsParallelWriter();

            var chunkArchetype = _chunkArchetype;
            var renderGroupArchetype = _renderGroupArchetype;
            
            var terrains = EntityManager.CreateEntityQuery(
                typeof(TerrainData)).ToEntityArray(Allocator.Temp);
            foreach (var terrainEntity in terrains)
            {
                var terrainData     = GetComponent<TerrainData>(terrainEntity);
                
                var chunkLoaders = GetBuffer<LinkedEntityGroup>(terrainEntity).AsNativeArray().Reinterpret<Entity>();
                var loadedChunks = terrainData.LoadedChunks;
                var loadedRenderGroups = terrainData.LoadedRenderGroups;

                var found = TryGetSingletonEntity<ChunkRenderGroup>(out Entity parent);
                Debug.Log(TryGetSingleton(out ChunkRenderGroup test));

                // load chunks around each chunkloader
                Entities
                    .WithFilter(chunkLoaders)
                    .WithoutBurst()
                    .ForEach((int entityInQueryIndex, in ChunkLoader chunkLoader, in LocalToWorld localToWorld) =>
                    {
                        var radius = chunkLoader.Radius;
                        
                        var loadOrder = new NativeArray<int2>(radius * (2 * radius - 2) + 1, Allocator.Temp);
                        GenerateLoadOrder(loadOrder);

                        var loaderRenderIndex = (int2) math.floor(localToWorld.Position.xz / terrainData.RegionSize + 0.5f);
                        var renderRadius = radius * terrainData.ChunkSize / terrainData.RegionSize + 1;
                        for (int lod = 0; lod < terrainData.LODLevels; lod++)
                        {
                            for (int i = 0; i < renderRadius * (2 * renderRadius - 2) + 1; i++)
                            {
                                var index = loadOrder[i] + loaderRenderIndex / terrainData.RegionSize;

                                if (!loadedRenderGroups.ContainsKey((index, lod)))
                                {
                                    var renderGroup = ecb.CreateEntity(entityInQueryIndex, renderGroupArchetype);
                                    //ecb.SetName(entityInQueryIndex, renderGroup, "Render Group");
                                    
                                    ecb.SetComponent(entityInQueryIndex, renderGroup, new ChunkRenderGroup(index, lod));
                                    ecb.SetComponent(entityInQueryIndex, renderGroup, new Parent
                                    {
                                        Value = terrainEntity
                                    });

                                    ecb.AddBuffer<MeshJobDataBuffer>(entityInQueryIndex, renderGroup);

                                    loadedRenderGroups.Add((index, lod), renderGroup);
                                }
                            }
                        }

                        if (!found) return;
                        var loaderIndex = (int2) math.floor(localToWorld.Position.xz / terrainData.ChunkSize + 0.5f);
                        var length = chunkLoader.LoadingVolume;
                        for (int i = 0; i < length; i++)
                        {
                            var index = loadOrder[i] + loaderIndex;

                            //var chunkEntity = Entity.Null;
                            if (!loadedChunks.ContainsKey(index))
                            {
                                var chunkEntity = ecb.CreateEntity(entityInQueryIndex, chunkArchetype);
                               // ecb.SetName(entityInQueryIndex, chunkEntity, "Terrain Chunk (" + index.x + ", " + index.y + ")");
                                var chunk = new Chunk
                                {
                                    Index = index,
                                    Division = 0
                                };
                                ecb.SetComponent(entityInQueryIndex, chunkEntity, chunk);
                                ecb.SetComponent(entityInQueryIndex, chunkEntity, new Parent
                                {
                                    Value = parent
                                });
                                //ecb.SetSharedComponent(chunk, new RenderGroup{Filter = 0});
                                
                                loadedChunks.Add(index, chunkEntity);
                            }
                            else if (length < loadOrder.Length - 1) length++;
                            else break;
                        }
                        
                    }).WithScheduleGranularity(ScheduleGranularity.Entity).ScheduleParallel();

                var renderMeshBuffer = _endInitializationECB.CreateCommandBuffer();
                Entities
                    .WithoutBurst()
                    .WithNone<RenderMesh>()
                    .WithAll<ChunkRenderGroup>()
                    .ForEach((Entity entity) =>
                    {
                       /*RenderMeshUtility.AddComponents(entity, renderMeshBuffer, new RenderMeshDescription
                        {
                            RenderMesh = new RenderMesh
                            {
                                mesh = new Mesh(),
                                material = Resources.Load<Material>("New Material"),
                                layerMask = 1
                            }
                        });*/
                        renderMeshBuffer.SetComponent(entity, new LocalToWorld
                        {
                            Value = float4x4.identity
                        });
                    }).Run();
                
                Entities.ForEach((int entityInQueryIndex, Entity entity, in Chunk chunk) =>
                {
                    var inRange = false;
                    foreach (var loaderEntity in chunkLoaders)
                    {
                        var loader = GetComponent<ChunkLoader>(loaderEntity);
                        var loaderIndex = GetComponent<LocalToWorld>(loaderEntity).Position.xz
                            / terrainData    .ChunkSize + 0.5f;

                        if (math.distance(loaderIndex, chunk.Index) < loader.Radius * loader.UnloadOffset) return;
                    }

                    loadedChunks.Remove(chunk.Index);
                    ecb.DestroyEntity(entityInQueryIndex, entity);
                }).Schedule();
                
                CompleteDependency();
            }
        }
        
        static void GenerateLoadOrder(NativeArray<int2> indices)
        {
            var index = int2.zero;
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = index;
                if (index.y == 0 && index.x >= 0) index = new int2(-index.x - 1, 0);
                else if (index.y < 0) index = new int2(index.x, -index.y);
                else index = new int2(index.x + 1, (int)(-index.y + math.sign(index.x + 0.1) * math.sign(index.y + 0.1)));
            }
        }
    } 
}

