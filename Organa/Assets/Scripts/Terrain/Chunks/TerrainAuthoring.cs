using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Organa.Terrain
{
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
        [SerializeField] int regionSize = 1024;
        [SerializeField] int lodLevels = 1;
        [SerializeField] int maxLoadVolume = 1;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentObject(entity, transform);
            dstManager.AddComponentData(entity, new TerrainData
            {
                Root = float3.zero,
                Bounds = -1,
                
                LODLevels = lodLevels,
                ChunkSize = chunkSize,
                RegionSize = regionSize,
                LoadVolume = maxLoadVolume,
                
                LoadedChunks = new UnsafeHashMap<int2, Entity>(1, Allocator.Persistent),
                LoadedRenderGroups = new UnsafeHashMap<(int2, int), Entity>(1, Allocator.Persistent)
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

    struct TerrainData : IComponentData
    {
        public float3 Root;
        public int2 Bounds;
        public int LODLevels;
        public int ChunkSize;
        public int RegionSize;
        public int LoadVolume;
        
        public UnsafeHashMap<int2, Entity> LoadedChunks;
        public UnsafeHashMap<(int2, int), Entity> LoadedRenderGroups;

        public Entity GetChunkEntity(int2 index) => LoadedChunks[index];

        public Entity GetRenderGroupEntity(Chunk chunk)
        {
            var entity = LoadedRenderGroups[((int2) math.round(chunk.Index * ChunkSize / RegionSize), chunk.Division)];
            return entity;
        }
        public Entity GetRenderGroupEntity(int2 index, int division) => 
            LoadedRenderGroups[((int2)math.round(index * ChunkSize / RegionSize), division)];
    }
}

