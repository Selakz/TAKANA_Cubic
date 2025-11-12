#nullable enable

using MusicGame.Components.Chart;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.MVC;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.Components.Tracks
{
	public class TrackController : MonoBehaviour, IModifiableView2D, IColliderView, IController<Track>
	{
		// Serializable and Public
		[SerializeField] private SpriteRenderer trackFace = default!;
		[SerializeField] private Transform sprite = default!;
		[SerializeField] private Transform leftLine = default!;
		[SerializeField] private Transform rightLine = default!;
		[SerializeField] private BoxCollider boxCollider = default!;
		[SerializeField] private Transform childrenRoot = default!;

		public Track Model { get; private set; } = default!;

		public IModel GenericModel => Model;

		public GameObject Object => gameObject;

		public Modifier<Vector2> PositionModifier { get; private set; } = default!;

		public Modifier<Vector2> ScaleModifier { get; private set; } = default!;

		public Modifier<float> RotationModifier { get; private set; } = default!;

		public Modifier<Sprite> SpriteModifier { get; private set; } = default!;

		public Modifier<Color> ColorModifier { get; private set; } = default!;

		public Modifier<int> SortingOrderModifier { get; private set; } = default!;

		public Modifier<bool> ColliderEnabledModifier { get; private set; } = default!;

		public float GameWidth
		{
			get => sprite.localScale.x;
			private set
			{
				Model.Width = value;
				sprite.localScale = new(value, sprite.localScale.y);
				leftLine.localPosition = new(-value / 2, 0);
				rightLine.localPosition = new(value / 2, 0);
				boxCollider.size = new(Mathf.Max(value, 0.5f), boxCollider.size.y, boxCollider.size.z);
			}
		}

		public float GamePos
		{
			get => transform.localPosition.x;
			private set
			{
				Model.Position = new(value, 0);
				transform.localPosition = new(value, 0);
			}
		}

		public bool IsHidden
		{
			get => isHidden;
			set
			{
				if (isHidden == value) return;
				isHidden = value;
				sprite.gameObject.SetActive(!value);
				leftLine.gameObject.SetActive(!value);
				rightLine.gameObject.SetActive(!value);
				ColliderEnabledModifier.Register(enabled => !value && enabled, int.MaxValue);
			}
		}

		// Private
		private static float Current => LevelManager.Instance.Music.ChartTime;
		private bool isHidden = false;
		private Sprite defaultTrackFaceSprite = default!;

		// Defined Functions
		public void Init(Track model)
		{
			Model = model;
			if (Model.Parent is IControllerRetrievable<MonoBehaviour> r)
			{
				var parentTransform = r.Controller.transform;
				var parentChildrenRoot = parentTransform.Find("ChildrenRoot");
				transform.SetParent(parentChildrenRoot, false);
			}
		}

		public void Destroy()
		{
			// Released to pool
			foreach (Transform children in childrenRoot)
			{
				if (children.TryGetComponent(out IModelRetrievable r))
				{
					r.GenericModel.Destroy();
				}
			}

			Model.Controller = null;
			gameObject.SetActive(false);
			gameObject.transform.SetParent(LevelManager.Instance.PoolingStorage);
		}

		private void SpriteInit()
		{
			sprite.localScale = new(1f, 30f);
			leftLine.localScale = new(0.015f, 30f);
			rightLine.localScale = new(0.015f, 30f);
			boxCollider.size = new(1f, 30f, boxCollider.size.z);
			IsHidden = Model.TimeInstantiate > Current || Current > Model.TimeEnd;
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
		void Awake()
		{
			PositionModifier = new Modifier<Vector2>(
				() => new(GamePos, 0),
				position => GamePos = position.x,
				_ => new());
			ScaleModifier = new Modifier<Vector2>(
				() => new(GameWidth, 0),
				scale => GameWidth = scale.x,
				_ => new());
			// TODO: RotationModifier
			defaultTrackFaceSprite = trackFace.sprite;
			SpriteModifier = new Modifier<Sprite>(
				() => trackFace.sprite,
				value => trackFace.sprite = value,
				_ => defaultTrackFaceSprite
			);
			ColorModifier = new Modifier<Color>(
				() => trackFace.color,
				value => trackFace.color = value,
				_ => ISingletonSetting<PlayfieldSetting>.Instance.TrackFaceDefaultColor);
			SortingOrderModifier = new Modifier<int>(
				() => trackFace.sortingOrder,
				value => trackFace.sortingOrder = value,
				_ => 0);
			ColliderEnabledModifier = new Modifier<bool>(
				() => boxCollider.enabled,
				value =>
				{
					if (value == boxCollider.enabled) return;
					boxCollider.enabled = value;
				},
				_ => true);

			SpriteInit();
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<T3Time>("Level_OnReset", LevelOnReset);
			EventManager.Instance.AddListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);

			PositionModifier.Register(
				_ => new(Model.Movement.GetPos(Current), 0),
				0);
			ScaleModifier.Register(
				_ => new(Model.Movement.GetWidth(Current), 0),
				0);
			Update();
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<T3Time>("Level_OnReset", LevelOnReset);
			EventManager.Instance.RemoveListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
		}

		void Update()
		{
			PositionModifier.Update();
			ScaleModifier.Update();
			// RotationModifier.Update();
			SpriteModifier.Update();
			ColorModifier.Update();
			SortingOrderModifier.Update();
			// TODO: Encapsulate this into a modifier? split trackLine and trackFace's controller?
			IsHidden = Model.TimeInstantiate > Current || Current > Model.TimeEnd;

			if (Current > Model.TimeEnd + ISingletonSetting<PlayfieldSetting>.Instance.TimeAfterEnd)
			{
				Model.Destroy();
			}
		}
	}
}