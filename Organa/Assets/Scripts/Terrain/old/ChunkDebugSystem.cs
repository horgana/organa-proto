using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Organa
{
    /*[BurstCompile]
    public partial class ChunkDebugSystem : SystemBase
    {
        BeginSimulationEntityCommandBufferSystem _beginSimulationECB;
        protected override void OnCreate()
        {
            //RequireForUpdate(EntityManager.CreateEntityQuery(typeof(MapChunk)));
            base.OnCreate();

            _beginSimulationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _beginSimulationECB.CreateCommandBuffer();

            var terrain = GetSingleton<TerrainSettings>();

            Entities
                .WithAll<MapChunk>()
                .WithoutBurst()
                .ForEach((Entity e, in Chunk chunk) =>
                {
                    var scale = terrain.ChunkSize;
                    var pos = chunk.Index * scale;

                    var vertices = new Vector3[]
                    {
                        new Vector3(pos.x, 0, pos.y),
                        new Vector3(pos.x, 0, pos.y + scale),
                        new Vector3(pos.x + scale, 0, pos.y),
                        new Vector3(pos.x + scale, 0, pos.y),
                        new Vector3(pos.x, 0, pos.y + scale),
                        new Vector3(pos.x + scale, 0, pos.y + scale),
                    };
                    var triangles = new int[]
                        {0,1,2,3,4,5};

                    var mesh = GetMesh(pos, scale+1);
                    mesh.RecalculateNormals();
                    
                    var renderMesh = new RenderMesh
                    {
                        mesh = mesh,
                        material = Resources.Load("New Material", typeof(Material)) as Material,
                        layerMask = 1,
                        receiveShadows = true,
                        castShadows = ShadowCastingMode.TwoSided
                    };
                    
                    RenderMeshUtility.AddComponents(e, ecb, new RenderMeshDescription
                    {
                        RenderMesh = renderMesh,
                    });
                    ecb.AddSharedComponent(e, renderMesh);
                    ecb.AddComponent(e, new RenderBounds {Value = mesh.bounds.ToAABB()});
                    ecb.AddComponent<LocalToWorld>(e);
                    ecb.AddComponent(e, new Rotation{Value = quaternion.identity});
                    ecb.RemoveComponent<MapChunk>(e);
                }).Run();
            
            CompleteDependency();
        }
        
        #region testing
        [BurstCompile(FloatPrecision.Low, FloatMode.Fast, CompileSynchronously = true)]
        Mesh GetMesh(int2 chunk, int size)
        {
            var vertices = new Vector3[size * size * 6];
            var triangles = new int[size * size * 6];
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    float2 p = chunk + new float2(x, z);
                    var i = (x * size + z) * 6;

                    vertices[i++] = new Vector3(p.x, PerlinFractal(p/100f)*70, p.y);
                    vertices[i++] = new Vector3(p.x, PerlinFractal((p + new float2(0, 1))/100f)*70, p.y+1);
                    vertices[i++] = new Vector3(p.x+1, PerlinFractal((p + new float2(1, 0))/100f)*70, p.y);
                    vertices[i++] = new Vector3(p.x+1, PerlinFractal((p + new float2(1, 0))/100f)*70, p.y);
                    vertices[i++] = new Vector3(p.x, PerlinFractal((p + new float2(0, 1))/100f)*70, p.y+1);
                    vertices[i++] = new Vector3(p.x+1, PerlinFractal((p+1)/100f)*70, p.y+1);

                    for (int j = 6; j > 0; j--)
                    {
                        triangles[i - j] = i - j;
                    }
                }
            }

            return new Mesh
            {
                vertices = vertices,
                triangles = triangles
            };
        }

        [BurstCompile]
        public static float PerlinFractal(float2 p)
        {
            var depth = 2;
            var n = noise.cnoise(p);
            for (int i = 0; i < depth; i++)
            {
                var f = 2 << i;
                n += noise.cnoise(p * f) / f;
            }

            return n * (1 << depth) / ((2 << depth) - 1);
        }
        #endregion
    }*/
}