using Unity.Mathematics;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(NoiseGenerator2D))]
    class NoiseGenerator2DEditor : UnityEditor.Editor
    {
        int index = 0;

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            var script = (NoiseGenerator2D) target;
            
            var options = NoiseMenu.NoiseGroup<float2, float>.Labels;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Noise Type");
            
            script.selectedNoise = NoiseMenu.NoiseGroup<float2, float>.Sources[EditorGUILayout.Popup(index, options)];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("profile"), new GUIContent("Noise Profile"));
            
            EditorGUILayout.EndVertical();
            
        }
    }
}