using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Organa.Editor
{
    public class TestNode : CollapsibleInOutNode
    {
        
    }

    public class TestNodeModel : NodeModel
    {
        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            AddInputPort("Input", PortType.Data, TypeSerializer.GenerateTypeHandle<NoiseGenerator2D<Noise.Perlin>>());
        }
    }
}