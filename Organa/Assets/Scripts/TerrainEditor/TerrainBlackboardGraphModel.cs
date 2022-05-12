using System.Collections.Generic;
using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Organa.Editor
{
    public class TerrainBlackboardGraphModel : BlackboardGraphModel
    {
        internal static readonly string[] k_Sections = { "Properties"};

        /// <inheritdoc />
        public TerrainBlackboardGraphModel(IGraphAssetModel graphAssetModel)
            : base(graphAssetModel) { }

        public override string GetBlackboardTitle()
        {
            return AssetModel?.FriendlyScriptName == null ? "Terrain Model" : AssetModel?.FriendlyScriptName + " Terrain Model";
        }

        public override IEnumerable<string> SectionNames =>
            GraphModel == null ? Enumerable.Empty<string>() : k_Sections;

        public override IEnumerable<IVariableDeclarationModel> GetSectionRows(string sectionName)
        {
            if (sectionName == k_Sections[0])
            {
                return GraphModel?.VariableDeclarations?.Where(v => v.DataType == TerrainStencil.PlaceHolder) ??
                       Enumerable.Empty<IVariableDeclarationModel>();
            }

            return Enumerable.Empty<IVariableDeclarationModel>();
        }
    }
}