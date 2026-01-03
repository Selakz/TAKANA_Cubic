#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Basic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
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
		[SerializeField] private InspectorDictionary<T3Flag, InspectorDictionary<string, Sprite>> sprites = default!;

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
		private Dictionary<T3Flag, Dictionary<string, Sprite>> NoteSelectedSprites => noteSelectedSprites ??=
			new(sprites.Value.Select(pair =>
				new KeyValuePair<T3Flag, Dictionary<string, Sprite>>(pair.Key, pair.Value.Value)));

		private readonly T3ChartClassifier classifier = new();
		private Dictionary<T3Flag, Dictionary<string, Sprite>>? noteSelectedSprites;
		private ChartSelectDataset selectDataset = default!;
		private IViewPool<ChartComponent> viewPool = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			ChartSelectDataset selectDataset,
			[Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.selectDataset = selectDataset;
			this.viewPool = viewPool;
		}

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
						presenter.Textures[textures.Key].SpriteModifier.Register
							(_ => textures.Value, noteSpritePriority.Value);
						presenter.Textures[textures.Key].SortingOrderModifier.Register
							(value => value + 1, noteSortingOrderPriority.Value);
					}
				}
			}

			if (classifier.IsSubType(T3Flag.Track, type))
			{
				presenter.ColorModifier.Register
					(_ => ISingleton<SelectSetting>.Instance.TrackSelectedColor, trackColorPriority.Value);
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
						presenter.Textures[textures.Key].SpriteModifier.Unregister(noteSpritePriority.Value);
						presenter.Textures[textures.Key].SortingOrderModifier
							.Unregister(noteSortingOrderPriority.Value);
					}
				}
			}

			if (classifier.IsSubType(T3Flag.Track, type))
			{
				presenter.ColorModifier.Unregister(trackColorPriority.Value);
				presenter.Textures["main"].SortingOrderModifier.Unregister(trackSortingOrderPriority.Value);
			}
		}
	}
}