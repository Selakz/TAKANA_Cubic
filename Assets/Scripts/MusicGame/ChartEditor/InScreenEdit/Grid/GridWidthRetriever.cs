using System;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class GridWidthRetriever : MonoBehaviour, IWidthRetriever
	{
		// Serializable and Public
		[SerializeField] private Toggle toggle;
		[SerializeField] private TMP_InputField gridIntervalInputField;
		[SerializeField] private TMP_InputField gridOffsetInputField;
		[SerializeField] private Transform widthGridRoot;

		public event Action OnBeforeResetGrid = delegate { };

		public float GridInterval
		{
			get => gridInterval;
			set
			{
				gridInterval = Mathf.Max(0.1f, value);
				gridIntervalInputField.text = gridInterval.ToString("0.000");
				if (LevelManager.Instance.LevelInfo.Preference is EditorPreference preference)
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
				if (LevelManager.Instance.LevelInfo.Preference is EditorPreference preference)
				{
					preference.WidthGridOffset = value;
				}

				ResetGrid();
			}
		}

		// Private
		private float gridInterval = 1.5f;
		private float gridOffset = 0f;

		private readonly ObjectPool<WidthGridController> widthGridPool = new(
			() => Instantiate<GameObject>(lazyPrefab).GetComponent<WidthGridController>(),
			grid => grid.gameObject.SetActive(true),
			grid => grid.gameObject.SetActive(false),
			grid => Destroy(grid.gameObject));

		// Static
		private const float GameWidth = 6f;
		private static LazyPrefab lazyPrefab;

		// Defined Functions
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
			widthGridPool.Release(widthGrid);
		}

		public void ResetGrid()
		{
			OnBeforeResetGrid.Invoke();
			float left = GridOffset, right = GridOffset + GridInterval;
			while (left > -GameWidth)
			{
				var widthGrid = widthGridPool.Get();
				widthGrid.transform.SetParent(widthGridRoot);
				widthGrid.Init(this, left);
				left -= GridInterval;
			}

			while (right < GameWidth)
			{
				var widthGrid = widthGridPool.Get();
				widthGrid.transform.SetParent(widthGridRoot);
				widthGrid.Init(this, right);
				right += GridInterval;
			}
		}

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			if (levelInfo.Preference is EditorPreference preference)
			{
				GridOffset = preference.WidthGridOffset;
				GridInterval = preference.WidthGridInterval;
			}

			ResetGrid();
		}

		private void OnToggleChanged(bool isOn)
		{
			if (isOn)
			{
				widthGridRoot.gameObject.SetActive(true);
				InScreenEditManager.Instance.WidthRetriever = this;
				ResetGrid();
			}
			else
			{
				widthGridRoot.gameObject.SetActive(false);
				InScreenEditManager.Instance.WidthRetriever = InScreenEditManager.FallbackWidthRetriever;
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
		void Awake()
		{
			toggle.onValueChanged.AddListener(OnToggleChanged);
			gridIntervalInputField.onEndEdit.AddListener(OnGridIntervalInputFieldEndEdit);
			gridOffsetInputField.onEndEdit.AddListener(OnGridOffsetInputFieldEndEdit);
			lazyPrefab ??= new("Prefabs/EditorUI/InScreenEdit/WidthGrid", "WidthGridPrefab_OnLoad");
		}

		void OnEnable()
		{
			OnToggleChanged(toggle.isOn);
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}
	}
}