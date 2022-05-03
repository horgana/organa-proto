using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Organa.Terrain
{
    public struct MeshJobData : IBufferElementData
    {
        public Mesh.MeshData MeshData;
    }
}