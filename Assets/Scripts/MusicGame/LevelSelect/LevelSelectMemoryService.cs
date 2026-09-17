#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;

namespace MusicGame.LevelSelect
{
	public interface ILevelSelectMemoryService
	{
		public void SyncLevelInfo(RawLevelInfo<GameplayPreference>? info);

		public void SyncDifficulty(int diff);

		public void SyncPack(PackInfo pack);

		public void SyncSort(SortMethod sort, bool isAscend);

		public void Save();
	}

	public class LevelSelectMemoryService : HierarchySystem<LevelSelectMemoryService>, ILevelSelectMemoryService
	{
		// Serializable and Public
		public override bool AsImplementedInterfaces => true;

		// Private
		[Inject] private NotifiableProperty<bool> levelsLoaded = default!;

		// Defined Functions
		public void SyncLevelInfo(RawLevelInfo<GameplayPreference>? info)
		{
			if (!levelsLoaded.Value || info is null) return;
			var setting = ISingletonSetting<LevelSelectSetting>.Instance;
			setting.LastSongId.Value = info.SongInfo.Value?.Id ?? string.Empty;
			setting.LastLevelPath.Value = info.LevelPath;
		}

		public void SyncDifficulty(int diff)
		{
			if (!levelsLoaded.Value) return;
			ISingletonSetting<LevelSelectSetting>.Instance.LastDifficulty.Value = diff;
		}

		public void SyncPack(PackInfo pack)
		{
			if (!levelsLoaded.Value) return;
			ISingletonSetting<LevelSelectSetting>.Instance.LastPackId.Value = pack.Id;
		}

		public void SyncSort(SortMethod sort, bool isAscend)
		{
			if (!levelsLoaded.Value) return;
			var setting = ISingletonSetting<LevelSelectSetting>.Instance;
			setting.LastSortId.Value = sort.Id;
			setting.LastSortAscend.Value = isAscend;
		}

		public void Save() => ISingletonSetting<LevelSelectSetting>.SaveInstance();

		// System Functions
		protected override void OnDisable()
		{
			base.OnDisable();
			Save();
		}
	}
}
