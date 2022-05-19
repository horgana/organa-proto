using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Editor
{
    [CustomEditor(typeof(NoiseGenerator2D))]
    class NoiseGenerator2DEditor : DefaultEditorDrawer
    {
        SerializedProperty _choiceIndex;

        void OnEnable()
        {
            _choiceIndex = serializedObject.FindProperty("choiceIndex");
            //Debug.Log(_choiceIndex.intValue);
        }

        public override VisualElement CreateInspectorGUI()
        {
            var noiseTypes = NoiseMenu.Source<float2, float>.NoiseTypes;
            if (noiseTypes.Count == 0)
                return new HelpBox(
                    "No types implementing INoiseSource<float2, float> were found (with a [NoiseMenu] tag).", 
                    HelpBoxMessageType.Error);
            
            var container = new VisualElement();

            var choiceTest = new IntegerField("choice")
            {
                bindingPath = "selectedIndex"
            };

            var popup = new PopupField<Type>("Noise Type", NoiseMenu.Source<float2, float>.NoiseTypes, _choiceIndex.intValue)
            {
                formatListItemCallback = type => ((NoiseMenu)type.GetCustomAttribute(typeof(NoiseMenu))).Label,
                formatSelectedValueCallback = type =>
                {
                    // probably a better way to do this
                    _choiceIndex.intValue = noiseTypes.IndexOf(type);
                    serializedObject.ApplyModifiedProperties();
                    return ((NoiseMenu) type.GetCustomAttribute(typeof(NoiseMenu))).Label;
                },
            };
            
            container.Add(popup);
            return container;
        }
    }
}