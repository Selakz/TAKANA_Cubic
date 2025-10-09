using MusicGame.ChartEditor.EditingComponents;
using MusicGame.Components.Notes;
using MusicGame.Components.Tracks;
using T3Framework.Runtime;

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

	public abstract EditingNote Clone(T3Time newTime, ITrack newTrack);
}