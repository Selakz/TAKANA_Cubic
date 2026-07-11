#nullable enable

using MusicGame.Models;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;

namespace MusicGame.Gameplay.Stage
{
	public enum TrackBehaviour
	{
		Instant,
		Falling,
	}

	[CreateAssetMenu(fileName = "Gameplay Stage Skin Config", menuName = "T3GameplayConfig/GameplayStageSkinConfig")]
	public class GameplayStageSkinConfig : ScriptableObject
	{
		public string skinNameLocalized = string.Empty;

		public InspectorDictionary<T3Flag, PrefabObject> prefabs = new();

		public TrackBehaviour trackBehaviour;

		public bool noteWidthAlignWithTrack;

		public PrefabObject laneBeamPrefab = default!;
	}
}