using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Organa.Terrain
{
    [AddComponentMenu("Organa/Chunk Loader")]
    [UpdateAfter(typeof(TerrainAuthoring))]
    public class ChunkLoaderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] int radius = 1;
        [SerializeField, Range(1, 10)] int lodLevels = 1;
        [SerializeField, Range(1, 2)] float unloadOffset = 1.5f;
        [SerializeField, Range(1, 128)] int loadingVolume;
        [SerializeField] GameObject parent;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.SetName(entity, dstManager.GetName(entity) + " (Chunk Loader)");
            dstManager.AddComponentObject(entity, transform);
            dstManager.AddComponentData(entity, new ChunkLoader
            {
                Radius = radius,
                LoadingVolume = loadingVolume,
                UnloadOffset = unloadOffset
            });
            
            dstManager.AddComponent<CopyTransformFromGameObject>(entity);
            dstManager.AddComponent<UpdateTag>(entity);

            // creates dynamicbuffer of LOD entities to group mesh data
            var lodArchetype = dstManager.CreateArchetype(
                typeof(RenderMesh), 
                typeof(RenderBounds));
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            
            var buffer = ecb.AddBuffer<LinkedEntityGroup>(entity);
            for (int i = 0; i < lodLevels; i++)
            {
                var chunkGroup = ecb.CreateEntity(lodArchetype);
                var renderMesh = new RenderMesh
                {
                    mesh = new Mesh(),
                    material = Resources.Load("New Material", typeof(Material)) as Material,
                    layerMask = 1,
                };
                //ecb.SetSharedComponent(chunkGroup, renderMesh);
                RenderMeshUtility.AddComponents(chunkGroup, ecb, new RenderMeshDescription
                {
                    RenderMesh = renderMesh
                });
                ecb.AddComponent<LocalToWorld>(chunkGroup);
                ecb.AddComponent(chunkGroup, new Rotation{Value = quaternion.identity});
                ecb.AddComponent<Translation>(chunkGroup);
                buffer.Add(chunkGroup);

                ecb.AddBuffer<MeshJobDataBuffer>(chunkGroup);
            }
            
            ecb.Playback(dstManager);
            ecb.Dispose();
            
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