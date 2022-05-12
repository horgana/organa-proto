using System;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.Samples.Recipes;

namespace Organa.Editor
{
    [GraphElementsExtensionMethodsCache]
    public static class TerrainGraphUIFactoryExtensions
    {
        public static IModelUI CreateNode(this ElementBuilder elementBuilder, CommandDispatcher store,
            TestNodeModel model)
        {
            IModelUI ui = new TestNode();
            ui.SetupBuildAndUpdate(model, store, elementBuilder.GraphView, elementBuilder.Context);
            return ui;
        }
        /*public static IModelUI CreateNode(this ElementBuilder elementBuilder, CommandDispatcher store, MixNodeModel model)
        {
            IModelUI ui = new VariableIngredientNode();
            ui.SetupBuildAndUpdate(model, store, elementBuilder.GraphView, elementBuilder.Context);
            return ui;
        }

        public static IModelUI CreateNode(this ElementBuilder elementBuilder, CommandDispatcher store, BakeNodeModel model)
        {
            IModelUI ui = new BakeNode();
            ui.SetupBuildAndUpdate(model, store, elementBuilder.GraphView, elementBuilder.Context);
            return ui;
        }*/
    }
}