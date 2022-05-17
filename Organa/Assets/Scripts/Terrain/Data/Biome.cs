using System;
using System.Collections.Generic;

namespace Organa.Terrain
{
    [Serializable]
    public struct Biome
    {
        public List<Layer> layers;

        public Biome(int listCapacity = 0)
        {
            layers = new List<Layer>(listCapacity);
        }
    }
}