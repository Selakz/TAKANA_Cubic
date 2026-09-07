#nullable enable

using MusicGame.Models.Note;

namespace DLCore.Models.Note
{
	public interface IDLNote : INote
	{
		public ColorVariant Color { get; set; }
	}
}