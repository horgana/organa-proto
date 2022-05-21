using Unity.Collections;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Editor
{
    public class NoiseGeneratorMenuItems
    {
        static void CreateAsset(ScriptableObject asset)
        {
            string assetName = "Assets/NoiseGenerator.asset";

            AssetDatabase.CreateAsset(asset, assetName);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }

        [MenuItem("Assets/Create/Organa/Noise Generator", false, 1)]
        static void Default()
        {
            var asset = ScriptableObject.CreateInstance<NoiseGenerator2D>();
            asset.selectedNoise = typeof(Noise.Perlin);
            asset.choiceIndex = 0;
            CreateAsset(asset);
            //asset.Set(new NoiseProducer2D<Noise.Perlin>(Noise.NoiseProfile.Default, allocator: Allocator.Persistent));
        }

        [MenuItem("Assets/Create/Organa/Preset")]
        static void PresetTest()
        {
            var noisePreset = new Noise.NoiseSettings
            {
                Profile = Noise.NoiseProfile.Default,
                Name = "Test"
            };
            var preset = new Preset(noisePreset);
            AssetDatabase.CreateAsset(preset, "Assets/" + noisePreset.Name + ".preset");
        }
    }
}