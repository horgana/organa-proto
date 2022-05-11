using System.Linq;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Organa.Editor
{
    public class TerrainStencil : Stencil
    {
        public static string toolName = "Terrain Editor";

        public override string ToolName => toolName;

        public static readonly string graphName = "Terrain Graph";
        public static TypeHandle PlaceHolder { get; } = TypeSerializer.GenerateCustomTypeHandle("Placeholder");

        public static TypeHandle Generator { get; } = TypeSerializer.GenerateTypeHandle<NoiseGenerator2D<N>>();
        
        //public static TypeHandle Cookware { get; } = TypeSerializer.GenerateCustomTypeHandle("Cookware");

        public TerrainStencil()
        {
            SetSearcherSize(SearcherService.Usage.k_CreateNode, new Vector2(375, 300), 2.0f);
        }

        /// <inheritdoc />
        public override IBlackboardGraphModel CreateBlackboardGraphModel(IGraphAssetModel graphAssetModel)
        {
            return new TerrainBlackboardGraphModel(graphAssetModel);
        }

        /// <inheritdoc />
        public override void PopulateBlackboardCreateMenu(string sectionName, GenericMenu menu, CommandDispatcher commandDispatcher)
        {
            if (sectionName == TerrainBlackboardGraphModel.k_Sections[0])
            {
                menu.AddItem(new GUIContent("Add"), false, () =>
                {
                    CreateVariableDeclaration(PlaceHolder.Identification, PlaceHolder);
                });
            }
            
            void CreateVariableDeclaration(string name, TypeHandle type)
            {
                var finalName = name;
                var i = 0;

                // ReSharper disable once AccessToModifiedClosure
                while (commandDispatcher.State.WindowState.GraphModel.VariableDeclarations.Any(v => v.Title == finalName))
                    finalName = name + i++;

                commandDispatcher.Dispatch(new CreateGraphVariableDeclarationCommand(finalName, true, type));
            }
        }
    }
}
