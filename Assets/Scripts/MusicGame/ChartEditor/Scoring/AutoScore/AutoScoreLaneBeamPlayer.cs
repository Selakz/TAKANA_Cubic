#nullable enable

using MusicGame.Components.Tracks;
using MusicGame.Gameplay.LaneBeam;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.ChartEditor.Scoring.AutoScore
{
	public class AutoScoreLaneBeamPlayer : MonoBehaviour
	{
		// Private
		private TrackController trackController = default!;
		private LaneBeamHandler laneBeamHandler = default!;

		// Event Handlers
		private void LevelOnReset(T3Time chartTime)
		{
			laneBeamHandler.StopAnimation();
		}

		private void AutoScoreOnPlayLaneBeam(int trackId)
		{
			if (trackId == trackController.Model.Id)
			{
				laneBeamHandler.PlayAnimation();
			}
		}

		// System Functions
		void Awake()
		{
			trackController = GetComponentInParent<TrackController>();
			laneBeamHandler = GetComponent<LaneBeamHandler>();
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<int>("AutoScore_OnPlayLaneBeam", AutoScoreOnPlayLaneBeam);
			EventManager.Instance.AddListener<T3Time>("Level_OnReset", LevelOnReset);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<int>("AutoScore_OnPlayLaneBeam", AutoScoreOnPlayLaneBeam);
			EventManager.Instance.RemoveListener<T3Time>("Level_OnReset", LevelOnReset);
		}
	}
}