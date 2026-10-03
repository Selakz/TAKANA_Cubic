#nullable enable

using System;
using System.Collections.Generic;
using EditorPlugin.Shared.To;
using MusicGame.ChartEditor.InScreenEdit.CopyPaste;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Runtime.ECS;

namespace EditorPlugin.Shared
{
	/// <summary>
	/// The clipboard of the editor, as seen by plugins. Reads hand out detached copies, so plugin writes to them
	/// change neither the clipboard nor the chart; writes replace the whole clipboard content with copies of the
	/// models the plugin provides.
	/// </summary>
	public class ClipboardApi : IDisposable
	{
		// Private
		private readonly IDataset<ClipboardItem> clipboard;
		private readonly ChartApi chartApi;

		private List<ClipboardItem>? staging;

		// Constructor
		public ClipboardApi(IDataset<ClipboardItem> clipboard, ChartApi chartApi)
		{
			this.clipboard = clipboard;
			this.chartApi = chartApi;
		}

		// Defined Functions
		public void Dispose() => staging = null;

		/// <summary>
		/// One detached copy per clipboard item, in no particular order. The children of a copy are expanded
		/// lazily, so a copied track hands out its copied notes.
		/// </summary>
		public object[] getAll()
		{
			var relations = new ClipboardRelations(chartApi);
			List<object> items = new();
			foreach (var item in clipboard)
			{
				var component = IChartSerializable.Clone(item.Component);
				relations.AddRoot(component);
				if (item.Parent is { } source) relations.SetSourceParent(component, source);
				if (relations.GetSnapshot(component) is { } raw) items.Add(raw);
			}

			return items.ToArray();
		}

		/// <summary> Starts replacing the clipboard content. Nothing changes until <see cref="commitOverride" />. </summary>
		public void beginOverride() => staging = new List<ClipboardItem>();

		/// <summary> Stages a track copy together with its notes as the clipboard content. </summary>
		public bool addTrack(object model, object[] noteModels, int? layerId)
		{
			if (staging is null) return false;
			if (model is not ITrack track) return false;
			List<INote> notes = new();
			foreach (var noteModel in noteModels)
			{
				if (noteModel is not INote note) return false;
				notes.Add(note);
			}

			if (layerId is { } requestedLayer)
			{
				if (chartApi.Chart.GetsLayersInfo()[requestedLayer] is null) return false;
				track.SetLayer(requestedLayer);
			}

			var trackComponent = new ChartComponent(track) { Id = chartApi.Chart.NewId };
			foreach (var note in notes)
			{
				var noteComponent = new ChartComponent(note) { Id = chartApi.Chart.NewId };
				if (!noteComponent.SetParent(trackComponent)) return false;
			}

			staging.Add(new ClipboardItem(trackComponent, chartApi.Chart.DefaultJudgeLine()));
			return true;
		}

		/// <summary> Stages a note copy attached to the given track of the current chart. </summary>
		public bool addNote(object model, object rawTrack)
		{
			if (staging is null) return false;
			if (model is not INote note) return false;
			if (!chartApi.TryGetComponent(rawTrack, out var parent) || parent.Model is not ITrack) return false;
			staging.Add(new ClipboardItem(new ChartComponent(note) { Id = chartApi.Chart.NewId }, parent));
			return true;
		}

		/// <summary> Stages a floating (draft) note copy. </summary>
		public bool addDraftNote(object model)
		{
			if (staging is null) return false;
			if (model is not INote note) return false;
			staging.Add(new ClipboardItem(new ChartComponent(note) { Id = chartApi.Chart.NewId },
				chartApi.Chart.DefaultJudgeLine()));
			return true;
		}

		/// <summary> Replaces the clipboard content with the staged copies. </summary>
		public void commitOverride()
		{
			if (staging is null) return;
			clipboard.Clear();
			foreach (var item in staging) clipboard.Add(item);
			staging = null;
		}

		/// <summary> Drops the staged copies; the clipboard keeps its current content. </summary>
		public void cancelOverride() => staging = null;

		private static object BuildRaw(ChartComponent component, IComponentRelations relations) => component.Model switch
		{
			DraftHit => new RawDraftHitData(component, null),
			DraftHold => new RawDraftHoldData(component, null),
			Hit => new RawHitData(component, null, relations),
			Hold => new RawHoldData(component, null, relations),
			ITrack => new RawTrackData(component, null, relations),
			_ => throw new InvalidOperationException(
				$"Unsupported clipboard component model: {component.Model.GetType()}")
		};

		/// <summary>
		/// Relations of the copies handed out by <see cref="getAll" />. A copy belongs to no chart, so its parent is
		/// the component the clipboard item was copied from, and its children are the copies made in the same read.
		/// </summary>
		private sealed class ClipboardRelations : IComponentRelations
		{
			private readonly ChartApi chartApi;
			private readonly HashSet<ChartComponent> readRoots = new();
			private readonly Dictionary<ChartComponent, ChartComponent> sourceParents = new();
			private readonly Dictionary<ChartComponent, object> copies = new();

			public ClipboardRelations(ChartApi chartApi)
			{
				this.chartApi = chartApi;
			}

			/// <summary> Marks a component as one of the copies this read handed out. </summary>
			public void AddRoot(ChartComponent component) => readRoots.Add(component);

			public void SetSourceParent(ChartComponent component, ChartComponent sourceParent) =>
				sourceParents[component] = sourceParent;

			public ChartComponent? GetParent(ChartComponent component) =>
				component.Parent ?? sourceParents.GetValueOrDefault(component);

			public object? GetSnapshot(ChartComponent component)
			{
				if (copies.TryGetValue(component, out var copy)) return copy;
				if (chartApi.ResolveSnapshot(component) is { } snapshot) return snapshot;
				if (!IsCopyOfThisRead(component)) return null;
				if (component.Model.IsEditorOnly()) return null;
				return copies[component] = BuildRaw(component, this);
			}

			public LayerInfo? GetLayer(ChartComponent track) =>
				track.Model is ITrack model ? chartApi.Chart.GetsLayersInfo()[model.GetLayerId()] : null;

			/// <summary>
			/// Whether the component is a copy of this read. Copies and their descendants are the only detached
			/// components that may be turned into raw data here; every other detached component (the track a note
			/// was copied from, for instance, once it has left the chart) must stay invisible to plugins.
			/// </summary>
			private bool IsCopyOfThisRead(ChartComponent component)
			{
				for (ChartComponent? current = component; current is not null; current = current.Parent)
				{
					if (readRoots.Contains(current)) return true;
				}

				return false;
			}
		}
	}
}
