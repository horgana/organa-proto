using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


/*[UpdateBefore(typeof(ChunkManagerSystem))]
    class TerrainManager : SystemBase
    {
        static 
        protected override void OnUpdate()
        {
            
            Entities
                .WithAll<UpdateValues>()
                .ForEach((in ChunkLoader loader) =>
                {

                });
        }
    }*/

[AddComponentMenu("Organa/Terrain")]
public class TerrainAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] int chunkSize = 64;
    [SerializeField] int lodLevels = 1;
    [SerializeField] int maxLoadVolume = 1;

    [SerializeField] List<BiomeParam<INoiseJob<float>>> biomeParams;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentObject(entity, transform);
        dstManager.AddComponentData(entity, new TerrainSettings
        {
            Root = float3.zero,
            Bounds = -1,

            LODLevels = lodLevels,
            ChunkSize = chunkSize,
            LoadVolume = maxLoadVolume
        });
        dstManager.AddComponentData(entity, new TerrainData
        {
            LoadedChunks = new UnsafeHashMap<int2, Entity>(1, Allocator.Persistent),
            //BiomeMap = new BiomeGenerator()
        });

        /*var ecb = new EntityCommandBuffer(Allocator.Temp);
        var lodGroupArchetype = dstManager.CreateArchetype(typeof(LODGroup));
        
        var buffer = dstManager.AddBuffer<LinkedEntityGroup>(entity);
        for (int i = 0; i < lodLevels; i++)
        {
            var lodGroup = ecb.CreateEntity(lodGroupArchetype);
            
            ecb.SetComponent(lodGroup, new LODGroup
            {
                RegionSize = regionSize / (1 << i),
                ChunkRenderGroups = new UnsafeHashMap<int2, Entity>(0, Allocator.Persistent)
            });
            
            
        }*/
    }
}

struct TerrainSettings : IComponentData
{
    public float3 Root;
    public int2 Bounds;

    public int LODLevels;
    public int ChunkSize;
    public int LoadVolume;
}

struct TerrainData : IComponentData
{
    public UnsafeHashMap<int2, Entity> LoadedChunks;

    //public IGenerator2D<Biome> BiomeMap; 

    public Entity GetChunkEntity(int2 index) => LoadedChunks[index];
}