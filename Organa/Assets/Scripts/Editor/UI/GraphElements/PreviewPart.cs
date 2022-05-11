using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook;
using UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook.UI;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Organa.Editor
{
    
    [GraphElementsExtensionMethodsCache(GraphElementsExtensionMethodsCacheAttribute.toolDefaultPriority)]
    public static class PreviewUIExtensions
    {
        public static IModelUI CreatePreviewUI(this ElementBuilder elementBuilder, CommandDispatcher commandDispatcher, PreviewNode model)
        {
            var ui = new NoisePreviewUI();

            ui.SetupBuildAndUpdate(model, commandDispatcher, elementBuilder.GraphView, elementBuilder.Context);
            return ui;
        }
    }

    public class NoisePreviewUI : CollapsibleInOutNode
    {
        public static readonly string previewNoisePartName = "preview-part";

        protected override void BuildPartList()
        {
            base.BuildPartList();

            PartList.AppendPart(PreviewPart.Create(previewNoisePartName, Model, this, ussClassName));
        }
    }
    

    public class PreviewPart : BaseModelUIPart
    {
        public static readonly string ussClassName = "preview-part";

        public static PreviewPart Create(string name, IGraphElementModel model, IModelUI modelUI, string parentClassName)
        {
            if (model is PreviewNode)
            {
                return new PreviewPart(name, model, modelUI, parentClassName);
            }

            return null;
        }

        public Button Button;
        public Image MeshImage;
        public Image TextureImage;
        
        protected PreviewPart(string name, IGraphElementModel model, IModelUI ownerElement, string parentClassName)
            : base(name, model, ownerElement, parentClassName)
        {
        }

        void OnPreview()
        {
            TextureImage.image =
                (m_Model as PreviewNode)?.Preview((int)TextureImage.sourceRect.height, (int)TextureImage.sourceRect.width);
            
            //TextureImage.MarkDirtyRepaint();
        }

        public override VisualElement Root { get; }

        protected override void BuildPartUI(VisualElement container)
        {
            return;
            Button = new Button() { text = "Preview" };
            Button.clicked += OnPreview;
            container.Add(Button);
            
            TextureImage = new Image();
            var texture = new Texture2D(100, 100);
            TextureImage.image = texture;

            container.Add(TextureImage);
            MeshImage = new Image();
            
            var mesh = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(0, 0, 10),
                    new Vector3(10, 0, 0),
                    new Vector3(10, 0, 10)
                },
                triangles = new[]
                {
                    0, 1, 2, 2, 1, 3
                }
            };
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            

            var material = Resources.Load<Material>("new Material");
            
            var drawRect = new Rect(0, 0, 100, 100);
            /*var preview = new PreviewRenderUtility();

            preview.BeginPreview(drawRect, GUIStyle.none);
            
            //InternalEditorUtility.SetCustomLighting(preview.lights, new Color(0.6f, 0.6f, 0.6f, 1f));

            preview.DrawMesh(mesh, Matrix4x4.identity, material, 0);
            
            preview.camera.Render();

            Image.image = preview.EndPreview();*/
            //InternalEditorUtility.RemoveCustomLighting();
            MeshImage.image = AssetPreview.GetAssetPreview(obj);
            GameObject.DestroyImmediate(obj);
            
            container.Add(MeshImage);
        }

        protected override void UpdatePartFromModel()
        {
        }
    }
}
