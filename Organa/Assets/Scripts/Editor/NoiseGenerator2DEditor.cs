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

            //var profileBox = new Box()
            var profile = new PropertyField(_noiseProfile, "Noise Profile") { style = {paddingTop = 10}};

            var previewAreaBox = new Box
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    backgroundColor = Color.clear
                }
            };
            
            var previewScale = new IntegerField("Scale (m)", 1000)
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                    maxWidth = 150
                }
            };
            previewScale.labelElement.style.minWidth = 20;

            var zoomSlider = new SliderInt("- ", 10, 200) {
                value = 100,
                style =
                {
                    flexGrow = 1,
                    paddingRight = 10,
                    paddingLeft = 5
                }
            };
            zoomSlider.labelElement.style.minWidth = 0;
            var zoomLabel = new Label(zoomSlider.value + "%")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };

            zoomSlider.Add(new Label(" + "));
            zoomSlider.Add(zoomLabel);
            
            previewAreaBox.Add(previewScale);
            previewAreaBox.Add(zoomSlider);
            //profileBox.Add(profile);
            
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

            zoomSlider.RegisterValueChangedCallback(v =>
            {
                zoomLabel.text = v.newValue + "%";
                UpdatePreview(ref preview, v.newValue, 4);
            });
            zoomSlider.RegisterCallback<MouseCaptureOutEvent>(evt =>
            {
                UpdatePreview(ref preview, zoomSlider.value);
            });

            profile.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                UpdatePreview(ref preview, zoomSlider.value);
            });
            
            UpdatePreview(ref preview);
            
            container.Add(popup);
            container.Add(preview);
            container.Add(previewAreaBox);
            container.Add(new Label("this is a test"));

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

        void UpdatePreview(ref Image preview, int zoom = 100, int step = 1, FilterMode filterMode = FilterMode.Bilinear)
        {
            var rect = new Rect(0, 0, 100 / step, 100 / step);
            if (rect.IsNullOrInverted() || rect.size == Vector2.zero) return;

            var generator = (NoiseGenerator2D) target;
            
            // remove this
            var oldFreq = generator.profile.frequency;
            generator.profile.frequency *= zoom / 100f / step;
            
            float2 start = 100000 * generator.profile.frequency;
            
            var noise = new NativeArray<float>((int) (rect.width * rect.height), Allocator.TempJob);
            generator.Schedule(noise, start, rect.size).Complete();

            generator.profile.frequency = oldFreq;
            
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

            texture.filterMode = filterMode;

            preview.image = texture;
        }

        Color GetColor(float n)
        {
            return Color.Lerp(Color.black, Color.white, n);
        }
    }
}