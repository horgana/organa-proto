using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook.UI
{
    public class PrintResultPart : BaseModelUIPart
    {
        public static readonly string ussClassName = "print-result-part";

        public static PrintResultPart Create(string name, IGraphElementModel model, IModelUI modelUI, string parentClassName)
        {
            if (model is MathResult)
            {
                return new PrintResultPart(name, model, modelUI, parentClassName);
            }

            return null;
        }

        public Button Button { get; private set; }
        public Image Image;

        public override VisualElement Root => Button;

        protected PrintResultPart(string name, IGraphElementModel model, IModelUI ownerElement, string parentClassName)
            : base(name, model, ownerElement, parentClassName)
        {
        }

        void OnPrintResult()
        {
            float result = (m_Model as MathResult)?.Evaluate() ?? 0.0f;

            Debug.Log($"Result is {result}");
        }

        protected override void BuildPartUI(VisualElement container)
        {
            Button = new Button() { text = "Print Result" };
            Button.clicked += OnPrintResult;
            container.Add(Button);

            Image = new Image();
            
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
            Image.image = AssetPreview.GetAssetPreview(obj);

            container.Add(Image);
        }

        protected override void UpdatePartFromModel()
        {
        }
    }
}
