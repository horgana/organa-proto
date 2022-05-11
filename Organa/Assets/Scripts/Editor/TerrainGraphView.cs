using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Organa.Editor
{
    public class TerrainGraphView : GraphView
    {
        static readonly Vector2 s_CopyOffset = new Vector2(50, 50);
        TerrainGraphWindow _terrainGraphWindow;

        public TerrainGraphWindow window => _terrainGraphWindow;

        public TerrainGraphView(TerrainGraphWindow terrainGraphWindow, CommandDispatcher store, string graphViewName)
            : base(terrainGraphWindow, store, graphViewName)
        {
            _terrainGraphWindow = terrainGraphWindow;
        }
    }
}