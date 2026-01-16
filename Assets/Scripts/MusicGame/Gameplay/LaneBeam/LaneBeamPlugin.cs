#nullable enable

using MusicGame.Gameplay.Basic.T3;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Movement;
using UnityEngine;

namespace MusicGame.Gameplay.LaneBeam
{
	public class LaneBeamPlugin : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private FloatMovementContainer movement = default!;
		[SerializeField] private SpriteRenderer texture = default!;

		public Color LaneBeamColor
		{
			get => laneBeamColor;
			set
			{
				laneBeamColor = value;
				texture.color = laneBeamColor;
			}
		}

		// Private
		private PrefabHandler handler = default!;
		private T3TrackViewPresenter? presenter;
		private Color laneBeamColor;

		// Defined Functions
		public void PlayAnimation()
		{
			texture.gameObject.SetActive(true);
			movement.Move(() => texture.color.a, value => texture.color = texture.color with { a = value });
		}

		public void StopAnimation()
		{
			texture.gameObject.SetActive(false);
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