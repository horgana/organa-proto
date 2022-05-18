using Unity.Collections;
using UnityEditor;
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
            CreateAsset(asset);
            //asset.Set(new NoiseProducer2D<Noise.Perlin>(Noise.NoiseProfile.Default, allocator: Allocator.Persistent));
        }
    }
}