#nullable enable

using MusicGame.Gameplay.Performance;
using MusicGame.Gameplay.Stage;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Static;

namespace MusicGame.Utility
{
	public class DebugPanel : T3MonoBehaviour
	{
		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(ISingleton<PerformanceSetting>.Instance.UseDebugPanel,
				value => gameObject.SetActive(value))
		};
	}
}