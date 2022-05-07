using UnityEngine;

namespace Terrain.Data
{
    public struct Layer<S, E> 
        where S: struct, Noise.INoiseMethod2D 
        where E: struct, Noise.INoiseMethod2D
    {
        public NoiseGenerator2D<S> Start;
        public NoiseGenerator2D<E> End;
    }
}