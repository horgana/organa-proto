using System;
using UnityEngine;


[Serializable]
public struct Layer
{
    public INoiseProcessor2D<float> Start;
    public INoiseProcessor2D<float> End;

    public TerrainMaterial Material;
}