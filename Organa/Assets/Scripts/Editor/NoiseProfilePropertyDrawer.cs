using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    [CustomPropertyDrawer(typeof(Noise.NoiseProfile))]
    public class NoiseProfilePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            
            container.Add(new PropertyField(property.FindPropertyRelative("frequency")));

            return container;
        }
    }
}