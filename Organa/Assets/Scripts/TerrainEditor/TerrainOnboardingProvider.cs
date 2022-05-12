using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.GraphToolsFoundation.Overdrive.Samples.Recipes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Organa.Editor
{
    public class TerrainOnboardingProvider : OnboardingProvider
    {
        public override VisualElement CreateOnboardingElements(CommandDispatcher store)
        {
            var template = new GraphTemplate<TerrainStencil>(TerrainStencil.graphName);
            return AddNewGraphButton<TerrainGraphAssetModel>(template);
        }
    }
}