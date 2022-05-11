using System;
using Unity.Collections;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.GraphToolsFoundation.Overdrive.Samples.Recipes;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Organa.Editor
{
    [Serializable]
    [SearcherItem(typeof(TerrainStencil), SearcherContext.Graph, "Generator")]
    public class GeneratorNodeModel : NodeModel
    {
        [SerializeField] public NoiseGenerator2D<Noise.Perlin> Generator;

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            
            Generator = new NoiseGenerator2D<Noise.Perlin>(Noise.NoiseProfile.Default, Allocator.Persistent);
            
            AddInputPort("Input", PortType.Data, TerrainStencil.PlaceHolder,
                options: PortModelOptions.NoEmbeddedConstant);

            //AddInputPort("Ingredients", PortType.Data, RecipeStencil.Ingredient, options: PortModelOptions.NoEmbeddedConstant);
            AddOutputPort("Result", PortType.Data, TerrainStencil.Perlin, options: PortModelOptions.NoEmbeddedConstant);
        }
    }
}