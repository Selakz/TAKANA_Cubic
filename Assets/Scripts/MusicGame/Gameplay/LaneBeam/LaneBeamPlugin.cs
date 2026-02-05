#nullable enable

using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Basic.T3;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Movement;
using T3Framework.Runtime.Threading;
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
		private readonly ReusableCancellationTokenSource rcts = new();

		private PrefabHandler handler = default!;
		private T3TrackViewPresenter? presenter;
		private Color laneBeamColor;

		// Defined Functions
		public void PlayAnimation()
		{
			texture.gameObject.SetActive(true);
			movement.Move(() => texture.color.a, value => texture.color = texture.color with { a = value });
			rcts.CancelAndReset();
			UniTask.Delay(movement.Length.Milli, cancellationToken: rcts.Token)
				.ContinueWith(() => texture.gameObject.SetActive(false)); // Avoid overdraw
		}

		public void StopAnimation()
		{
			rcts.CancelAndReset();
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