#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit.CopyPaste
{
	[Serializable]
	public struct ModuleDetail
	{
		[field: SerializeField]
		public SequencePriority ModuleId { get; set; }

		[field: SerializeField]
		public string ClipboardText { get; set; }
	}

	public class CopyPasteModuleSystem : HierarchySystem<CopyPasteModuleSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriorities aliveModules = default!;
		[SerializeField] private InspectorDictionary<T3Flag, ModuleDetail> typeModuleMap = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(moduleInfo.CurrentModule,
				id => IsEnabled = aliveModules.Values.Any(moduleId => moduleId == id))
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Cut", () =>
			{
				if (selectDataset.Count == 0) return;
				clipboard.Clear();

				List<ChartComponent> cutComponents = new();
				foreach (var type in typeModuleMap.Value.Keys)
				{
					if (selectDataset.Any(component => T3ChartClassifier.Instance.IsOfType(component, type)))
					{
						cutComponents.AddRange(selectDataset.Where(component =>
							T3ChartClassifier.Instance.IsOfType(component, type)));
						break;
					}
				}

				List<ICommand> commands = new();
				foreach (var component in cutComponents)
				{
					clipboard.Add(ClipboardItem.FromComponent(component));
					commands.Add(new DeleteComponentCommand(component));
				}

				commandManager.Add(new BatchCommand(commands, "NoteCut"));
			}),
			new InputRegistrar("InScreenEdit", "Copy", () =>
			{
				if (selectDataset.Count == 0) return;
				clipboard.Clear();

				List<ChartComponent> copyComponents = new();
				foreach (var type in typeModuleMap.Value.Keys)
				{
					if (selectDataset.Any(component => T3ChartClassifier.Instance.IsOfType(component, type)))
					{
						copyComponents.AddRange(selectDataset.Where(component =>
							T3ChartClassifier.Instance.IsOfType(component, type)));
						T3Logger.Log("Notice", "Edit_CopyPaste_CopySuccess", T3LogType.Success);
						break;
					}
				}

				foreach (var component in copyComponents)
				{
					clipboard.Add(ClipboardItem.FromComponent(component));
				}
			}),
			new InputRegistrar("InScreenEdit", "Paste", ChangeModule),
			new InputRegistrar("InScreenEdit", "ExactPaste", ChangeModule),
			new InputRegistrar("InScreenEdit", "CheckClipboard", () =>
			{
				foreach (var item in clipboard)
				{
					foreach (var type in typeModuleMap.Value.Keys
						         .Where(type => T3ChartClassifier.Instance.IsOfType(item.Component, type)))
					{
						var count = clipboard.Count(c => T3ChartClassifier.Instance.IsOfType(c.Component, type));
						T3Logger.Log("Notice", $"{typeModuleMap.Value[type].ClipboardText}|{count}", T3LogType.Info);
						return;
					}
				}

				T3Logger.Log("Notice", "Edit_CopyPaste_Empty", T3LogType.Info);
			}),
			new InputRegistrar("InScreenEdit", "Create", () =>
			{
				UniTask.Yield().ToUniTask().ContinueWith(() =>
				{
					foreach (var detail in typeModuleMap.Value.Values) moduleInfo.Unregister(detail.ModuleId);
				});
			}),
			new InputRegistrar("General", "Escape", () =>
			{
				foreach (var detail in typeModuleMap.Value.Values) moduleInfo.Unregister(detail.ModuleId);
			})
		};

		// Private
		private ModuleInfo moduleInfo = default!;
		private IDataset<ClipboardItem> clipboard = default!;
		private ChartSelectDataset selectDataset = default!;
		private CommandManager commandManager = default!;

		// Constructor
		[Inject]
		private void Construct(
			ModuleInfo moduleInfo,
			IDataset<ClipboardItem> clipboard,
			ChartSelectDataset selectDataset,
			CommandManager commandManager)
		{
			this.moduleInfo = moduleInfo;
			this.clipboard = clipboard;
			this.selectDataset = selectDataset;
			this.commandManager = commandManager;
		}

		// Defined Functions
		private void ChangeModule()
		{
			foreach (var item in clipboard)
			{
				var find = false;
				foreach (var type in typeModuleMap.Value.Keys
					         .Where(type => T3ChartClassifier.Instance.IsOfType(item.Component, type)))
				{
					moduleInfo.Register(typeModuleMap.Value[type].ModuleId);
					find = true;
					break;
				}

				if (find) break;
			}
		}
	}
}