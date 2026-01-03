#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.Models;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;

namespace MusicGame.Gameplay.SortingOrder
{
	[CreateAssetMenu(fileName = "SpriteSortingOrderConfig", menuName = "ScriptableObjects/Sprite Sorting Order Config")]
	public class SpriteSortingOrderMapConfig : ScriptableObject
	{
		[Tooltip("Key: sorting order name; Value: sprite name in IModelViewPresenter")] [SerializeField]
		private InspectorDictionary<T3Flag, InspectorDictionary<string, string>> sortingOrderMap = new();

		private Dictionary<T3Flag, Dictionary<string, string>>? map;

		public Dictionary<T3Flag, Dictionary<string, string>> Value => map ??= new(sortingOrderMap.Value.Select(
			pair => new KeyValuePair<T3Flag, Dictionary<string, string>>(pair.Key, pair.Value.Value)));
	}
}