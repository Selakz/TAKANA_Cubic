#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.MVC;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.Components.Notes
{
	public abstract class BaseNoteController<T> : MonoBehaviour, IModifiableView2D, IController<T> where T : BaseNote
	{
		// Serializable and Public
		[SerializeField] protected SpriteRenderer spriteRenderer = default!;
		[SerializeField] protected BoxCollider2D boxCollider = default!;

		public T Model { get; private set; } = default!;

		public IModel GenericModel => Model;

		public GameObject Object => gameObject;

		public Modifier<Vector2> PositionModifier { get; private set; } = default!;

		public Modifier<Vector2> ScaleModifier { get; protected set; } = default!;

		public Modifier<float> RotationModifier { get; private set; } = default!;
		public Modifier<Sprite> SpriteModifier { get; private set; } = default!;

		public Modifier<Color> ColorModifier { get; private set; } = default!;

		public Modifier<int> SortingOrderModifier { get; protected set; } = default!;

		// Private
		private Sprite defaultSprite = default!;

		private Color SpriteColor
		{
			get => spriteRenderer.color;
			set => spriteRenderer.color = value;
		}

		private Vector2 Position
		{
			get => transform.localPosition;
			set
			{
				Model.Position = value;
				transform.localPosition = value;
			}
		}

		protected virtual float Width
		{
			get => spriteRenderer.size.x;
			set
			{
				spriteRenderer.size = new(value, spriteRenderer.size.y);
				boxCollider.size = new(value, boxCollider.size.y);
			}
		}

		// Defined Functions
		public virtual void Init(T model)
		{
			Model = model;
			if (Model.Parent is IControllerRetrievable<MonoBehaviour> r)
			{
				var parentTransform = r.Controller.transform;
				var childrenRoot = parentTransform.Find("ChildrenRoot");
				transform.SetParent(childrenRoot, false);
			}
		}

		public abstract void Destroy();

		// System Functions
		protected virtual void Awake()
		{
			defaultSprite = spriteRenderer.sprite;
			SpriteModifier = new Modifier<Sprite>(
				() => spriteRenderer.sprite,
				sprite => spriteRenderer.sprite = sprite,
				_ => defaultSprite);
			ScaleModifier = new Modifier<Vector2>(
				() => new(Width, 1),
				scale => Width = scale.x,
				_ => new(0, 0));
			// TODO: RotationModifier
			ColorModifier = new Modifier<Color>(
				() => SpriteColor,
				color => SpriteColor = color,
				_ => Color.white);
			PositionModifier = new Modifier<Vector2>(
				() => Position,
				position => Position = position,
				_ => new(0, ISingletonSetting<PlayfieldSetting>.Instance.UpperThreshold + 1));
			SortingOrderModifier = new Modifier<int>(
				() => spriteRenderer.sortingOrder,
				order => spriteRenderer.sortingOrder = order,
				_ => 0);
		}

		protected virtual void Update()
		{
			SpriteModifier.Update();
			ScaleModifier.Update();
			// RotationModifier.Update();
			ColorModifier.Update();
			PositionModifier.Update();
			SortingOrderModifier.Update();
		}
	}
}