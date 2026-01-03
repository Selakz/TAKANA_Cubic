#nullable enable

using UnityEngine;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	public class HitEffectAnimator : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Animator animator = default!;

		// Defined Functions
		public void PlayComboAnimation()
		{
			animator.gameObject.SetActive(true);
			animator.Play(0);
		}

		public void StopComboAnimation()
		{
			animator.gameObject.SetActive(false);
		}
	}
}