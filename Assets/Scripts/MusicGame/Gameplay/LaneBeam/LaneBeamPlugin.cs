#nullable enable

using MusicGame.Gameplay.Basic.T3;
using T3Framework.Runtime.ECS;
using UnityEngine;

namespace MusicGame.Gameplay.LaneBeam
{
	public class LaneBeamPlugin : MonoBehaviour
	{
		// Serializable and Public
		// TODO: Replace animator with lightweight DOTWeen easing
		[SerializeField] private Animator animator = default!;

		// Private
		private PrefabHandler handler = default!;
		private T3TrackViewPresenter? presenter;

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
			handler = GetComponent<PrefabHandler>();
		}

		void OnEnable()
		{
			presenter = handler.Parent!.Script<T3TrackViewPresenter>();
			transform.localPosition = Vector3.zero;
		}

		void OnDisable() => StopAnimation();

		void Update()
		{
			if (presenter is null) return;
			transform.localScale = new(presenter.WidthModifier.Value, transform.localScale.y);
		}
	}
}