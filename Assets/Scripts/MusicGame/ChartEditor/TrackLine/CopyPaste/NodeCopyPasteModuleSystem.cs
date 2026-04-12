#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine.CopyPaste
{
	public class NodeCopyPasteModuleSystem : HierarchySystem<NodeCopyPasteModuleSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriorities aliveModules = default!;
		[SerializeField] private SequencePriority nodeModuleId = default!;
		[SerializeField] private SequencePriority copyPasteModuleId = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(moduleInfo.CurrentModule,
				id => IsEnabled = aliveModules.Values.Any(moduleId => moduleId == id)),
			// Critically these registrars should be a single system but whatever.
			new PropertyRegistrar<ChartComponent?>(chartSelectDataset.CurrentSelecting, () =>
			{
				if (moduleInfo.CurrentModule == nodeModuleId) moduleInfo.Unregister(nodeModuleId);
			}),
			new PropertyRegistrar<EdgeNodeComponent?>(edgeSelectDataset.CurrentSelecting, () =>
			{
				if (edgeSelectDataset.CurrentSelecting.LastValue is null &&
				    edgeSelectDataset.CurrentSelecting.Value is not null) moduleInfo.Register(nodeModuleId);
			}),
			new PropertyRegistrar<DirectNodeComponent?>(directSelectDataset.CurrentSelecting, () =>
			{
				if (directSelectDataset.CurrentSelecting.LastValue is null &&
				    directSelectDataset.CurrentSelecting.Value is not null) moduleInfo.Register(nodeModuleId);
			})
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Cut", () =>
			{
				if (edgeSelectDataset.Count == 0 && directSelectDataset.Count == 0) return;
				clipboard.Clear();
				Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
				foreach (var component in edgeSelectDataset)
				{
					clipboard.Add(NodeRawInfo.FromComponent(component));
					var track = component.Locator.Track;
					var arg = new UpdateMoveListArg(component.Locator.IsLeft, component.Locator.Time, null);
					if (!args.ContainsKey(track)) args[track] = new() { arg };
					else args[track].Add(arg);
				}

				foreach (var component in directSelectDataset)
				{
					clipboard.Add(NodeRawInfo.FromComponent(component));
					var track = component.Locator.Track;
					var arg = new UpdateMoveListArg(component.Locator.IsPos, component.Locator.Time, null);
					if (!args.ContainsKey(track)) args[track] = new() { arg };
					else args[track].Add(arg);
				}

				clipboard.Sort((a, b) => a.Time.Value.CompareTo(b.Time));

				List<ICommand> commands = (
						from pair in args
						let command = new UpdateMoveListCommand(pair.Value)
						where command.SetInit(pair.Key)
						select command)
					.Cast<ICommand>().ToList();
				commandManager.Add(new BatchCommand(commands, "NodeCut"));
			}),
			new InputRegistrar("InScreenEdit", "Copy", () =>
			{
				if (edgeSelectDataset.Count == 0 && directSelectDataset.Count == 0) return;
				clipboard.Clear();
				foreach (var component in edgeSelectDataset) clipboard.Add(NodeRawInfo.FromComponent(component));
				foreach (var component in directSelectDataset) clipboard.Add(NodeRawInfo.FromComponent(component));
				clipboard.Sort((a, b) => a.Time.Value.CompareTo(b.Time));
				T3Logger.Log("Notice", "Edit_CopyPaste_CopySuccess", T3LogType.Success);
			}),
			new InputRegistrar("InScreenEdit", "Paste", ChangeModule),
			new InputRegistrar("InScreenEdit", "ExactPaste", ChangeModule),
			new InputRegistrar("InScreenEdit", "CheckClipboard", () =>
			{
				T3Logger.Log("Notice", clipboard.Count > 0
						? $"TrackLine_NodeClipboard|{clipboard.Count}"
						: "Edit_CopyPaste_Empty",
					T3LogType.Info);
			}),
			new InputRegistrar("InScreenEdit", "Create", () => moduleInfo.Unregister(copyPasteModuleId)),
			new InputRegistrar("General", "Escape", () => moduleInfo.Unregister(copyPasteModuleId))
		};

		// Private
		[Inject] private readonly ModuleInfo moduleInfo = default!;
		[Inject] [Key("clipboard")] private readonly List<NodeRawInfo> clipboard = default!;
		[Inject] private readonly ChartSelectDataset chartSelectDataset = default!;
		[Inject] private readonly EdgeNodeSelectDataset edgeSelectDataset = default!;
		[Inject] private readonly DirectNodeSelectDataset directSelectDataset = default!;
		[Inject] private readonly CommandManager commandManager = default!;

		// Event Handlers
		private void ChangeModule()
		{
			if (clipboard.Count > 0) moduleInfo.Register(copyPasteModuleId);
		}
	}
}