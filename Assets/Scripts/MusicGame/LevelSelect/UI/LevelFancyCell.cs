#nullable enable

using MusicGame.Gameplay.Level;
using MusicGame.Utility.UI;
using T3Framework.Runtime.ECS;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace MusicGame.LevelSelect.UI
{
	public class LevelFancyCell : FancyCell<LevelComponent<GameplayPreference>,
		ViewPoolFancyScrollViewContext<LevelComponent<GameplayPreference>>>
	{
		// Serializable and Public
		[SerializeField] private Animator animator = default!;
		[SerializeField] private string scrollAnimation = default!;
		
		// Private
		private PrefabHandler handler = default!;
		private float currentPosition = 0;
		private int scrollAnimationId;

		public override void Initialize()
		{
			base.Initialize();
			if (!TryGetComponent(out handler)) handler = gameObject.AddComponent<PrefabHandler>();
			Context.View.NotifyCellCreated(handler);
		}

		public override void UpdateContent(LevelComponent<GameplayPreference> itemData)
		{
			Context.View.NotifyCellAssigned(handler, itemData);
		}

		public override void UpdatePosition(float position)
		{
			currentPosition = position;
			if (animator.isActiveAndEnabled) animator.Play(scrollAnimationId, -1, position);
			animator.speed = 0;
		}

		public override void SetVisible(bool visible)
		{
			base.SetVisible(visible);
			if (!visible) Context.View.NotifyCellReleased(handler);
		}

		// System Functions
		void Awake() => scrollAnimationId = Animator.StringToHash(scrollAnimation);

		void OnEnable() => UpdatePosition(currentPosition);
	}
}