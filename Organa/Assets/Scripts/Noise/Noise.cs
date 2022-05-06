using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using static Unity.Mathematics.math;

[BurstCompile]
public static class Noise
{
    public interface INoiseMethod2D
    {
        public float NoiseAt(float2 p);
    }

    [Serializable]
    public struct NoiseProfile
    {
        public static NoiseProfile Default => new NoiseProfile()
        {
            seed = 0,
            octaves = 1,
            frequency = 16f,
            amplitude = 16f,
            lacunarity = 1f,
            persistence = 0.5f
        };

        public int seed;
        [SerializeField, Range(1, 10)]
        public int octaves;
        [SerializeField, Range(2, 1024)]
        public float frequency;
        [SerializeField, Range(1, 256)]
        public float amplitude;
        [SerializeField, Range(1, 2)]
        public float lacunarity;
        [SerializeField, Range(0, 1)]
        public float persistence;
    }
    
    [CreateAssetMenu(fileName = "NoiseProfile", menuName = "Organa/Noise Profile", order = 1)]
    public class NoisePreset : ScriptableObject
    {
        public static NoisePreset Default => CreateInstance<NoisePreset>();
        
        public NoiseProfile profile = NoiseProfile.Default;

        [SerializeField, Range(1, 16)] public int reloadRadius = 1;
    }
    
    [CustomPropertyDrawer(typeof(NoisePreset))]
    public class IngredientDrawerUIE : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var seed = new PropertyField(property.FindPropertyRelative("seed"));
            var octaves = new PropertyField(property.FindPropertyRelative("octaves"));
            var frequency = new PropertyField(property.FindPropertyRelative("frequency"));
            var amplitude = new PropertyField(property.FindPropertyRelative("amplitude"));
            var lacunarity = new PropertyField(property.FindPropertyRelative("lacunarity"));
            var persistence = new PropertyField(property.FindPropertyRelative("persistence"));

            container.Add(seed);
            container.Add(octaves);
            container.Add(frequency);
            container.Add(amplitude);
            container.Add(lacunarity);
            container.Add(persistence);

            return container;
        }
    }

    public struct Perlin : INoiseMethod2D
    {
        public float NoiseAt(float2 p)
        {
            return noise.cnoise(p);
        }
    }
}