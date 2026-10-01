#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Stage
{
	public class StageViewPool<TClass> : ViewPool<ChartComponent, TClass> where TClass : struct
	{
		public StageViewPool(
			IObjectResolver resolver,
			[Key("stage")] IClassifier<TClass> classifier,
			[Key("stage")] Dictionary<TClass, PrefabObject> prefabs,
			[Key("stage")] Transform defaultTransform) :
			base(resolver, classifier, prefabs, defaultTransform)
		{
		}
	}
}