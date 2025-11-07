#nullable enable

using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.Utility.Setting
{
	public class ColorListSettingItem : ListSettingItem<Color?, ColorListItemSettingItem>
	{
		protected override LazyPrefab ListItemPrefab =>
			new("Prefabs/EditorUI/Setting/ColorListItemSettingItem", "ColorListItemSettingItemPrefab_OnLoad");
	}
}