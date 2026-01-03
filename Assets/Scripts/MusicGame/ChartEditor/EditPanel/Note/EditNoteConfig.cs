using UnityEngine;

namespace MusicGame.ChartEditor.EditPanel
{
	[CreateAssetMenu(fileName = "EditNoteConfig", menuName = "ScriptableObjects/EditNoteConfig", order = 0)]
	public class EditNoteConfig : ScriptableObject
	{
		[SerializeField] private Sprite realNoteImage;
		[SerializeField] private Sprite dummyNoteImage;
		[SerializeField] private Sprite tapImage;
		[SerializeField] private Sprite slideImage;
		[SerializeField] private Sprite holdImage;

		public Sprite RealNoteImage => realNoteImage;

		public Sprite DummyNoteImage => dummyNoteImage;

		public Sprite TapImage => tapImage;

		public Sprite SlideImage => slideImage;

		public Sprite HoldImage => holdImage;
	}
}