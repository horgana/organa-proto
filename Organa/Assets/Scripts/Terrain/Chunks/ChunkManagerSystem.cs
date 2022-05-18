using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


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
            typeof(MeshStream),
            typeof(MapChunk)
        );

        _endInitializationECB = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = _endInitializationECB.CreateCommandBuffer().AsParallelWriter();

        var chunkArchetype = _chunkArchetype;

        var terrains = EntityManager.CreateEntityQuery(
            typeof(TerrainSettings)).ToEntityArray(Allocator.Temp);
        foreach (var terrainEntity in terrains)
        {
            var terrainSettings = GetComponent<TerrainSettings>(terrainEntity);

            var chunkLoaders = GetBuffer<LinkedEntityGroup>(terrainEntity).AsNativeArray().Reinterpret<Entity>();
            var loadedChunks = GetComponent<TerrainData>(terrainEntity).LoadedChunks;

            // load chunks around each chunkloader
            Entities
                .WithFilter(chunkLoaders)
                .WithoutBurst()
                .ForEach((int entityInQueryIndex, in ChunkLoader chunkLoader, in LocalToWorld localToWorld) =>
                {
                    var radius = chunkLoader.Radius;

                    var loadOrder = new NativeArray<int2>(radius * (2 * radius - 2) + 1, Allocator.Temp);
                    GenerateLoadOrder(loadOrder);


                    var loaderIndex = (int2) math.floor(localToWorld.Position.xz / terrainSettings.ChunkSize + 0.5f);
                    var length = terrainSettings.LoadVolume;
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
                            //ecb.SetSharedComponent(chunk, new RenderGroup{Filter = 0});

                            loadedChunks.Add(index, chunkEntity);
                        }
                        else if (length < loadOrder.Length - 1) length++;
                        else break;
                    }
                }).WithScheduleGranularity(ScheduleGranularity.Entity).ScheduleParallel();

            Entities.ForEach((int entityInQueryIndex, Entity entity, in Chunk chunk) =>
            {
                foreach (var loaderEntity in chunkLoaders)
                {
                    var loader = GetComponent<ChunkLoader>(loaderEntity);
                    var loaderIndex = GetComponent<LocalToWorld>(loaderEntity).Position.xz
                        / terrainSettings.ChunkSize + 0.5f;

                    if (math.distance(loaderIndex, chunk.Index) < loader.Radius * loader.UnloadOffset) return;
                }

                loadedChunks.Remove(chunk.Index);
                ecb.DestroyEntity(entityInQueryIndex, entity);
            }).Schedule();

            CompleteDependency();
            chunkLoaders.Dispose();
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
            else index = new int2(index.x + 1, (int) (-index.y + math.sign(index.x + 0.1) * math.sign(index.y + 0.1)));
        }
    }
}