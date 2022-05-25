using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities.UniversalDelegates;
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

        int Scaled(float n) => (int) (n * n);
        
        void OnEnable()
        {
            _choiceIndex = serializedObject.FindProperty("choiceIndex");
            _noiseProfile = serializedObject.FindProperty("profile");

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

            var previewBox = new Box()
            {
                style =
                {
                    backgroundColor = Color.clear,
                    paddingLeft = 5,
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1
                }
            };
            var profile = new PropertyField(_noiseProfile, "Noise Profile") { style = {paddingTop = 10}};

            var scaleSlider = new Slider("Scale", 4, 64)
            {
                value = 8,
                style =
                {
                    paddingLeft = 40,
                    paddingRight = 30,
                    maxHeight = 20,
                    flexGrow = 0.5f,
                    flexDirection = FlexDirection.Row,
                   // alignSelf = Align.Center
                }
            };
            scaleSlider.labelElement.style.minWidth = 0;
            
            var scaleLabel = new Label(Scaled(scaleSlider.value) + "m") {style = {paddingLeft = 5}};
            scaleSlider.Add(scaleLabel);
            //profileBox.Add(profile);

            var filterMode = new EnumField("Filter Mode", FilterMode.Bilinear) {style = {paddingTop = 10}};
            
            var preview = new Image
            {
                scaleMode = ScaleMode.ScaleAndCrop,
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.ColumnReverse,
                    minWidth = 275,
                    paddingBottom = 5,
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
            previewBox.Add(preview);

            scaleSlider.RegisterValueChangedCallback(v =>
            {
                scaleLabel.text = Scaled(v.newValue) + "m";
                UpdatePreview(ref preview, Scaled(v.newValue), 4, filterMode: (FilterMode)filterMode.value);
            });
            scaleSlider.RegisterCallback<MouseCaptureOutEvent>(evt =>
            {
                UpdatePreview(ref preview, Scaled(scaleSlider.value), filterMode: (FilterMode)filterMode.value);
            });
            profile.RegisterCallback<SerializedPropertyChangeEvent>(evt => {
                UpdatePreview(ref preview, Scaled(scaleSlider.value), 4, filterMode: (FilterMode)filterMode.value); });
            profile.RegisterCallback<MouseCaptureOutEvent>(evt =>
            {
                UpdatePreview(ref preview, Scaled(scaleSlider.value), filterMode: (FilterMode)filterMode.value);
            });
            filterMode.RegisterValueChangedCallback(evt =>
            {
                UpdatePreview(ref preview, Scaled(scaleSlider.value), filterMode: (FilterMode)evt.newValue);
            });
            
            
            UpdatePreview(ref preview);
            
            container.Add(popup);
            container.Add(previewBox);
            container.Add(scaleSlider);
            container.Add(filterMode);

            // If it works it works ¯\_(ツ)_/¯
            container.RegisterCallback<AttachToPanelEvent>(evt =>
            {
                var fs = 0;
                var element = container.parent;
                while (element != container.panel.visualTree && fs++ < 20)
                {
                    element.style.flexGrow = 1;
                    element.style.flexDirection = FlexDirection.Column;
                    element = element.GetFirstAncestorOfType<VisualElement>();
                }
            });
            
            return container;
        }

        void UpdatePreview(ref Image preview, int scale = 100, int step = 1, FilterMode filterMode = FilterMode.Bilinear)
        {
            var res = 128 / step;
            var rect = new Rect(0, 0, res, res);
            if (rect.IsNullOrInverted() || rect.size == Vector2.zero) return;

            var generator = (NoiseGenerator2D) target;
            
            // remove this
            var oldFreq = generator.profile.frequency;
            generator.profile.frequency *= res / (float) scale;
            //generator.profile.frequency /= step;

            float2 start = 10000 + res * -((generator.profile.frequency-0.5f)/generator.profile.frequency) ;
            
            var noise = new NativeArray<float>((int) (rect.width * rect.height), Allocator.TempJob);
            generator.Schedule(noise, start, rect.size).Complete();

            generator.profile.frequency = oldFreq;
            
            var pixels = new Color[noise.Length];
            var min = noise.Min();
            var range = noise.Max() - min;
            
            for (int i = 0; i < noise.Length; i++)
            {
                pixels[i] = GetColor((noise[i] - min) / range);
            }
            var texture = new Texture2D((int) rect.width, (int) rect.height);
            texture.SetPixels(pixels);
            texture.Apply();

            noise.Dispose();

            texture.filterMode = filterMode;

            preview.image = texture;
        }

        Color GetColor(float n)
        {
            return Color.Lerp(Color.black, Color.white, n);
        }
    }
}