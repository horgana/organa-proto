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

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentObject(entity, transform);
            dstManager.AddComponentData(entity, new TerrainSettings
            {
                Bounds = -1,
                ChunkSize = chunkSize,
                LoadedChunks = new UnsafeHashMap<int2, Entity>(1, Allocator.Persistent)
            });

            // dstManager.AddBuffer<LinkedEntityGroup>(entity);
        }
    }

    struct TerrainSettings : IComponentData
    {
        public float3 Root;
        public int2 Bounds;
        public int ChunkSize;

        public UnsafeHashMap<int2, Entity> LoadedChunks;
    } 
}

