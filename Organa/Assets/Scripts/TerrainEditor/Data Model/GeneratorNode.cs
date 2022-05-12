using System;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.GraphToolsFoundation.Overdrive.Samples.Recipes;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Organa.Editor
{
    [Serializable]
    [SearcherItem(typeof(TerrainStencil), SearcherContext.Graph, "Generator")]
    public class GeneratorNode : NodeModel
    {
        [SerializeField] public NoiseGenerator2D<Noise.Perlin> Generator;
        
        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            
            Generator = new NoiseGenerator2D<Noise.Perlin>(Noise.NoiseProfile.Default, Allocator.Persistent);
            
            AddInputPort("Input", PortType.Data, TerrainStencil.PlaceHolder,
                options: PortModelOptions.NoEmbeddedConstant);

            //AddInputPort("Ingredients", PortType.Data, RecipeStencil.Ingredient, options: PortModelOptions.NoEmbeddedConstant);
            AddOutputPort("Result", PortType.Data, TerrainStencil.Generator, options: PortModelOptions.NoEmbeddedConstant);
        }

        public Texture2D Sample(float2 start, Rect sourceRect, float scale = 1) =>
            Sample(start, (int)sourceRect.height, (int)sourceRect.width, scale);

        public Texture2D Sample(float2 start, int height, int width, float scale = 1)
        {
            if (!(this.GetInputPorts().FirstOrDefault()?.GetConnectedEdges().FirstOrDefault()?.FromPort.NodeModel is GeneratorNode port))
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
    }
}