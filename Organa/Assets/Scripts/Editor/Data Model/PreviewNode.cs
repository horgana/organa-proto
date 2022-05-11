using System;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using TypeHandleExtensions = UnityEditor.GraphToolsFoundation.Overdrive.TypeHandleExtensions;

namespace Organa.Editor
{
    [Serializable]
    [SearcherItem(typeof(TerrainStencil), SearcherContext.Graph, "Preview")]
    public class PreviewNode : NodeModel
    {
        public Texture2D Preview(int height, int width)
        {
            if (!(this.GetInputPorts().FirstOrDefault()?.GetConnectedEdges().FirstOrDefault()?.FromPort.NodeModel is GeneratorNodeModel port))
                return Texture2D.blackTexture;

            var size = height * width;
            var generator = port.Generator;
            

            var noise = new NativeArray<float>(size, Allocator.TempJob);
            generator.Schedule(noise, float2.zero, new float2(height, width)).Complete();

            var colors = new Color[size];
            var texture = new Texture2D(width, height);

            /*for (int i = 0, y = 0; y < height; y++, i++)
            {
                for (int x = 0; x < width; x++, i++)
                {
                    texture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, noise[i]));
                }
            }*/

            for (int i = 0; i < noise.Length; i++)
            {
                colors[i] = Color.Lerp(Color.black, Color.white, noise[i] / generator.profile.amplitude);
            }
            texture.SetPixels(colors);
            texture.Apply();

            noise.Dispose();
            
            return texture;
        }
        
        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            AddInputPort("Generator", PortType.Data, TerrainStencil.Perlin,
                options: PortModelOptions.NoEmbeddedConstant);
        }
    }
}