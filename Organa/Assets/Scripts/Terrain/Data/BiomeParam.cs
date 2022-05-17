using System;
using UnityEngine;

namespace Organa
{
    [Serializable]
    public struct BiomeParam<T> where T: IGenerator2D<float>
    {
        public string name;
        public T source;
    }
}