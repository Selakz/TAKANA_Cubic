#nullable enable

using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.VContainer;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.EditPanel
{
	public class EditPanelUI : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private TMP_Text title = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DatasetRegistrar<ChartComponent>(dataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataAddedOrRemoved,
				_ =>
				{
					UniTask.DelayFrame(1).ContinueWith(() =>
					{
						title.text = I18NSystem.GetText("Edit_SelectedCount", dataset.Count.ToString());
					});
				})
		};

		// Private
		private ChartSelectDataset dataset = default!;

		// Constructor
		[Inject]
		private void Construct(ChartSelectDataset dataset)
		{
			this.dataset = dataset;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}