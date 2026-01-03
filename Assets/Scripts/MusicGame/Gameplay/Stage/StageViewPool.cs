#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Runtime.ECS;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Stage
{
	public class StageViewPool : ViewPool<ChartComponent, T3Flag>
	{
		public StageViewPool(
			IObjectResolver resolver,
			[Key("stage")] IClassifier<T3Flag> classifier,
			[Key("stage")] Dictionary<T3Flag, PrefabObject> prefabs,
			[Key("stage")] Transform defaultTransform) :
			base(resolver, classifier, prefabs, defaultTransform)
		{
		}
	}
}