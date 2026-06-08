#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Select
{
	[Serializable]
	public struct SelectInclusionConfig
	{
		public T3Flag selectedType;
		public bool isInclusion;
		public List<T3Flag> targetTypes;
	}

	public class SelectManageSystem : HierarchySystem<SelectManageSystem>
	{
		// Serializable and Public
		[SerializeField] private List<SelectInclusionConfig> selectInclusionConfigs = new();

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			// Clear dataset when chart change
			new PropertyRegistrar<LevelInfo?>(levelInfo, () => dataset.Clear()),
			// Clear other type of components when current selecting change
			new PropertyRegistrar<ChartComponent?>(dataset.CurrentSelecting, () =>
			{
				var selecting = dataset.CurrentSelecting.Value;
				if (selecting is null) return;

				var selectedType = Classifier.Classify(selecting);
				foreach (var config in selectInclusionConfigs)
				{
					if (!Classifier.IsSubType(config.selectedType, selectedType)) continue;
					var selected = dataset.ToArray();
					if (config.isInclusion)
					{
						foreach (var component in selected)
						{
							if (!config.targetTypes.Any(flag => Classifier.IsOfType(component, flag)))
								dataset.Remove(component);
						}
					}
					else
					{
						foreach (var component in selected)
						{
							if (config.targetTypes.Any(flag => Classifier.IsOfType(component, flag)))
								dataset.Remove(component);
						}
					}
				}
			}),
			new PropertyNestedRegistrar<LevelInfo?>(levelInfo, info => CustomRegistrar.Generic<Action<ChartComponent>>(
				action => info!.Chart.OnComponentRemoved += action,
				action => info!.Chart.OnComponentRemoved -= action,
				component => dataset.Remove(component)))
		};

		// Private
		[Inject] private readonly NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private readonly ChartSelectDataset dataset = default!;

		private static T3ChartClassifier Classifier => T3ChartClassifier.Instance;
	}
}