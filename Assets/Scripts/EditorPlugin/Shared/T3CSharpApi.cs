#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EditorPlugin.PluginSystem;
using EditorPlugin.Shared.To;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Message;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using Newtonsoft.Json;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Log;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared
{
	public class T3CSharpApi : IDisposable
	{
		public ChartApi chart { get; }
		public EditorApi editor { get; }
		public StagingApi staging { get; }
		public NodeApi nodes { get; }
		public MouseApi mouse { get; }

		private readonly string pluginDirectory;

		public T3CSharpApi(ChartApi chartApi, EditorApi editorApi, StagingApi stagingApi, NodeApi nodeApi,
			MouseApi mouseApi, string pluginDirectory)
		{
			chart = chartApi;
			editor = editorApi;
			staging = stagingApi;
			nodes = nodeApi;
			mouse = mouseApi;
			this.pluginDirectory = pluginDirectory;
		}

		public void Dispose()
		{
			chart.Dispose();
			editor.Dispose();
			staging.Dispose();
			nodes.Dispose();
			mouse.Dispose();
		}

		public ChartApi? loadChart(string path)
		{
			string fullPath = ResolvePath(path);
			ChartInfo? loaded = ChartLoader.LoadFromFileSync(fullPath);
			return loaded is null ? null : new ChartApi(loaded, null, null);
		}

		public ChartApi createNewChart()
		{
			return new ChartApi(new ChartInfo(), null, null);
		}

		public bool saveChart(string path, object chartApi)
		{
			if (chartApi is not ChartApi api) return false;
			string fullPath = ResolvePath(path);
			try
			{
				File.WriteAllText(fullPath, JsonConvert.SerializeObject(api.Chart.GetSerializationToken()));
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to save chart to {fullPath}: {e}");
				return false;
			}
		}

		private string ResolvePath(string path) => Path.IsPathRooted(path) ? path : Path.Combine(pluginDirectory, path);
	}

	public class ChartApi : IDisposable
	{
		private readonly ChartInfo chart;
		private readonly StagingRegistry? registry;
		private readonly ChartSelectDataset? chartSelectDataset;
		private readonly Dictionary<ChartComponent, object> componentSnapshots = new();
		private readonly Dictionary<object, ChartComponent> rawToComponent = new();

		private Action<object>? onNoteAddedInner;
		private Action<object>? onNoteRemovedInner;
		private Action<object>? onTrackAddedInner;
		private Action<object>? onTrackRemovedInner;

		internal ChartInfo Chart => chart;

		public RawBpmListData bpmList { get; }

		public RawLayersInfoData layersInfo { get; }

		internal ChartApi(ChartInfo chart, StagingRegistry? registry, ChartSelectDataset? chartSelectDataset)
		{
			this.chart = chart;
			this.registry = registry;
			this.chartSelectDataset = chartSelectDataset;
			bpmList = new RawBpmListData(chart.GetsBpmList());
			layersInfo = new RawLayersInfoData(chart.GetsLayersInfo());
			chart.OnComponentAdded += HandleComponentAdded;
			chart.OnComponentRemoved += HandleComponentRemoved;
			foreach (var component in chart.Components)
			{
				if (component.Model.IsEditorOnly()) continue;
				if (component.Model is INote or ITrack)
				{
					RegisterComponent(component);
				}
			}
		}

		public void onNoteAdded(Action<object> callback) => onNoteAddedInner += callback;

		public void onNoteRemoved(Action<object> callback) => onNoteRemovedInner += callback;

		public void onTrackAdded(Action<object> callback) => onTrackAddedInner += callback;

		public void onTrackRemoved(Action<object> callback) => onTrackRemovedInner += callback;

		public int offsetMilli => chart.GetsOffsetInfo().Value.Milli;

		internal object GetComponentSnapshot(ChartComponent component) => componentSnapshots[component];

		public object[] getAllNotes() =>
			componentSnapshots.Where(pair => pair.Key.Model is INote).Select(pair => pair.Value).ToArray();

		public object[] getAllTracks() =>
			componentSnapshots.Where(pair => pair.Key.Model is ITrack).Select(pair => pair.Value).ToArray();

		private void HandleComponentAdded(ChartComponent component)
		{
			if (component.Model.IsEditorOnly()) return;
			if (component.Model is INote)
			{
				RegisterComponent(component);

				try
				{
					onNoteAddedInner?.Invoke(componentSnapshots[component]);
				}
				catch (Exception e)
				{
					T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|onNoteAdded", T3LogType.Error);
					T3Logger.Log("MessageRaw", $"{e.Message}\n{e.StackTrace}");
				}
			}
			else if (component.Model is ITrack)
			{
				RegisterComponent(component);
				try
				{
					onTrackAddedInner?.Invoke(componentSnapshots[component]);
				}
				catch (Exception e)
				{
					T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|onTrackAdded", T3LogType.Error);
					T3Logger.Log("MessageRaw", $"{e.Message}\n{e.StackTrace}");
				}
			}
		}

		private void HandleComponentRemoved(ChartComponent component)
		{
			if (!componentSnapshots.Remove(component, out var snapshot)) return;
			rawToComponent.Remove(snapshot);
			if (component.Model is INote)
			{
				try
				{
					onNoteRemovedInner?.Invoke(snapshot);
				}
				catch (Exception e)
				{
					T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|onNoteRemoved", T3LogType.Error);
					T3Logger.Log("MessageRaw", $"{e.Message}\n{e.StackTrace}");
				}
			}
			else if (component.Model is ITrack)
			{
				try
				{
					onTrackRemovedInner?.Invoke(snapshot);
				}
				catch (Exception e)
				{
					T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|onTrackRemoved", T3LogType.Error);
					T3Logger.Log("MessageRaw", $"{e.Message}\n{e.StackTrace}");
				}
			}
		}

		public object[] getAllSelected()
		{
			if (chartSelectDataset is null) return Array.Empty<object>();
			var list = new List<object>();
			foreach (var component in chartSelectDataset)
			{
				if (componentSnapshots.TryGetValue(component, out var snapshot)) list.Add(snapshot);
			}

			return list.ToArray();
		}

		public object? getCurrentSelecting()
		{
			if (chartSelectDataset is null) return null;
			if (chartSelectDataset.CurrentSelecting.Value is not { } component) return null;
			return componentSnapshots.GetValueOrDefault(component);
		}

		public void addSelected(object raw)
		{
			if (chartSelectDataset is null) return;
			if (rawToComponent.TryGetValue(raw, out var component)) chartSelectDataset.Add(component);
		}

		public void removeSelected(object raw)
		{
			if (chartSelectDataset is null) return;
			if (rawToComponent.TryGetValue(raw, out var component)) chartSelectDataset.Remove(component);
		}

		public void clearSelected() => chartSelectDataset?.Clear();

		public void addTrack(object model, object[]? noteModels = null)
		{
			if (registry is null)
			{
				var component = AddComponent((IChartModel)model);
				component.SetParent(chart.DefaultJudgeLine());
				if (noteModels is not null)
				{
					foreach (var noteModel in noteModels)
					{
						var noteComponent = AddComponent((IChartModel)noteModel);
						noteComponent.SetParent(component);
					}
				}

				return;
			}

			registry.Add(() =>
			{
				var trackComponent = new ChartComponent((IChartModel)model) { Id = chart.NewId };
				var commands = new List<ICommand>
				{
					new AddComponentCommand(chart, trackComponent, chart.DefaultJudgeLine())
				};
				if (noteModels is not null)
				{
					foreach (var noteModel in noteModels)
					{
						commands.Add(new AddComponentCommand(chart, (IChartModel)noteModel, trackComponent));
					}
				}

				return new BatchCommand(commands, "Add Track with Notes");
			});
		}

		public void addNote(object model, object rawTrack)
		{
			if (registry is null)
			{
				var component = AddComponent((IChartModel)model);
				if (rawToComponent.TryGetValue(rawTrack, out var track)) component.SetParent(track);
				return;
			}

			rawToComponent.TryGetValue(rawTrack, out var parent);
			registry.Add(() => new AddComponentCommand(chart, (IChartModel)model, parent));
		}

		public void addDraftNote(object model)
		{
			if (registry is null)
			{
				var component = AddComponent((IChartModel)model);
				component.SetParent(chart.DefaultJudgeLine());
				return;
			}

			registry.Add(() => new AddComponentCommand(chart, (IChartModel)model, chart.DefaultJudgeLine()));
		}

		public void removeComponent(object raw)
		{
			if (!rawToComponent.TryGetValue(raw, out var component)) return;
			if (registry is null)
			{
				chart.RemoveComponent(component);
				return;
			}

			registry.Add(() => new DeleteComponentCommand(component));
		}

		private ChartComponent AddComponent(IChartModel model) => chart.AddComponent(model);

		private void RegisterComponent(ChartComponent component)
		{
			var snapshot = BuildComponentSnapshot(component);
			componentSnapshots[component] = snapshot;
			rawToComponent[snapshot] = component;
		}

		private object BuildComponentSnapshot(ChartComponent component)
		{
			return component.Model switch
			{
				DraftHit => new RawDraftHitData(component, registry),
				DraftHold => new RawDraftHoldData(component, registry),
				Hit => new RawHitData(component, registry, this),
				Hold => new RawHoldData(component, registry, this),
				ITrack => new RawTrackData(component, registry),
				_ => throw new InvalidOperationException($"Unsupported component model: {component.Model.GetType()}")
			};
		}

		public void Dispose()
		{
			chart.OnComponentAdded -= HandleComponentAdded;
			chart.OnComponentRemoved -= HandleComponentRemoved;
			onNoteAddedInner = null;
			onNoteRemovedInner = null;
			onTrackAddedInner = null;
			onTrackRemovedInner = null;
			componentSnapshots.Clear();
			rawToComponent.Clear();
		}
	}

	public class EditorApi : IDisposable
	{
		private readonly IGameAudioPlayer music;
		private readonly MessageBox messageBox;

		internal EditorApi(IGameAudioPlayer music, MessageBox messageBox)
		{
			this.music = music;
			this.messageBox = messageBox;
			chartTime = new ValueWrapper<int>(() => music.ChartTime.Milli, value => music.ChartTime = value);
		}

		public void Dispose()
		{
		}

		public IWrapper<int> chartTime { get; }

		public int audioLengthMilli => music.AudioLength.Milli;

		public void showHeader(I18NString content, int logType)
		{
			T3Logger.Log("NoticeRaw", content.Value, (T3LogType)logType);
		}

		public void showConfirm(I18NString content, Action? callback)
		{
			messageBox.ShowConfirm(content.Value, () =>
			{
				try
				{
					callback?.Invoke();
				}
				catch
				{
					T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|showConfirm", T3LogType.Error);
				}
			}, isKey: false);
		}

		public void showConfirmAndCancel(I18NString content, Action<int>? callback)
		{
			messageBox.ShowConfirmAndCancel(content.Value, choice =>
			{
				try
				{
					callback?.Invoke(choice);
				}
				catch
				{
					T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|showConfirmAndCancel", T3LogType.Error);
				}
			}, isKey: false);
		}
	}

	public class StagingApi : IDisposable
	{
		private readonly StagingRegistry registry;

		internal StagingApi(StagingRegistry registry)
		{
			this.registry = registry;
		}

		public void Dispose() => registry.Dispose();

		public bool hasPending => registry.HasPending;

		public void commit() => registry.Flush();
	}
}