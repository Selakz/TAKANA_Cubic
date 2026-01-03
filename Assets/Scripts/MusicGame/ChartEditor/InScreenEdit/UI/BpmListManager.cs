#nullable enable

using System.Collections.Generic;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit.UI
{
	public class BpmListManager : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private PrefabObject contentPrefab = default!;
		[SerializeField] private Transform contentRoot = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				var lastInfo = levelInfo.LastValue;
				if (lastInfo is not null) lastInfo.GetsBpmList().OnBpmListUpdate -= UpdateUI;
				var info = levelInfo.Value;
				if (info is not null)
				{
					info.GetsBpmList().OnBpmListUpdate += UpdateUI;
					UpdateUI();
				}
				else
				{
					viewPool.Clear();
				}
			})
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private ViewPool<BaseComponent<KeyValuePair<T3Time, float>>> viewPool = default!;
		private ListViewAutoSorter<BaseComponent<KeyValuePair<T3Time, float>>> sorter = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			IObjectResolver resolver,
			NotifiableProperty<LevelInfo?> levelInfo)
		{
			this.levelInfo = levelInfo;
			viewPool = new(resolver, contentPrefab, contentRoot);
			sorter = new(viewPool)
			{
				ListSorter = (a, b) => a.Model.Key.CompareTo(b.Model.Key),
			};
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		private void UpdateUI()
		{
			foreach (var component in viewPool)
			{
				var handler = viewPool[component];
				var content = handler!.Script<EditBpmContent>();
				content.OnBpmChanged -= OnBpmChanged;
			}

			viewPool.Clear();
			if (levelInfo.Value is not { } info) return;
			foreach (var pair in info.GetsBpmList())
			{
				var component = new BaseComponent<KeyValuePair<T3Time, float>>(pair);
				if (viewPool.Add(component))
				{
					var handler = viewPool[component];
					var content = handler!.Script<EditBpmContent>();
					content.Init(pair.Key, pair.Value);
					content.OnBpmChanged += OnBpmChanged;
				}
			}
		}

		// Event Handlers
		private void OnBpmChanged(T3Time oldTime, T3Time newTime, float bpm)
		{
			if (levelInfo.Value?.GetsBpmList() is not { } list) return;
			if (oldTime != T3Time.MinValue) list.Remove(oldTime);
			if (newTime != T3Time.MinValue) list.Add(newTime, bpm);
		}

		// System Functions
		protected override void OnDestroy()
		{
			base.OnDestroy();
			sorter.Dispose();
			viewPool.Dispose();
		}
	}
}