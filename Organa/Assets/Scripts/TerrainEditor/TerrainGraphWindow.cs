using System;
using System.Collections.Generic;
using Organa.Terrain;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.Samples.Recipes;
using UnityEngine;

namespace Organa.Editor
{
    public class TerrainGraphWindow : GraphViewEditorWindow
    {
        [InitializeOnLoadMethod]
        static void RegisterTool()
        {
            ShortcutHelper.RegisterDefaultShortcuts<TerrainGraphWindow>(TerrainStencil.toolName);
        }

        [MenuItem("Organa/Terrain Editor", false)]
        public static void ShowTerrainGraphWindow()
        {
            FindOrCreateGraphWindow<TerrainGraphWindow>();
        }

        protected override void OnEnable()
        {
            EditorToolName = "Terrain Editor";
            base.OnEnable();
        }

        /// <inheritdoc />
        protected override GraphToolState CreateInitialState()
        {
            var prefs = Preferences.CreatePreferences(EditorToolName);
            return new TerrainState(GUID, prefs);
        }

        protected override GraphView CreateGraphView()
        {
            return new TerrainGraphView(this, CommandDispatcher, EditorToolName);
        }

        protected override BlankPage CreateBlankPage()
        {
            var onboardingProviders = new List<OnboardingProvider>();
            onboardingProviders.Add(new TerrainOnboardingProvider());

            return new BlankPage(CommandDispatcher, onboardingProviders);
        }

        /// <inheritdoc />
        protected override bool CanHandleAssetType(GraphAssetModel asset)
        {
            return asset is TerrainGraphAssetModel;
        }
    }
}