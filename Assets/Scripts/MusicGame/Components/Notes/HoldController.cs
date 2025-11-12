using MusicGame.Components.Chart;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.Components.Notes
{
	public class HoldController : BaseNoteController<Hold>
	{
		// Serializable and Public
		[SerializeField] private SpriteRenderer holdStartSpriteRenderer;

		public Modifier<Sprite> HoldStartSpriteModifier { get; private set; }

		// Private
		private Sprite defaultHoldStartSprite;

		private float Current => LevelManager.Instance.Music.ChartTime;

		protected override float Width
		{
			get => base.Width;
			set
			{
				base.Width = value;
				holdStartSpriteRenderer.size = new(value, holdStartSpriteRenderer.size.y);
			}
		}

		private float Length
		{
			get => spriteRenderer.size.y;
			set
			{
				spriteRenderer.size = new(spriteRenderer.size.x, value);
				boxCollider.size = new(spriteRenderer.size.x, Mathf.Max(value, 0.5f));
				boxCollider.center = new(boxCollider.center.x, value / 2, boxCollider.center.z);
			}
		}

		// Defined Functions
		public override void Destroy()
		{
			// Released to pool
			Model.Controller = null;
			gameObject.SetActive(false);
			gameObject.transform.SetParent(LevelManager.Instance.PoolingStorage);
		}

		// Event Handlers
		private void LevelOnReset(T3Time chartTime)
		{
			if (chartTime < Model.TimeInstantiate ||
			    chartTime > Model.TimeEnd + ISingletonSetting<PlayfieldSetting>.Instance.TimeAfterEnd)
			{
				Model.Destroy();
			}
		}

		private void ChartOnUpdate(ChartInfo chartInfo)
		{
			if (!chartInfo.Contains(Model.Id))
			{
				Model.Destroy();
			}
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			defaultHoldStartSprite = holdStartSpriteRenderer.sprite;

			ScaleModifier = new Modifier<Vector2>(
				() => new(Width, Length),
				scale => (Width, Length) = (scale.x, scale.y),
				_ => new(0, 0));
			SortingOrderModifier = new Modifier<int>(
				() => spriteRenderer.sortingOrder,
				order =>
				{
					spriteRenderer.sortingOrder = order;
					holdStartSpriteRenderer.sortingOrder = order + 1;
				},
				_ => 0);
			HoldStartSpriteModifier = new Modifier<Sprite>(
				() => holdStartSpriteRenderer.sprite,
				sprite => holdStartSpriteRenderer.sprite = sprite,
				_ => defaultHoldStartSprite);
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<T3Time>("Level_OnReset", LevelOnReset);
			EventManager.Instance.AddListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);

			PositionModifier.Register(
				value => LevelManager.Instance.Music.ChartTime < Model.TimeJudge
					? new(value.x, Model.Movement.GetPos(Current) * LevelManager.Instance.LevelSpeed.SpeedRate)
					: new(value.x, 0),
				0);
			ScaleModifier.Register(
				_ =>
				{
					// Width
					var width = Model.Parent.Width;
					var holdTrackGap = ISingletonSetting<PlayfieldSetting>.Instance.TrackGap1;
					width = width > 2 * holdTrackGap ? width - holdTrackGap : width;
					// Length
					var length = LevelManager.Instance.Music.ChartTime <= Model.TimeEnd
						? Model.TailMovement.GetPos(Current) * LevelManager.Instance.LevelSpeed.SpeedRate -
						  PositionModifier.Value.y
						: 0;
					return new(width, length);
				},
				0);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<T3Time>("Level_OnReset", LevelOnReset);
			EventManager.Instance.RemoveListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);

			PositionModifier.Unregister(0);
			ScaleModifier.Unregister(0);
		}

		protected override void Update()
		{
			base.Update();
			ScaleModifier.Update();

			holdStartSpriteRenderer.color = Current > Model.TimeJudge ? Color.clear : ColorModifier.Value;
			if (Current > Model.TimeEnd + ISingletonSetting<PlayfieldSetting>.Instance.TimeAfterEnd)
			{
				Model.Destroy();
			}
		}
	}
}