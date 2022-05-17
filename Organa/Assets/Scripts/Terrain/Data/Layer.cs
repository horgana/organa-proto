using System;
using UnityEngine;

namespace Organa.Terrain
{
    [Serializable]
    public struct Layer
    {
        public INoiseGenerator Start;
        public INoiseGenerator End;

        public TerrainMaterial Material;
    }
}