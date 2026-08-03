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
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.LevelSelect
{
	public class SortMethod
	{
		public I18NString NameLocalized { get; }

		private readonly Func<SongInfo, SongInfo, int, int> comparison;
		private int difficulty = 0;

		/// <param name="comparison"> The last int parameter stands for the difficulty the two levels are expected to be compared. </param>
		public SortMethod(I18NString nameLocalized, Func<SongInfo, SongInfo, int, int> comparison)
		{
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
		[SerializeField] private ViewPoolInstaller levelPanelInstaller = default!;
		[SerializeField] private DifficultyConfig difficultyConfig = default!;
		[SerializeField] private SongInfoPanel songInfoPanel = default!;
		[SerializeField] private ScrollRect scrollRect = default!;
		[SerializeField] private TMP_Dropdown packDropdown = default!;
		[SerializeField] private TMP_Dropdown sortDropdown = default!;
		[SerializeField] private Button ascendButton = default!;
		[SerializeField] private Image ascendIcon = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			// Level
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
			new PropertyRegistrar<int>(difficulty, SortAndKeepSelectedPosition),

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
				viewPool.Clear();
				foreach (var component in levelDataset)
				{
					if (component.Model.SongInfo.Value is { } songInfo && CurrentPack.Contains(songInfo))
						viewPool.Add(component);
				}
			}),

			// Sort
			new DropdownRegistrar(sortDropdown, _ => SortAndKeepSelectedPosition(difficulty)),
			new ButtonRegistrar(ascendButton, () =>
			{
				isAscend = !isAscend;
				ascendIcon.transform.rotation = Quaternion.Euler(0, 0, isAscend ? 0 : 180);
				SortAndKeepSelectedPosition(difficulty);
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

		private SortMethod[] sortOptions = Array.Empty<SortMethod>();
		private SortMethod? CurrentSort => sortOptions is { Length: > 0 } ? sortOptions[sortDropdown.value] : null;

		private bool isAscend = true;

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
			var content = scrollRect.content;
			LevelComponent<GameplayPreference>? selectedComponent = null;
			if (levelInfo.Value is not null)
			{
				foreach (var c in levelDataset)
				{
					if (ReferenceEquals(c.Model, levelInfo.Value))
					{
						selectedComponent = c;
						break;
					}
				}
			}

			var selectedTransform = selectedComponent is not null ? viewPool[selectedComponent]?.transform : null;
			float contentAnchoredYBefore = content.anchoredPosition.y;
			float itemLocalYBefore = selectedTransform?.localPosition.y ?? 0;

			Sort(diff);
			if (selectedTransform is null)
			{
				scrollRect.verticalNormalizedPosition = 1;
				return;
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(content);
			float itemLocalYAfter = selectedTransform.localPosition.y;
			content.anchoredPosition = new Vector2(
				content.anchoredPosition.x,
				contentAnchoredYBefore + itemLocalYBefore - itemLocalYAfter);
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
				new(I18NString.FromLocalized("LevelSelect_Sort_ByName"), (a, b, _) =>
					string.Compare(a.Title.Value, b.Title.Value, StringComparison.Ordinal)),
				new(I18NString.FromLocalized("LevelSelect_Sort_ByLevel"), (a, b, diff) =>
				{
					var aVal = a.Difficulties.TryGetValue(diff, out var aLevel) ? aLevel.LevelDisplay : string.Empty;
					var bVal = b.Difficulties.TryGetValue(diff, out var bLevel) ? bLevel.LevelDisplay : string.Empty;
					return GetLevelValue(aVal).CompareTo(GetLevelValue(bVal));
				}),
				new(I18NString.FromLocalized("LevelSelect_Sort_ByScore"), (a, b, diff) =>
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