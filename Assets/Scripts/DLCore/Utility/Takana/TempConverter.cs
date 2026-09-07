#nullable enable

using System.IO;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace DLCore.Utility.Takana
{
	public class TempConverter : HierarchySystem<TempConverter>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, info =>
			{
				if (info is not null)
				{
					var theChart = TakanaToDLConverter.Convert(info.Chart);
					File.WriteAllText("G:/temp.json", theChart.GetSerializationToken().ToString());
					Debug.Log("Temp converter done");
				}
			})
		};

		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
	}
}