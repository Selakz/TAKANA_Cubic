#nullable enable

using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.Gameplay.LaneBeam
{
	public class LaneBeamManager : MonoBehaviour
	{
		// Private
		private LazyPrefab laneBeamPrefab = default!;

		// Event Handlers
		private void TrackPrefabOnLoad(GameObject prefab)
		{
			laneBeamPrefab.Instantiate(prefab.transform);
		}

		// System Functions
		void Awake()
		{
			laneBeamPrefab = new("Prefabs/LaneBeam", "LaneBeamPrefab_OnLoad");
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<GameObject>("TrackPrefab_OnLoad", TrackPrefabOnLoad);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<GameObject>("TrackPrefab_OnLoad", TrackPrefabOnLoad);
		}
	}
}