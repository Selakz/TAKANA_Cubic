#nullable enable

using System;
using EditorPlugin.PuerTS;
using EditorPlugin.Shared;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Message;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;

namespace EditorPlugin.PluginSystem
{
	public interface IBridgeBootstrapService
	{
		public void Initialize(PluginRuntimeEnv env);
	}

	public class T3BridgeBootstrapService : HierarchySystem<T3BridgeBootstrapService>, IBridgeBootstrapService
	{
		// Serializable and Public
		public override bool AsImplementedInterfaces => true;

		// Private
		[Inject] private IGameAudioPlayer music = default!;
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private CommandManager commandManager = default!;
		[Inject] private ChartSelectDataset chartSelectDataset = default!;
		[Inject] private EdgeNodeDataset edgeNodeDataset = default!;
		[Inject] private DirectNodeDataset directNodeDataset = default!;
		[Inject] private EdgeNodeSelectDataset edgeNodeSelectDataset = default!;
		[Inject] private DirectNodeSelectDataset directNodeSelectDataset = default!;
		[Inject] private StageMouseTimeRetriever timeRetriever = default!;
		[Inject] private StageMouseWidthRetriever widthRetriever = default!;
		[Inject] private MessageBox messageBox = default!;

		// Defined Functions
		public void Initialize(PluginRuntimeEnv env)
		{
			if (levelInfo.Value is not { } info) return;

			var registry = new StagingRegistry(commandManager);
			var chartApi = new ChartApi(info.Chart, registry, chartSelectDataset);
			var editorApi = new EditorApi(music, messageBox);
			var stagingApi = new StagingApi(registry);
			var nodeApi = new NodeApi(
				edgeNodeDataset, directNodeDataset, edgeNodeSelectDataset, directNodeSelectDataset, chartApi);
			var mouseApi = new MouseApi(timeRetriever, widthRetriever);
			var api = new T3CSharpApi(chartApi, editorApi, stagingApi, nodeApi, mouseApi, env.RootDirectory);
			env.AddDisposable(api);
			env.BridgeObject.Get<Action<object>>("__t3_bridge_init")(api);
		}
	}
}