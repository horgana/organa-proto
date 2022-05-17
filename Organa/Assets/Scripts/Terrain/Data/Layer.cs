using System;
using UnityEngine;

namespace Organa
{
    [Serializable]
    public struct Layer
    {
        public IGenerator2D<> Start;
        public IGenerator2D<> End;

        public TerrainMaterial Material;
    }
}