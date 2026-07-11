#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Basic;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Stage;
using MusicGame.Models;
using T3Framework.Preset.Wrapper;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Select
{
	public class SelectViewSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority trackColorPriority = default!;
		[SerializeField] private SequencePriority noteSpritePriority = default!;
		[SerializeField] private SequencePriority trackSortingOrderPriority = default!;
		[SerializeField] private SequencePriority noteSortingOrderPriority = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolRegistrar<ChartComponent>(viewPool, ViewPoolRegistrar<ChartComponent>.RegisterTarget.Get,
				handler =>
				{
					var component = viewPool[handler]!;
					if (selectDataset.Contains(component))
					{
						var type = classifier.Classify(component);
						Register(type, handler);
					}
				}),
			new ViewPoolRegistrar<ChartComponent>(viewPool, ViewPoolRegistrar<ChartComponent>.RegisterTarget.Release,
				handler =>
				{
					var component = viewPool[handler]!;
					if (selectDataset.Contains(component))
					{
						var type = classifier.Classify(component);
						Unregister(type, handler);
					}
				}),
			new DatasetRegistrar<ChartComponent>(selectDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataAdded,
				component =>
				{
					var handler = viewPool[component];
					if (handler is not null)
					{
						var type = classifier.Classify(component);
						Register(type, handler);
					}
				}),
			new DatasetRegistrar<ChartComponent>(selectDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					var handler = viewPool[component];
					if (handler is not null)
					{
						var type = classifier.Classify(component);
						Unregister(type, handler);
					}
				})
		};

		// Private
		[Inject] private ChartSelectDataset selectDataset = default!;
		[Inject] private NotifiableProperty<GameplayStageSkinConfig> skinConfig = default!;
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;

		private Dictionary<T3Flag, Dictionary<string, Sprite>> NoteSelectedSprites
		{
			get
			{
				if (skinConfig.Value is EditorStageSkinConfig config) return config.SelectedSprites;
				Debug.LogError("Current skin config is not EditorStageSkinConfig");
				return null!;
			}
		}

		private readonly T3ChartClassifier classifier = T3ChartClassifier.Instance;
		private Dictionary<T3Flag, Dictionary<string, Sprite>>? noteSelectedSprites;

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		private void Register(T3Flag type, PrefabHandler handler)
		{
			var presenter = handler.Script<IT3ModelViewPresenter>();
			foreach (var pair in NoteSelectedSprites)
			{
				if (classifier.IsSubType(pair.Key, type))
				{
					foreach (var textures in pair.Value)
					{
						if (presenter.Textures.TryGetValue(textures.Key, out var modifier) &&
						    modifier is SpriteRendererModifier srm)
						{
							srm.SpriteModifier.Register(_ => textures.Value, noteSpritePriority.Value);
							srm.SortingOrderModifier.Register(value => value + 1, noteSortingOrderPriority.Value);
						}
					}
				}
			}

			if (classifier.IsSubType(T3Flag.Track, type))
			{
				foreach (var cm in presenter.ColorModifiers)
					cm.Register(_ => ISingleton<SelectSetting>.Instance.TrackSelectedColor, trackColorPriority.Value);
				presenter.Textures["main"].SortingOrderModifier.Assign
					(50, trackSortingOrderPriority.Value); // Temp
			}
		}

		private void Unregister(T3Flag type, PrefabHandler handler)
		{
			var presenter = handler.Script<IT3ModelViewPresenter>();
			foreach (var pair in NoteSelectedSprites)
			{
				if (classifier.IsSubType(pair.Key, type))
				{
					foreach (var textures in pair.Value)
					{
						if (presenter.Textures.TryGetValue(textures.Key, out var modifier) &&
						    modifier is SpriteRendererModifier srm)
						{
							srm.SpriteModifier.Unregister(noteSpritePriority.Value);
							srm.SortingOrderModifier.Unregister(noteSortingOrderPriority.Value);
						}
					}
				}
			}

			if (classifier.IsSubType(T3Flag.Track, type))
			{
				foreach (var cm in presenter.ColorModifiers) cm.Unregister(trackColorPriority.Value);
				presenter.Textures["main"].SortingOrderModifier.Unregister(trackSortingOrderPriority.Value);
			}
		}
	}
}