using System;
using UnityEngine;

namespace Organa
{
    [Serializable]
    public struct Layer
    {
        public INoiseProcessor2D<float> Start;
        public INoiseProcessor2D<float> End;

        public TerrainMaterial Material;
    }
}