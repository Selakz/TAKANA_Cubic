#nullable enable

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Audio;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Threading;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Judge
{
	public class JudgeStorage : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		public IReadOnlyDictionary<IComboItem, IJudgeItem> JudgeItems => judgeItems;

		public event Action<IJudgeItem>? OnJudgeItemAdded;
		public event Action? OnJudgeItemCleared;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<Action>(
				e => comboStorage.OnComboReset += e,
				e => comboStorage.OnComboReset -= e,
				() =>
				{
					rcts.CancelAndReset();
					judgeItems.Clear();
					OnJudgeItemCleared?.Invoke();
				})
		};

		// Private
		private ComboStorage comboStorage = default!;
		private GameAudioPlayer music = default!;

		private readonly ReusableCancellationTokenSource rcts = new(true);
		private readonly Dictionary<IComboItem, IJudgeItem> judgeItems = new();
		private readonly Dictionary<IComboItem, IJudgeItem> schedulingJudgeItems = new();

		// Constructor
		[Inject]
		private void Construct(
			ComboStorage comboStorage,
			GameAudioPlayer music)
		{
			this.comboStorage = comboStorage;
			this.music = music;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this).AsSelf();

		// Defined Functions
		public bool ContainsOrToContain(IComboItem comboItem)
		{
			return judgeItems.ContainsKey(comboItem) || schedulingJudgeItems.ContainsKey(comboItem);
		}

		public void AddJudgeItem(IJudgeItem judgeItem)
		{
			if (judgeItems.TryAdd(judgeItem.ComboItem, judgeItem))
			{
				OnJudgeItemAdded?.Invoke(judgeItem);
			}
			else
			{
				Debug.LogWarning(
					$"Duplicate to add judge item. combo time: {judgeItem.ComboItem.ExpectedTime}, component id: {judgeItem.ComboItem.FromComponent.Id}");
			}
		}

		public void AddJudgeItemScheduled(IJudgeItem judgeItem, T3Time chartTime)
		{
			var current = music.ChartTime;
			if (current >= chartTime)
			{
				AddJudgeItem(judgeItem);
			}
			else
			{
				schedulingJudgeItems.Add(judgeItem.ComboItem, judgeItem);
				UniTask.Delay((chartTime - current).Milli, cancellationToken: rcts.Token)
					.ContinueWith(() =>
					{
						schedulingJudgeItems.Remove(judgeItem.ComboItem);
						AddJudgeItem(judgeItem);
					});
			}
		}

		// System Functions
		protected override void OnDestroy()
		{
			base.OnDestroy();
			rcts.Dispose();
		}
	}
}