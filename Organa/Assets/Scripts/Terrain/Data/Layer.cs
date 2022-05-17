using System;
using UnityEngine;

namespace Organa
{
    [Serializable]
    public struct Layer
    {
        public IGenerator2D<float> Start;
        public IGenerator2D<float> End;

        public TerrainMaterial Material;
    }
}