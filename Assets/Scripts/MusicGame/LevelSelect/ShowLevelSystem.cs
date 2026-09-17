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
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.LevelSelect
{
	public class SortMethod
	{
		public I18NString NameLocalized { get; }

		public string Id { get; }

		private readonly Func<SongInfo, SongInfo, int, int> comparison;
		private int difficulty = 0;

		/// <param name="comparison"> The last int parameter stands for the difficulty the two levels are expected to be compared. </param>
		public SortMethod(string id, I18NString nameLocalized, Func<SongInfo, SongInfo, int, int> comparison)
		{
			Id = id;
			NameLocalized = nameLocalized;
			this.comparison = comparison;
		}

		public Comparison<SongInfo> GetComparison(int difficulty)
		{
			this.difficulty = difficulty;
			return Compare;
		}

		private int Compare(SongInfo a, SongInfo b) => comparison.Invoke(a, b, difficulty);
	}

	public class ShowLevelSystem : HierarchySystem<ShowLevelSystem>
	{
		// Serializable and Public
		[SerializeField] private DifficultyConfig difficultyConfig = default!;
		[SerializeField] private SongInfoPanel songInfoPanel = default!;
		[SerializeField] private LevelFancyScrollView levelScrollView = default!;
		[SerializeField] private TMP_Dropdown packDropdown = default!;
		[SerializeField] private TMP_Dropdown sortDropdown = default!;
		[SerializeField] private Button ascendButton = default!;
		[SerializeField] private Image ascendIcon = default!;
		[SerializeField] private float scrollDuration;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			// Level
			new CustomRegistrar(ApplyPackFilter, () => viewPool.Clear()),
			new DatasetRegistrar<LevelComponent<GameplayPreference>>(levelDataset,
				DatasetRegistrar<LevelComponent<GameplayPreference>>.RegisterTarget.DataAdded,
				component =>
				{
					SortDataset(difficulty);
					if (component.Model.SongInfo.Value is { } songInfo && CurrentPack.Contains(songInfo))
					{
						viewPool.Add(component);
					}
				}),
			new DatasetRegistrar<LevelComponent<GameplayPreference>>(levelDataset,
				DatasetRegistrar<LevelComponent<GameplayPreference>>.RegisterTarget.DataRemoved,
				component => viewPool.Remove(component)),
			new ViewPoolLifetimeRegistrar<LevelComponent<GameplayPreference>>(viewPool,
				handler => new LevelPanelRegistrar(handler.Script<LevelPanel>(), levelInfo, difficulty,
					viewPool[handler]!, difficultyConfig, preEntryPanel)),
			new PropertyRegistrar<RawLevelInfo<GameplayPreference>?>(levelInfo, info =>
			{
				memoryService.SyncLevelInfo(info);
				if (info is null) return;
				songInfoPanel.LoadCover(info.Cover.Value);
				songInfoPanel.LoadSongInfo(info.SongInfo.Value);
				var index = 0;
				foreach (var c in viewPool)
				{
					if (ReferenceEquals(c.Model, info))
					{
						levelScrollView.ScrollTo(index, scrollDuration);
						return;
					}

					index++;
				}
			}),
			new PropertyRegistrar<int>(difficulty, diff =>
			{
				SortAndKeepSelectedPosition(diff);
				memoryService.SyncDifficulty(diff);
			}),
			new PropertyRegistrar<bool>(levelsLoaded, loaded =>
			{
				if (loaded) Restore();
			}),

			// Pack
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
				ApplyPackFilter();
				memoryService.SyncPack(CurrentPack);
			}),

			// Sort
			new DropdownRegistrar(sortDropdown, _ =>
			{
				SortAndKeepSelectedPosition(difficulty);
				if (CurrentSort is { } sort) memoryService.SyncSort(sort, isAscend);
			}),
			new ButtonRegistrar(ascendButton, () =>
			{
				isAscend = !isAscend;
				ascendIcon.transform.rotation = Quaternion.Euler(0, 0, isAscend ? 0 : 180);
				SortAndKeepSelectedPosition(difficulty);
				if (CurrentSort is { } sort) memoryService.SyncSort(sort, isAscend);
			})
		};

		// Private
		[Inject] private NotifiableProperty<RawLevelInfo<GameplayPreference>?> levelInfo = default!;
		[Inject] private NotifiableProperty<int> difficulty = default!;
		[Inject] private NotifiableProperty<bool> levelsLoaded = default!;
		[Inject] private ILevelSelectMemoryService memoryService = default!;
		[Inject] private ListDataset<LevelComponent<GameplayPreference>> levelDataset = default!;
		[Inject] private ListDataset<PackInfo> packDataset = default!;
		[Inject] private IViewPool<LevelComponent<GameplayPreference>> viewPool = default!;
		[Inject] private PreEntryPanel preEntryPanel = default!;

		private PackInfo[] packOptions = Array.Empty<PackInfo>();
		private PackInfo CurrentPack => packOptions is { Length: > 0 } ? packOptions[packDropdown.value] : PackInfo.All;

		private SortMethod[] sortOptions = Array.Empty<SortMethod>();
		private SortMethod? CurrentSort => sortOptions is { Length: > 0 } ? sortOptions[sortDropdown.value] : null;

		private bool isAscend = true;

		// Constructor
		public override void SelfInstall(IContainerBuilder builder)
		{
			base.SelfInstall(builder);
			builder.RegisterComponent(levelScrollView).As<IViewPool<LevelComponent<GameplayPreference>>>();
		}

		// Defined Functions
		private void SortDataset(int diff)
		{
			levelDataset.Sort((a, b) =>
			{
				bool aHas = a.Model.SongInfo.Value?.Difficulties.ContainsKey(diff) ?? false;
				bool bHas = b.Model.SongInfo.Value?.Difficulties.ContainsKey(diff) ?? false;
				if (aHas != bHas) return aHas ? -1 : 1;
				if (a.Model.SongInfo.Value is { } aInfo && b.Model.SongInfo.Value is { } bInfo)
				{
					int compareResult;
					if (aHas && bHas) compareResult = CurrentSort?.GetComparison(diff).Invoke(aInfo, bInfo) ?? 0;
					else
					{
						var aMaxDifficulty = aInfo.Difficulties.Keys.DefaultIfEmpty(0).Max();
						var bMaxDifficulty = bInfo.Difficulties.Keys.DefaultIfEmpty(0).Max();
						// Bigger max difficulty first. In this case, return directly
						if (aMaxDifficulty != bMaxDifficulty) return bMaxDifficulty.CompareTo(aMaxDifficulty);
						else compareResult = CurrentSort?.GetComparison(aMaxDifficulty).Invoke(aInfo, bInfo) ?? 0;
					}

					// If compare result is 0, fallback to name comparison
					compareResult = compareResult == 0
						? string.Compare(aInfo.Title.Value, bInfo.Title.Value, StringComparison.Ordinal)
						: compareResult;
					return isAscend ? compareResult : -compareResult;
				}
				else return 0;
			});
		}

		private void SortAndKeepSelectedPosition(int diff)
		{
			viewPool.Clear();
			SortDataset(diff);
			foreach (var level in levelDataset) viewPool.Add(level);
			if (levelInfo.Value is not null)
			{
				var index = 0;
				foreach (var c in viewPool)
				{
					if (ReferenceEquals(c.Model, levelInfo.Value))
					{
						levelScrollView.JumpTo(index);
						return;
					}

					index++;
				}
			}

			levelScrollView.JumpTo(0);
		}

		private void ApplyPackFilter()
		{
			viewPool.Clear();
			foreach (var component in levelDataset)
			{
				if (component.Model.SongInfo.Value is { } songInfo && CurrentPack.Contains(songInfo))
					viewPool.Add(component);
			}
		}

		private void Restore()
		{
			var setting = ISingletonSetting<LevelSelectSetting>.Instance;

			var sortIndex = string.IsNullOrEmpty(setting.LastSortId)
				? 0
				: Array.FindIndex(sortOptions, option => option.Id == setting.LastSortId);
			sortDropdown.SetValueWithoutNotify(sortIndex >= 0 ? sortIndex : 0);
			isAscend = setting.LastSortAscend;
			ascendIcon.transform.rotation = Quaternion.Euler(0, 0, isAscend ? 0 : 180);

			var packIndex = string.IsNullOrEmpty(setting.LastPackId)
				? 0
				: Array.FindIndex(packOptions, pack => pack.Id == setting.LastPackId);
			packDropdown.SetValueWithoutNotify(packIndex >= 0 ? packIndex : 0);
			ApplyPackFilter();

			var lastComponent = FindLastSelectedComponent();
			if (lastComponent is not null)
			{
				levelInfo.Value = lastComponent.Model;
				difficulty.Value = setting.LastDifficulty;
			}
			else if (levelInfo.Value is null && viewPool.FirstOrDefault() is { } first)
			{
				levelInfo.Value = first.Model;
				if (first.Model.SongInfo.Value?.Difficulties is { } difficulties)
				{
					difficulty.Value = difficulties.Keys.DefaultIfEmpty(3).Max();
				}
			}

			SortAndKeepSelectedPosition(difficulty);
			return;

			LevelComponent<GameplayPreference>? FindLastSelectedComponent()
			{
				LevelComponent<GameplayPreference>? pathMatch = null;
				foreach (var component in viewPool)
				{
					if (!string.IsNullOrEmpty(setting.LastSongId) &&
					    component.Model.SongInfo.Value?.Id == setting.LastSongId) return component;
					if (pathMatch is null && component.Model.LevelPath == setting.LastLevelPath)
						pathMatch = component;
				}

				return pathMatch;
			}
		}

		private static float GetLevelValue(string levelDisplay)
		{
			if (float.TryParse(levelDisplay, out var value))
				return value;
			if (levelDisplay.EndsWith('+') && float.TryParse(levelDisplay.Remove(levelDisplay.Length - 1), out value))
				return Mathf.CeilToInt(value) + 0.99999f;
			return float.MaxValue;
		}

		// System Functions
		void Start()
		{
			var defaultSortOptions = new List<SortMethod>
			{
				new("ByName", I18NString.FromLocalized("LevelSelect_Sort_ByName"), (a, b, _) =>
					string.Compare(a.Title.Value, b.Title.Value, StringComparison.Ordinal)),
				new("ByLevel", I18NString.FromLocalized("LevelSelect_Sort_ByLevel"), (a, b, diff) =>
				{
					var aVal = a.Difficulties.TryGetValue(diff, out var aLevel) ? aLevel.LevelDisplay : string.Empty;
					var bVal = b.Difficulties.TryGetValue(diff, out var bLevel) ? bLevel.LevelDisplay : string.Empty;
					return GetLevelValue(aVal).CompareTo(GetLevelValue(bVal));
				}),
				new("ByScore", I18NString.FromLocalized("LevelSelect_Sort_ByScore"), (a, b, diff) =>
				{
					var aScore = ISingleton<PlayInfo>.Instance.GetPlayData(a.Id, diff)?.Score ?? 0;
					var bScore = ISingleton<PlayInfo>.Instance.GetPlayData(b.Id, diff)?.Score ?? 0;
					return aScore.CompareTo(bScore);
				})
			};

			sortOptions = sortDropdown.SetOptions(
				defaultSortOptions,
				sortMethod => sortMethod.NameLocalized.Value);
		}
	}
}