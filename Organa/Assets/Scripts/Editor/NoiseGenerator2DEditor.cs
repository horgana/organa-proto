using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.PackageManager;
using UnityEditor.Rendering;
using UnityEditor.Rendering.LookDev;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Editor
{
    [CustomEditor(typeof(NoiseGenerator2D))]
    class NoiseGenerator2DEditor : DefaultEditorDrawer
    {
        SerializedProperty _choiceIndex;
        SerializedProperty _noiseProfile;

        Texture2D _previewTexture;

        void OnEnable()
        {
            _choiceIndex = serializedObject.FindProperty("choiceIndex");
            _noiseProfile = serializedObject.FindProperty("profile");

            _previewTexture = PreviewNoise(new Rect(0, 0, 100, 100), (NoiseGenerator2D) target);
            //Debug.Log(_choiceIndex.intValue);
        }
        

        public override VisualElement CreateInspectorGUI()
        {
            var noiseTypes = NoiseMenu.Source<float2, float>.NoiseTypes;
            if (noiseTypes.Count == 0)
                return new HelpBox(
                    "No types implementing INoiseSource<float2, float> were found (with a [NoiseMenu] tag).", 
                    HelpBoxMessageType.Error);

            var container = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    alignContent = Align.Stretch,
                    alignItems = Align.Stretch,
                    flexDirection = FlexDirection.Column
                }
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

            //var profileBox = new Box()
            var profile = new PropertyField(_noiseProfile, "Noise Profile") { style = {paddingTop = 10}};

            //profileBox.Add(profile);

            var preview = new Image
            {
                scaleMode = ScaleMode.ScaleAndCrop,
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.ColumnReverse,
                    minWidth = 275,
                    paddingBottom = 15,
                    paddingLeft = 5,
                    paddingRight = 15,
                    paddingTop = 15
                }
            };

            var foldout = new Foldout
            {
                style =
                {
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    backgroundColor = new Color(0, 0, 0, 0.4f),
                    flexDirection = FlexDirection.ColumnReverse
                },
            };
            
            
            foldout.Add(profile);
            preview.Add(foldout);

            profile.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                UpdatePreview(ref preview);
            });
            UpdatePreview(ref preview);
            
            container.Add(popup);
            container.Add(preview);
            container.Add(new Label("this is a test"));

            // If it works it works ¯\_(ツ)_/¯
            container.RegisterCallback<AttachToPanelEvent>(evt =>
            {
                var fs = 0;
                var element = container.parent;
                while (element != container.panel.visualTree && fs++ < 20)
                {
                    element.style.flexGrow = 1;
                    element = element.GetFirstAncestorOfType<VisualElement>();
                }
            });
            
            
            return container;
        }

        void UpdatePreview(ref Image preview)
        {
            preview.image = PreviewNoise(new Rect(0, 0, 100, 100), (NoiseGenerator2D) target);
            preview.MarkDirtyRepaint();
        }

        Texture2D PreviewNoise(Rect rect, NoiseGenerator2D generator)
        {
            if (rect.IsNullOrInverted() || rect.size == Vector2.zero) return Texture2D.redTexture;

            float2 start = 100000 * generator.profile.frequency;
            
            var noise = new NativeArray<float>((int) (rect.width * rect.height), Allocator.TempJob);
            generator.Schedule(noise, start, rect.size).Complete();

            var pixels = new Color[noise.Length];
            var min = noise.Min();
            var scale = noise.Max() - min;
            
            for (int i = 0; i < noise.Length; i++)
            {
                pixels[i] = GetColor((noise[i] - min) / scale);
            }
            var texture = new Texture2D((int) rect.width, (int) rect.height);
            texture.SetPixels(pixels);
            texture.Apply();

            noise.Dispose();

            texture.filterMode = FilterMode.Point;
            return texture;
        }

        Color GetColor(float n)
        {
            return Color.Lerp(Color.black, Color.white, n);
        }
    }
}