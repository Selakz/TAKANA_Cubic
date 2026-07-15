#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Level;
using MusicGame.LevelSelect.UI;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.LevelSelect
{
	public class ShowLevelSystem : HierarchySystem<ShowLevelSystem>
	{
		// Serializable and Public
		[SerializeField] private ViewPoolInstaller levelPanelInstaller = default!;
		[SerializeField] private DifficultyConfig difficultyConfig = default!;
		[SerializeField] private SongInfoPanel songInfoPanel = default!;
		[SerializeField] private ScrollRect scrollRect = default!;
		[SerializeField] private TMP_Dropdown packDropdown = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new CustomRegistrar(
				() =>
				{
					foreach (var component in levelDataset)
					{
						if (component.Model.SongInfo.Value is { } songInfo && CurrentPack.Contains(songInfo))
							viewPool.Add(component);
					}
				},
				() => viewPool.Clear()
			),
			new DatasetRegistrar<LevelComponent<GameplayPreference>>(levelDataset,
				DatasetRegistrar<LevelComponent<GameplayPreference>>.RegisterTarget.DataAdded,
				component =>
				{
					Sort(difficulty);
					if (component.Model.SongInfo.Value is { } songInfo && CurrentPack.Contains(songInfo))
					{
						viewPool.Add(component);
					}
				}),
			new DatasetRegistrar<LevelComponent<GameplayPreference>>(levelDataset,
				DatasetRegistrar<LevelComponent<GameplayPreference>>.RegisterTarget.DataRemoved,
				component => viewPool.Remove(component)),
			new ViewPoolRegistrar<LevelComponent<GameplayPreference>>(viewPool,
				ViewPoolRegistrar<LevelComponent<GameplayPreference>>.RegisterTarget.Get,
				handler =>
				{
					var component = viewPool[handler]!;
					if (viewPool.Count == 1)
					{
						levelInfo.Value = component.Model;
						if (component.Model.SongInfo.Value?.Difficulties is { } difficulties)
						{
							difficulty.Value = difficulties.Keys.DefaultIfEmpty(3).Max();
						}
					}
				}),
			new ViewPoolLifetimeRegistrar<LevelComponent<GameplayPreference>>(viewPool,
				handler => new LevelPanelRegistrar(handler.Script<LevelPanel>(), levelInfo, difficulty,
					viewPool[handler]!, difficultyConfig, preEntryPanel)),
			new ListDatasetViewSorter<LevelComponent<GameplayPreference>>(levelDataset, viewPool),
			new PropertyRegistrar<RawLevelInfo<GameplayPreference>?>(levelInfo, info =>
			{
				if (info is null) return;
				songInfoPanel.LoadCover(info.Cover.Value);
				songInfoPanel.LoadSongInfo(info.SongInfo.Value);
			}),
			new PropertyRegistrar<int>(difficulty, diff =>
			{
				Sort(diff);
				scrollRect.verticalNormalizedPosition = 1;
			}),

			new DatasetRegistrar<PackInfo>(packDataset,
				DatasetRegistrar<PackInfo>.RegisterTarget.DataAddedOrRemoved,
				_ =>
				{
					var previous = CurrentPack;
					packOptions = packDropdown.SetOptions(
						new List<PackInfo>(packDataset),
						pack => pack.Title.Value);

					var index = Array.FindIndex(packOptions,
						p => p.Id == previous.Id || ReferenceEquals(p, previous));
					packDropdown.SetValueWithoutNotify(index >= 0 ? index : 0);
				}),
			new DropdownRegistrar(packDropdown, _ =>
			{
				viewPool.Clear();
				foreach (var component in levelDataset)
				{
					if (component.Model.SongInfo.Value is { } songInfo && CurrentPack.Contains(songInfo))
						viewPool.Add(component);
				}
			})
		};

		// Private
		[Inject] private NotifiableProperty<RawLevelInfo<GameplayPreference>?> levelInfo = default!;
		[Inject] private NotifiableProperty<int> difficulty = default!;
		[Inject] private ListDataset<LevelComponent<GameplayPreference>> levelDataset = default!;
		[Inject] private ListDataset<PackInfo> packDataset = default!;
		[Inject] private IViewPool<LevelComponent<GameplayPreference>> viewPool = default!;
		[Inject] private PreEntryPanel preEntryPanel = default!;

		private PackInfo[] packOptions = Array.Empty<PackInfo>();
		private PackInfo CurrentPack => packOptions is { Length: > 0 } ? packOptions[packDropdown.value] : PackInfo.All;

		// Constructor
		public override void SelfInstall(IContainerBuilder builder)
		{
			base.SelfInstall(builder);
			levelPanelInstaller.Register<ViewPool<LevelComponent<GameplayPreference>>,
				LevelComponent<GameplayPreference>>(builder, Lifetime.Singleton);
		}

		// Defined Functions
		private void Sort(int diff)
		{
			levelDataset.Sort((a, b) =>
			{
				bool aHas = a.Model.SongInfo.Value?.Difficulties.ContainsKey(diff) ?? false;
				bool bHas = b.Model.SongInfo.Value?.Difficulties.ContainsKey(diff) ?? false;
				if (aHas != bHas) return aHas ? -1 : 1;
				if (a.Model.SongInfo.Value is { } aInfo && b.Model.SongInfo.Value is { } bInfo)
					return string.Compare(aInfo.Title.Value, bInfo.Title.Value, StringComparison.Ordinal);
				else return 0;
			});
		}
	}
}