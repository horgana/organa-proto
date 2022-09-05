using System;
using UnityEngine;


[Serializable]
public struct Layer
{
    public Generator Start;
    public Generator End;

    public TerrainMaterial Material;
}