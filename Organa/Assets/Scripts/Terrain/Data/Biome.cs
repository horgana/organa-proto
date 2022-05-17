using System;
using System.Collections.Generic;

namespace Organa
{
    [Serializable]
    public struct Biome
    {
        //public List<IGenerator2D<float>> biomeParams;
        
        public List<Layer> layers;

        public Biome(int listCapacity = 0)
        {
            layers = new List<Layer>(listCapacity);
        }
    }
}