using MusicGame.ChartEditor.EditingComponents;
using MusicGame.Components.Notes;

public abstract class EditingNote : EditingComponent
{
	// Serializable and Public
	public BaseNote Note => Component as BaseNote;

	// Private

	// Static

	// Defined Functions
	protected EditingNote(BaseNote baseNote) : base(baseNote)
	{
	}
}