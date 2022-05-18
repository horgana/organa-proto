using System;
using UnityEngine;


[Serializable]
public struct BiomeParam<T> where T : INoiseJob<float>
{
    public string name;
    public T source;
}