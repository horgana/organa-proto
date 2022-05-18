using System;
using UnityEngine;


[Serializable]
public struct Layer
{
    public INoiseJob<float> Start;
    public INoiseJob<float> End;

    public TerrainMaterial Material;
}