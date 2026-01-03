#nullable enable

using System;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class GridWidthRetriever : T3MonoBehaviour, IWidthRetriever, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Toggle toggle = default!;
		[SerializeField] private TMP_InputField gridIntervalInputField = default!;
		[SerializeField] private TMP_InputField gridOffsetInputField = default!;
		[SerializeField] private Transform widthGridRoot = default!;
		[SerializeField] private PrefabObject gridPrefab = default!;

		public event Action OnBeforeResetGrid = delegate { };

		public float GridInterval
		{
			get => gridInterval;
			set
			{
				gridInterval = Mathf.Max(0.1f, value);
				gridIntervalInputField.text = gridInterval.ToString("0.000");
				if (levelInfo.Value?.Preference is EditorPreference preference)
				{
					preference.WidthGridInterval = value;
				}

				ResetGrid();
			}
		}

		public float GridOffset
		{
			get => gridOffset;
			set
			{
				gridOffset = value;
				gridOffsetInputField.text = gridOffset.ToString("0.000");
				if (levelInfo.Value?.Preference is EditorPreference preference)
				{
					preference.WidthGridOffset = value;
				}

				ResetGrid();
			}
		}

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				var info = levelInfo.Value;
				if (info?.Preference is EditorPreference preference)
				{
					GridOffset = preference.WidthGridOffset;
					GridInterval = preference.WidthGridInterval;
				}

				ResetGrid();
			})
		};

		// Private
		private IObjectResolver resolver = default!;
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private NotifiableProperty<IWidthRetriever> widthRetriever = default!;

		private float gridInterval = 1.5f;
		private float gridOffset = 0f;

		private ObjectPool<WidthGridController> WidthGridPool => widthGridPool ??= new(
			() => gridPrefab.Instantiate(resolver, widthGridRoot).GetComponent<WidthGridController>(),
			grid => grid.gameObject.SetActive(true),
			grid => grid.gameObject.SetActive(false),
			grid => Destroy(grid.gameObject));

		private ObjectPool<WidthGridController>? widthGridPool;

		// Static
		private const float GameWidth = 6f;

		// Defined Functions
		[Inject]
		private void Construct(
			IObjectResolver resolver,
			NotifiableProperty<LevelInfo?> levelInfo,
			NotifiableProperty<IWidthRetriever> widthRetriever)
		{
			this.resolver = resolver;
			this.levelInfo = levelInfo;
			this.widthRetriever = widthRetriever;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		public float GetWidth(Vector3 position)
		{
			int lineCount = Mathf.FloorToInt((position.x - GridOffset) / GridInterval);
			float left = GridOffset + lineCount * GridInterval;
			float right = GridOffset + (lineCount + 1) * GridInterval;
			return Mathf.Abs(left - right);
		}

		public float GetPosition(Vector3 position)
		{
			int lineCount = Mathf.FloorToInt((position.x - GridOffset) / GridInterval);
			float left = GridOffset + lineCount * GridInterval;
			float right = GridOffset + (lineCount + 1) * GridInterval;
			return (left + right) / 2;
		}

		public float GetAttachedPosition(Vector3 position)
		{
			int lineCount = Mathf.FloorToInt((position.x - GridOffset) / GridInterval);
			float left = GridOffset + lineCount * GridInterval;
			float right = GridOffset + (lineCount + 1) * GridInterval;
			float leftDistance = Mathf.Abs(left - position.x);
			float rightDistance = Mathf.Abs(right - position.x);
			return leftDistance < rightDistance ? left : right;
		}

		public float GetLeftAttachedPosition(float x)
		{
			int lineCount = Mathf.FloorToInt((x - GridOffset) / GridInterval);
			var position = GridOffset + lineCount * GridInterval;
			if (Mathf.Approximately(position, x))
			{
				position = GridOffset + (lineCount - 1) * GridInterval;
			}

			return position;
		}

		public float GetRightAttachedPosition(float x)
		{
			int lineCount = Mathf.FloorToInt((x - GridOffset) / GridInterval);
			var position = GridOffset + (lineCount + 1) * GridInterval;
			if (Mathf.Approximately(position, x))
			{
				position = GridOffset + (lineCount + 2) * GridInterval;
			}

			return position;
		}

		public void ReleaseWidthGrid(WidthGridController widthGrid)
		{
			WidthGridPool.Release(widthGrid);
		}

		public void ResetGrid()
		{
			OnBeforeResetGrid.Invoke();
			float left = GridOffset, right = GridOffset + GridInterval;
			while (left > -GameWidth)
			{
				var widthGrid = WidthGridPool.Get();
				widthGrid.transform.SetParent(widthGridRoot);
				widthGrid.Init(this, left);
				left -= GridInterval;
			}

			while (right < GameWidth)
			{
				var widthGrid = WidthGridPool.Get();
				widthGrid.transform.SetParent(widthGridRoot);
				widthGrid.Init(this, right);
				right += GridInterval;
			}
		}

		// Event Handlers
		private void OnToggleChanged(bool isOn)
		{
			if (isOn)
			{
				widthGridRoot.gameObject.SetActive(true);
				widthRetriever.Value = this;
				ResetGrid();
			}
			else
			{
				widthGridRoot.gameObject.SetActive(false);
				widthRetriever.Value = DefaultWidthRetriever.Instance;
			}
		}

		private void OnGridIntervalInputFieldEndEdit(string content)
		{
			if (float.TryParse(content, out float interval))
			{
				GridInterval = interval;
			}

			gridIntervalInputField.SetTextWithoutNotify(GridInterval.ToString("0.000"));
		}

		private void OnGridOffsetInputFieldEndEdit(string content)
		{
			if (float.TryParse(content, out float offset))
			{
				GridOffset = offset;
			}

			gridOffsetInputField.SetTextWithoutNotify(GridOffset.ToString("0.000"));
		}

		// System Functions
		protected override void Awake()
		{
			toggle.onValueChanged.AddListener(OnToggleChanged);
			gridIntervalInputField.onEndEdit.AddListener(OnGridIntervalInputFieldEndEdit);
			gridOffsetInputField.onEndEdit.AddListener(OnGridOffsetInputFieldEndEdit);
		}

		protected override void OnEnable()
		{
			OnToggleChanged(toggle.isOn);
		}
	}
}