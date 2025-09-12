#nullable enable

using MusicGame.Components.Tracks;
using UnityEngine;

namespace MusicGame.Gameplay.LaneBeam
{
	public class LaneBeamHandler : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Animator animator = default!;

		// Private
		private TrackController trackController = default!;

		// Defined Functions
		public void PlayAnimation()
		{
			animator.gameObject.SetActive(true);
			animator.Play(0);
		}

		public void StopAnimation()
		{
			animator.gameObject.SetActive(false);
		}

		// System Functions
		void Awake()
		{
			trackController = GetComponentInParent<TrackController>();
			animator.gameObject.SetActive(false);
		}

		void Update()
		{
			transform.localScale = new(trackController.ScaleModifier.Value.x, transform.localScale.y);
			if (trackController.IsHidden && animator.gameObject.activeSelf)
			{
				animator.gameObject.SetActive(false);
			}
		}
	}
}