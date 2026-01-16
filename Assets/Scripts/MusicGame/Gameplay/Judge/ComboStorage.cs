#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Judge.T3;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Judge
{
	public interface IComboFactory
	{
		public IEnumerable<IComboItem> CreateCombo(ChartComponent component);
	}

	public class ComboStorage : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		public event Action? OnComboReset;

		public IReadOnlyList<IComboItem> Combos => combos;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				combos.Clear();
				var info = levelInfo.Value;
				if (info is not null) FormCombos(info.Chart);
				OnComboReset?.Invoke();
			})
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private IComboFactory comboFactory = default!;

		private readonly List<IComboItem> combos = new();

		// Constructor
		[Inject]
		private void Construct(
			NotifiableProperty<LevelInfo?> levelInfo,
			IComboFactory comboFactory)
		{
			this.levelInfo = levelInfo;
			this.comboFactory = comboFactory;
		}

		public void SelfInstall(IContainerBuilder builder)
		{
			// TODO: a way to specify implementation in hierarchy
			builder.Register<IComboFactory, T3ComboFactory>(Lifetime.Singleton);
			builder.RegisterComponent(this).AsSelf();
		}

		// Defined Functions
		private void FormCombos(ChartInfo chart)
		{
			foreach (var component in chart)
			{
				var comboItems = comboFactory.CreateCombo(component);
				combos.AddRange(comboItems);
			}

			combos.Sort((a, b) => a.ExpectedTime.CompareTo(b.ExpectedTime));
		}

		public int GetLowerBoundIndex(T3Time time)
		{
			int left = 0;
			int right = combos.Count;
			while (left < right)
			{
				int mid = left + (right - left) / 2;
				if (combos[mid].ExpectedTime < time) left = mid + 1;
				else right = mid;
			}

			return left;
		}
	}
}