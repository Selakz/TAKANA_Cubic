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
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class GridWidthRetriever : T3MonoBehaviour, IWidthRetriever, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Transform widthGridRoot = default!;
		[SerializeField] private PrefabObject gridPrefab = default!;

		public event Action OnBeforeResetGrid = delegate { };

		public NotifiableProperty<bool> IsOn { get; } = new(false);

		public NotifiableProperty<float> GridInterval { get; } = new(1.0f);

		public NotifiableProperty<float> GridOffset { get; } = new(0f);

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				var info = levelInfo.Value;
				if (info?.Preference is EditorPreference preference)
				{
					GridOffset.Value = preference.WidthGridOffset;
					GridInterval.Value = preference.WidthGridInterval;
				}

				ResetGrid();
			}),
			new PropertyRegistrar<bool>(IsOn, isOn =>
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
			}),
			new PropertyRegistrar<float>(GridInterval, value =>
			{
				if (levelInfo.Value?.Preference is EditorPreference preference) preference.WidthGridInterval = value;
				ResetGrid();
			}),
			new PropertyRegistrar<float>(GridOffset, value =>
			{
				if (levelInfo.Value?.Preference is EditorPreference preference) preference.WidthGridOffset = value;
				ResetGrid();
			})
		};

		// Private
		private IObjectResolver resolver = default!;
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private NotifiableProperty<IWidthRetriever> widthRetriever = default!;

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

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this).AsSelf();

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
	}
}