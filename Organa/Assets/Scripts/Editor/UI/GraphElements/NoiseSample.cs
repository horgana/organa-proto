using System;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace Organa.Editor
{
    public class NoiseSample : BaseModelUIPart
    {
        public static readonly string ussClassName = "preview-part";

        public Foldout Foldout = new Foldout();
        public Image Image = new Image();
        public Slider Slider = new Slider();

        float2 position;
        
        public override VisualElement Root => Foldout;
        
        public static NoiseSample Create(string name, IGraphElementModel model, IModelUI modelUI, string parentClassName)
        {
            return model is GeneratorNode ? new NoiseSample(name, model, modelUI, parentClassName) : null;
        }
        
        protected override void BuildPartUI(VisualElement parent)
        {
            position = float2.zero;

            Image.image = (m_Model as GeneratorNode)?.Sample(position, Image.sourceRect);
            Foldout.Add(Image);
            Foldout.Add(Slider);
        }

        protected override void UpdatePartFromModel()
        {
        }

        public NoiseSample(string name, IGraphElementModel model, IModelUI ownerElement, string parentClassName) : base(name, model, ownerElement, parentClassName)
        {
        }
    }
}