#nullable enable

using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Decoration.Note
{
	public class NoteDecorationViewScope : HierarchyLifetimeScope
	{
		[SerializeField] private ClassViewPoolInstaller<T3Flag> decoratorPool = default!;

		protected override void Configure(IContainerBuilder builder)
		{
			base.Configure(builder);

			// Note Decorator
			decoratorPool.Register<ViewPool<ChartComponent, T3Flag>, ChartComponent>(
				builder, Lifetime.Singleton, new T3ChartClassifier()).Keyed("note-decoration");
			builder.RegisterEntryPoint<NoteDecoratorSystem>();
		}
	}
}