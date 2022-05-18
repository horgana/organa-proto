using System;
using UnityEngine;


[Serializable]
public struct BiomeParam<T> where T : INoiseProcessor2D<float>
{
    public string name;
    public T source;
}