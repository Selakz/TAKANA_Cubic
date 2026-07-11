#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Select;
using MusicGame.Models;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;

namespace MusicGame.Gameplay.Stage
{
	[CreateAssetMenu(fileName = "Editor Stage Skin Config", menuName = "T3GameplayConfig/EditorStageSkinConfig")]
	public class EditorStageSkinConfig : GameplayStageSkinConfig
	{
		// for selected view
		[SerializeField] private InspectorDictionary<T3Flag, InspectorDictionary<string, Sprite>> sprites = new();

		public Dictionary<T3Flag, Dictionary<string, Sprite>> SelectedSprites => selectedSprites ??=
			new(sprites.Value.Select(pair =>
				new KeyValuePair<T3Flag, Dictionary<string, Sprite>>(pair.Key, pair.Value.Value)));

		private Dictionary<T3Flag, Dictionary<string, Sprite>>? selectedSprites;

		// for select collider
		public InspectorDictionary<T3Flag, TextureAlignInfo> textureAlignInfos = new();
	}
}