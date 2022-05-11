using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Organa.Editor
{
    public class NoiseSampleUI : CollapsibleInOutNode
    {
        public static readonly string noiseSamplePartName = "noise-part";

        protected override void BuildPartList()
        {
            base.BuildPartList();
            
            if (Model is GeneratorNode)
                PartList.AppendPart(new NoiseSample(noiseSamplePartName, Model, this, ussClassName));
        }
    }
    
    [GraphElementsExtensionMethodsCache]
    public static class NoiseSampleExtensions
    {
        public static IModelUI CreateNoiseSampleUI(this ElementBuilder elementBuilder, CommandDispatcher commandDispatcher, IGraphElementModel model)
        {
            var ui = new NoiseSampleUI();

            ui.SetupBuildAndUpdate(model, commandDispatcher, elementBuilder.GraphView, elementBuilder.Context);
            return ui;
        }
    }
}