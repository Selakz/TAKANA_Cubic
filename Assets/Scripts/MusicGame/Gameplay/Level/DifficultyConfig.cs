#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;

namespace MusicGame.Gameplay.Level
{
	[System.Serializable]
	public struct DifficultyData
	{
		public string name;
		public Color color;
	}

	[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "T3GameplayConfig/DifficultyConfig")]
	public class DifficultyConfig : ScriptableObject
	{
		[SerializeField] private InspectorDictionary<int, DifficultyData> difficulties = default!;

		public Dictionary<int, DifficultyData> Value => difficulties.Value;
	}
}