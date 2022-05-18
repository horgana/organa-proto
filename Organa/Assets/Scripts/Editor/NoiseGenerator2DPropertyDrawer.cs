using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(NoiseGenerator2D))]
    class NoiseGeneratorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Debug.Log("ran");
            EditorGUI.LabelField(position, label, new GUIContent("Test"));
        }
    }
}