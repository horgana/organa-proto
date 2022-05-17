using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Organa
{
    [AddComponentMenu("Organa/Chunk Loader")]
    [UpdateAfter(typeof(TerrainAuthoring))]
    public class ChunkLoaderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] int radius = 1;
        [SerializeField, Range(1, 10)] int lodLevels = 1;
        [SerializeField, Range(1, 2)] float unloadOffset = 1.5f;
        [SerializeField] GameObject parent;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.SetName(entity, dstManager.GetName(entity) + " (Chunk Loader)");
            dstManager.AddComponentObject(entity, transform);
            dstManager.AddComponentData(entity, new ChunkLoader
            {
                Radius = radius,
                UnloadOffset = unloadOffset
            });
            
            dstManager.AddComponent<CopyTransformFromGameObject>(entity);
            dstManager.AddComponent<UpdateTag>(entity);

            // parent to related terrain entity
            var parentTerrainEntity = conversionSystem.GetPrimaryEntity(parent);
            dstManager.AddComponentData(entity, new Parent
            {
                Value = parentTerrainEntity
            });
            dstManager.AddComponent<LocalToParent>(entity);
            dstManager.AddComponent<LocalToWorld>(entity);

            // adds entity to ChunkLoader buffer of parent terrain
            try
            { dstManager.GetBuffer<LinkedEntityGroup>(parentTerrainEntity).Add(entity); }
            
            catch (ArgumentException)
            { dstManager.AddBuffer<LinkedEntityGroup>(parentTerrainEntity).Add(entity); }
        }
    }

    struct ChunkLoader : IComponentData
    {
        public int Radius;
        public int LoadingVolume;
        public float UnloadOffset;
    }
}