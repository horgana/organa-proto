using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    [CustomEditor(typeof(Object), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class DefaultEditorDrawer : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
 
            var property = serializedObject.GetIterator();
            if (property.NextVisible(true)) // Expand first child.
            {
                do
                {
                    var field = new PropertyField(property) {name = "PropertyField:" + property.propertyPath};
 
                    if (property.propertyPath == "m_Script" && serializedObject.targetObject != null)
                    {
                        field.SetEnabled(false);
                    }
 
                    root.Add(field);
                } while (property.NextVisible(false));
            }
 
            return root;
        }
    }
}