#nullable enable

using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;

#if UNITY_IOS
using System.IO;
using UnityEngine;
#endif

namespace MusicGame.LevelSelect
{
	public class LevelSetting : ISingletonSetting<LevelSetting>
	{
		[HideInGame]
		public NotifiableProperty<string> StoragePath { get; set; } = new(
#if UNITY_ANDROID && !UNITY_EDITOR
			"/storage/emulated/0/data/takana"
#elif UNITY_IOS && !UNITY_EDITOR
			Path.Combine(Application.persistentDataPath, "Levels")
#else
			@"G:\TAKANACharts"
#endif
		);
	}
}