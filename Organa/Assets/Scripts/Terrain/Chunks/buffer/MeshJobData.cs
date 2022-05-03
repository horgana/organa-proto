using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Organa.Terrain
{
    public struct MeshJobData : IBufferElementData
    {
        public JobHandle Dependency;

        public Mesh.MeshData MeshData;
    }
}