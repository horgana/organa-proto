using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(NoiseGenerator2D))]
    class NoiseGenerator2DEditor : UnityEditor.Editor
    {
        SerializedProperty profileProperty;
        int index = 0;

        void OnEnable()
        {
            profileProperty = serializedObject.FindProperty("profile");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var script = (NoiseGenerator2D) target;
            
            var options = NoiseMenu.NoiseGroup<float2, float>.Labels;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Noise Type");
            
            script.selectedNoise = NoiseMenu.NoiseGroup<float2, float>.Sources[EditorGUILayout.Popup(index, options)];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.PropertyField(profileProperty, new GUIContent("Noise Profile"));
            
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}