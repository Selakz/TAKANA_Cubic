#nullable enable

using Cysharp.Threading.Tasks;
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

		public float Width
		{
			get => transform.localScale.x;
			set => transform.localScale = new(value, transform.localScale.y);
		}

		// Private
		private readonly ReusableCancellationTokenSource rcts = new();
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
		void OnEnable()
		{
			transform.localPosition = Vector3.zero;
		}

		void OnDisable() => StopAnimation();
	}
}